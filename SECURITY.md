# Security Policy

## 🔐 Security Best Practices

This document outlines security practices for the Motify project.

## Sensitive Files Protection

### ✅ Files Properly Ignored (Not Committed)

These files contain sensitive data and are properly excluded via `.gitignore`:

- `.cursor/mcp.json` - Contains Render API key
- `.env`, `.env.*` - Environment variables with secrets
- `data/` - Client memory files (may contain business data)
- `logs/` - Application logs (may contain sensitive info)
- `*.key`, `*.pem`, `*.pfx` - Certificate and key files
- `appsettings.Development.json` - Development settings

### ⚠️ Files to Review Before Commit

Always check these files before committing:

- `appsettings.json` - Should only contain defaults, no secrets
- `config/config.yaml` - Should only contain non-sensitive defaults
- `docker-compose.yml` - Use `${VAR}` syntax for secrets
- Source code - No hardcoded passwords or API keys

## API Keys Management

### Render API Key

**Location:** `.cursor/mcp.json` (local only)

**Format:**
```json
{
  "mcpServers": {
    "render": {
      "env": {
        "RENDER_API_KEY": "rnd_xxxxxxxxxxxxx"
      }
    }
  }
}
```

**Security:**
- ✅ Never commit to Git
- ✅ Rotate every 90 days
- ✅ Revoke immediately if exposed
- ✅ Use separate keys for dev/prod

### Application API Key

**Location:** Environment variables or Render dashboard

**Configuration:**
```bash
# .env (local)
API_KEY=motify-secure-key-2025

# Render dashboard
Environment Variables → API_KEY
```

**Security:**
- ✅ Generate strong random keys (32+ characters)
- ✅ Rotate regularly
- ✅ Never log or display in UI
- ✅ Use different keys per environment

## Environment Variables

### Local Development

1. Copy `.env.example` to `.env`
2. Fill in actual values
3. Never commit `.env` file

### Production (Render)

1. Set environment variables in Render dashboard
2. Use Render's environment variable encryption
3. Don't set secrets in `docker-compose.yml`

## Git Security Audit

### Pre-Commit Checklist

Before every commit, verify:

- [ ] No API keys in code
- [ ] No passwords or tokens
- [ ] No sensitive customer data
- [ ] No connection strings with credentials
- [ ] `.gitignore` is up to date
- [ ] Only necessary files committed

### Automated Checks

```bash
# Check for potential secrets
git grep -i "api_key\|password\|secret\|token" -- '*.cs' '*.json' '*.yaml'

# Check what's staged
git diff --staged

# Verify .gitignore is working
git status --ignored
```

## Incident Response

### If API Key is Exposed

**Immediate Actions:**
1. ✅ Revoke the exposed key in Render dashboard
2. ✅ Generate new API key
3. ✅ Update `.cursor/mcp.json` locally
4. ✅ Update production environment variables
5. ✅ Review access logs for suspicious activity

### If Committed by Mistake

```bash
# If not pushed yet
git reset HEAD~1
git checkout -- file-with-secret

# If already pushed (more complex)
# Contact team lead immediately
# May require git history rewrite
```

## Dependency Security

### Regular Updates

```bash
# Check for vulnerabilities
dotnet list package --vulnerable

# Update dependencies
dotnet outdated
```

### Trusted Sources

- ✅ Use official NuGet packages only
- ✅ Verify package authenticity
- ✅ Review package before installing
- ✅ Pin versions in production

## Access Control

### GitHub Repository

- Limit write access to core team
- Require PR reviews for main branch
- Enable branch protection
- Use signed commits (optional)

### Render Account

- Use SSO if available
- Enable 2FA for all team members
- Limit production access
- Audit access logs regularly

## Data Protection

### Client Data (`data/` folder)

- Contains learned account mappings
- May include client-specific patterns
- Must be backed up securely
- Should be encrypted if sensitive

### Logs (`logs/` folder)

- May contain request details
- Should not log sensitive data
- Rotate regularly
- Monitor for suspicious activity

## Reporting Security Issues

### Internal

Contact: [Your Security Team]

### External

If you discover a security vulnerability:

1. **Do NOT** create a public GitHub issue
2. Email: [security@yourcompany.com]
3. Include:
   - Description of vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

### Bug Bounty

We currently do not have a bug bounty program.

## Security Changelog

### 2025-10-02
- ✅ Initial security documentation
- ✅ Enhanced `.gitignore` for secrets
- ✅ Added `.env.example` template
- ✅ MCP config properly secured

---

## Quick Security Checklist

Daily:
- [ ] Review commit history for accidents
- [ ] Check for unusual activity in logs

Weekly:
- [ ] Rotate API keys (if suspicious activity)
- [ ] Review access logs
- [ ] Check for dependency updates

Monthly:
- [ ] Security audit of codebase
- [ ] Review team access
- [ ] Update documentation

Quarterly:
- [ ] Full security review
- [ ] Penetration testing (if applicable)
- [ ] Rotate all API keys
- [ ] Update dependencies

---

**Last Updated:** 2025-10-02  
**Next Review:** 2025-11-02

