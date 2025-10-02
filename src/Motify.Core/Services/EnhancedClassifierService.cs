using System.Text.Json;
using FuzzySharp;
using Motify.Core.Models;

namespace Motify.Core.Services;

public class EnhancedClassifierService
{
    private readonly MemoryService _memoryService;
    private readonly ConfigService _configService;
    private readonly ClassificationConfig _config;

    public EnhancedClassifierService(MemoryService memoryService, ConfigService configService)
    {
        _memoryService = memoryService;
        _configService = configService;
        _config = _configService.LoadConfig();
    }

    public List<ClassificationResult> ClassifyAccounts(
        List<TrialBalanceAccount> previous,
        List<TrialBalanceAccount> current,
        string clientId)
    {
        var results = new List<ClassificationResult>();
        var previousByCode = previous.ToDictionary(a => a.account_code, a => a);

        foreach (var currentAccount in current)
        {
            // Skip accounts that exist unchanged
            if (previousByCode.ContainsKey(currentAccount.account_code))
            {
                continue;
            }

            var result = ClassifyAccount(currentAccount, previous, previousByCode, clientId);
            results.Add(result);
        }

        return results;
    }

    private ClassificationResult ClassifyAccount(
        TrialBalanceAccount account,
        List<TrialBalanceAccount> previousAccounts,
        Dictionary<string, TrialBalanceAccount> previousByCode,
        string clientId)
    {
        var nameNorm = NormalizeName(account.account_name);
        var parentNorm = !string.IsNullOrEmpty(account.parent_code) 
            ? NormalizeName(GetParentName(account.parent_code, previousByCode))
            : null;

        // Check for renumbering
        var renumberedFrom = FindRenumberedAccount(account, previousAccounts);

        // Classify and get category suggestion
        var (category, confidence, evidenceList) = ClassifyWithConfidence(
            nameNorm, parentNorm, account.account_code, clientId);

        var result = new ClassificationResult
        {
            account_code = account.account_code,
            account_name = account.account_name,
            parent_code = account.parent_code,
            status = renumberedFrom != null ? "likely_renumbered" : "new",
            suggested_category = category,
            confidence = confidence,
            needs_review = confidence < _config.Thresholds.NeedsReviewBelow,
            renumbered_from_code = renumberedFrom?.account_code,
            renumbered_from_name = renumberedFrom?.account_name,
            evidence = JsonSerializer.Serialize(evidenceList)
        };

        return result;
    }

    private (string? category, int confidence, List<Evidence> evidence) ClassifyWithConfidence(
        string nameNorm, 
        string? parentNorm, 
        string accountCode,
        string clientId)
    {
        var evidenceList = new List<Evidence>();
        double totalScore = 0;
        string? bestCategory = null;

        // 1. Check Memory
        var memoryMatch = _memoryService.FindMatch(clientId, nameNorm, parentNorm);
        if (memoryMatch != null)
        {
            evidenceList.Add(new Evidence
            {
                method = "memory",
                score = _config.Thresholds.MemoryExactMatch,
                matched_name = nameNorm,
                matched_parent = parentNorm,
                category = memoryMatch.category
            });
            totalScore += _config.Weights.Memory * _config.Thresholds.MemoryExactMatch;
            bestCategory = memoryMatch.category;
        }

        // 2. Check Keywords
        var keywordMatch = FindKeywordMatch(nameNorm);
        if (keywordMatch != null)
        {
            evidenceList.Add(new Evidence
            {
                method = "keyword",
                score = _config.Thresholds.KeywordMatch,
                matched_keyword = keywordMatch.Value.keyword,
                category = keywordMatch.Value.category
            });
            totalScore += _config.Weights.Keyword * _config.Thresholds.KeywordMatch;
            if (bestCategory == null) bestCategory = keywordMatch.Value.category;
        }

        // 3. Check Parent Code (Ontology)
        var parentMatch = FindParentMatch(accountCode);
        if (parentMatch != null)
        {
            evidenceList.Add(new Evidence
            {
                method = "parent",
                score = _config.Thresholds.ParentMatch,
                matched_parent = accountCode,
                category = parentMatch
            });
            totalScore += _config.Weights.Parent * _config.Thresholds.ParentMatch;
            if (bestCategory == null) bestCategory = parentMatch;
        }

        // 4. Fuzzy Match against Memory (all clients for common patterns)
        var fuzzyMatch = FindFuzzyMemoryMatch(nameNorm, clientId);
        if (fuzzyMatch != null)
        {
            evidenceList.Add(new Evidence
            {
                method = "fuzzy",
                score = fuzzyMatch.Value.score,
                matched_name = fuzzyMatch.Value.matchedName,
                category = fuzzyMatch.Value.category
            });
            totalScore += _config.Weights.Fuzzy * fuzzyMatch.Value.score;
            if (bestCategory == null) bestCategory = fuzzyMatch.Value.category;
        }

        // If no match, return uncategorized
        if (bestCategory == null)
        {
            bestCategory = "Uncategorized";
            evidenceList.Add(new Evidence
            {
                method = "uncategorized",
                score = 0,
                category = "Uncategorized"
            });
        }

        // Calculate final confidence (0-100)
        var confidence = Math.Min(100, (int)(totalScore * 100));

        return (bestCategory, confidence, evidenceList);
    }

    private (string keyword, string category)? FindKeywordMatch(string nameNorm)
    {
        foreach (var kvp in _config.Keywords)
        {
            foreach (var keyword in kvp.Value)
            {
                var keywordNorm = NormalizeName(keyword);
                if (nameNorm.Contains(keywordNorm))
                {
                    return (keyword, kvp.Key);
                }
            }
        }
        return null;
    }

    private string? FindParentMatch(string accountCode)
    {
        // Try progressively shorter prefixes
        for (int len = accountCode.Length; len > 0; len--)
        {
            var prefix = accountCode.Substring(0, len);
            if (_config.Ontology.TryGetValue(prefix, out var category))
            {
                return category;
            }
        }
        return null;
    }

    private (string matchedName, string category, double score)? FindFuzzyMemoryMatch(
        string nameNorm, 
        string clientId)
    {
        var memory = _memoryService.LoadMemory(clientId);
        
        string? bestMatch = null;
        string? bestCategory = null;
        double bestScore = 0;

        foreach (var mapping in memory)
        {
            var score = Fuzz.Ratio(nameNorm, mapping.name_norm) / 100.0;
            
            if (score > bestScore && score >= _config.Thresholds.FuzzyMatch)
            {
                bestScore = score;
                bestMatch = mapping.name_norm;
                bestCategory = mapping.category;
            }
        }

        if (bestMatch != null && bestCategory != null)
        {
            return (bestMatch, bestCategory, bestScore);
        }

        return null;
    }

    private TrialBalanceAccount? FindRenumberedAccount(
        TrialBalanceAccount currentAccount,
        List<TrialBalanceAccount> previousAccounts)
    {
        var normalizedCurrentName = NormalizeName(currentAccount.account_name);
        
        TrialBalanceAccount? bestMatch = null;
        double bestScore = 0.0;

        foreach (var prevAccount in previousAccounts)
        {
            var normalizedPrevName = NormalizeName(prevAccount.account_name);

            var jaroWinklerScore = Fuzz.Ratio(normalizedCurrentName, normalizedPrevName) / 100.0;
            var tokenSortScore = Fuzz.TokenSortRatio(normalizedCurrentName, normalizedPrevName) / 100.0;
            var score = Math.Max(jaroWinklerScore, tokenSortScore);

            if (score > bestScore && score >= _config.Thresholds.RenumberedSimilarity)
            {
                bestScore = score;
                bestMatch = prevAccount;
            }
        }

        return bestMatch;
    }

    private string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var normalized = name.ToLower();
        var charsToRemove = new[] { '-', '_', '/', '(', ')', '.', ',', '"', ' ' };
        foreach (var c in charsToRemove)
        {
            normalized = normalized.Replace(c.ToString(), string.Empty);
        }

        return normalized;
    }

    private string GetParentName(string parentCode, Dictionary<string, TrialBalanceAccount> accounts)
    {
        return accounts.TryGetValue(parentCode, out var parent) 
            ? parent.account_name 
            : string.Empty;
    }
}

