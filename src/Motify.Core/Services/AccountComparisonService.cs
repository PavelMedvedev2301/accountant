using FuzzySharp;
using Motify.Core.Models;

namespace Motify.Core.Services;

public class AccountComparisonService
{
    private const double SimilarityThreshold = 0.9;

    public List<NewAccountResult> CompareTrialBalances(
        List<TrialBalanceAccount> previous,
        List<TrialBalanceAccount> current)
    {
        var results = new List<NewAccountResult>();

        // Create lookup dictionaries for previous accounts
        var previousByCode = previous.ToDictionary(a => a.account_code, a => a);
        var previousAccountsList = previous.ToList();

        foreach (var currentAccount in current)
        {
            // If the code exists in previous, it's not new or renumbered
            if (previousByCode.ContainsKey(currentAccount.account_code))
            {
                continue;
            }

            // Code doesn't exist in previous - it's either new or renumbered
            var normalizedCurrentName = NormalizeName(currentAccount.account_name);
            
            // Try to find a similar account name in previous
            var bestMatch = FindBestMatch(normalizedCurrentName, currentAccount.account_name, previousAccountsList);

            if (bestMatch != null)
            {
                // Likely renumbered
                results.Add(new NewAccountResult
                {
                    account_code = currentAccount.account_code,
                    account_name = currentAccount.account_name,
                    parent_code = currentAccount.parent_code,
                    status = "likely_renumbered",
                    renumbered_from_code = bestMatch.account_code,
                    renumbered_from_name = bestMatch.account_name
                });
            }
            else
            {
                // New account
                results.Add(new NewAccountResult
                {
                    account_code = currentAccount.account_code,
                    account_name = currentAccount.account_name,
                    parent_code = currentAccount.parent_code,
                    status = "new",
                    renumbered_from_code = null,
                    renumbered_from_name = null
                });
            }
        }

        return results;
    }

    private string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        // Convert to lowercase
        var normalized = name.ToLower();

        // Remove specified characters: - _ / ( ) . , "
        var charsToRemove = new[] { '-', '_', '/', '(', ')', '.', ',', '"', ' ' };
        foreach (var c in charsToRemove)
        {
            normalized = normalized.Replace(c.ToString(), string.Empty);
        }

        return normalized;
    }

    private TrialBalanceAccount? FindBestMatch(
        string normalizedCurrentName,
        string originalCurrentName,
        List<TrialBalanceAccount> previousAccounts)
    {
        TrialBalanceAccount? bestMatch = null;
        double bestScore = 0.0;

        foreach (var prevAccount in previousAccounts)
        {
            var normalizedPrevName = NormalizeName(prevAccount.account_name);

            // Calculate similarity using Jaro-Winkler (preferred for short strings)
            var jaroWinklerScore = Fuzz.Ratio(normalizedCurrentName, normalizedPrevName) / 100.0;

            // Also calculate token sort ratio for robustness
            var tokenSortScore = Fuzz.TokenSortRatio(normalizedCurrentName, normalizedPrevName) / 100.0;

            // Use the maximum of both scores
            var score = Math.Max(jaroWinklerScore, tokenSortScore);

            if (score > bestScore && score >= SimilarityThreshold)
            {
                bestScore = score;
                bestMatch = prevAccount;
            }
        }

        return bestMatch;
    }
}



