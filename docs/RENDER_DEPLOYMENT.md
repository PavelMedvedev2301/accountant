# 🚀 Render.com Deployment Guide

Complete guide for deploying Trial Balance Classifier to Render.com.

## ✅ Prerequisites

- [x] GitHub account with repository: `https://github.com/PavelMedvedev2301/accountant`
- [x] Render.com account (sign up with GitHub)
- [x] Docker configuration ready (Dockerfile, docker-compose.yml)
- [x] Application tested locally

## 📋 Deployment Steps

### 1️⃣ Connect GitHub Repository

1. Log in to [Render.com](https://render.com) with your GitHub account
2. Click **"New"** → **"Web Service"**
3. Click **"Connect GitHub"**
4. Select repository: **`PavelMedvedev2301/accountant`**
5. Click **"Connect"**

### 2️⃣ Configure Web Service

Fill in the following settings:

| Setting | Value | Notes |
|---------|-------|-------|
| **Name** | `tb-classifier` | Or any name you prefer |
| **Environment** | **Docker** | ⚠️ Important! Must be Docker |
| **Region** | Choose closest | e.g., Oregon (US West) |
| **Branch** | `main` | Or your default branch |
| **Root Directory** | Leave empty | Uses repo root |
| **Build Command** | Leave empty | Dockerfile handles build |
| **Start Command** | Leave empty | ENTRYPOINT in Dockerfile |

### 3️⃣ Configure Environment Variables

Add these environment variables in Render dashboard:

| Key | Value | Required |
|-----|-------|----------|
| `PORT` | `5000` | Yes |
| `DOTNET_ENVIRONMENT` | `Production` | Yes |
| `API_KEY` | `your-secure-api-key-here` | Yes |
| `TZ` | `UTC` | Optional |

**⚠️ Security:** Generate a strong API key! Use a password generator or:

```bash
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})

# Linux/Mac
openssl rand -base64 32
```

### 4️⃣ Configure Persistent Storage (Recommended)

To persist memory and logs between deployments:

1. In Render dashboard, go to your service
2. Click **"Disks"** tab
3. Click **"Add Disk"**
4. Configure:
   - **Name:** `tbx-data`
   - **Mount Path:** `/app/data`
   - **Size:** 1 GB (sufficient for most use cases)
5. Click **"Save"**

**Optional - Logs Disk:**
- **Name:** `tbx-logs`
- **Mount Path:** `/app/logs`
- **Size:** 500 MB

### 5️⃣ Deploy

1. Review all settings
2. Click **"Create Web Service"**
3. Render will:
   - Clone your GitHub repository
   - Build Docker image using your Dockerfile
   - Deploy the container
   - Assign a public URL

**Deployment time:** 3-5 minutes for first build

### 6️⃣ Access Your Application

After successful deployment, Render provides a URL:

```
https://tb-classifier.onrender.com
```

**Test the application:**

1. **Web UI:** 
   ```
   https://tb-classifier.onrender.com
   ```

2. **API Health Check:**
   ```bash
   curl https://tb-classifier.onrender.com/config \
     -H "Authorization: ApiKey your-secure-api-key-here"
   ```

3. **Test Classification:**
   ```bash
   curl -X POST https://tb-classifier.onrender.com/classify \
     -H "Authorization: ApiKey your-secure-api-key-here" \
     -F "prev=@TB_Previous.csv" \
     -F "curr=@TB_Current.csv" \
     -F "client_id=TEST"
   ```

## 🔄 Continuous Deployment

Render automatically redeploys when you push to GitHub:

```bash
# Make changes locally
git add .
git commit -m "Update classifier logic"
git push origin main

# Render automatically:
# - Detects the push
# - Rebuilds Docker image
# - Deploys new version
# - Zero-downtime deployment
```

**Monitor deployment:**
- Go to Render dashboard
- Click on your service
- View **"Events"** tab for deployment status
- View **"Logs"** tab for application logs

## 📊 Monitoring & Logs

### View Live Logs

In Render dashboard:
1. Click on your service
2. Go to **"Logs"** tab
3. See real-time application logs

### Metrics

Render provides:
- CPU usage
- Memory usage
- Request count
- Response times
- Deployment history

### Alerts

Set up alerts for:
- Service down
- High memory usage
- Failed deployments
- Error rates

## 🔐 Security Configuration

### Update API Key

**Never use the default API key in production!**

1. Generate secure API key (see step 3️⃣)
2. Add to Render environment variables
3. Update in `appsettings.json` is overridden by env var

### Enable HTTPS (Automatic)

Render automatically provides:
- Free SSL certificate
- HTTPS by default
- Auto-renewal
- HTTP → HTTPS redirect

### API Key Rotation

To rotate API key:
1. Generate new key
2. Update Render environment variable: `API_KEY`
3. Click **"Save Changes"**
4. Service auto-redeploys
5. Update API key in client applications

## 💾 Backup & Recovery

### Backup Data

Download data from persistent disk:

```bash
# SSH into Render shell (if enabled)
render shell tb-classifier

# Or use Render CLI
render disk backup tbx-data
```

**Manual backup via API:**

```bash
# Download all memory for a client
curl -X GET https://tb-classifier.onrender.com/memory/ACME \
  -H "Authorization: ApiKey your-key" > backup_ACME.json
```

### Restore Data

1. Download backup
2. Upload via memory update API
3. Or manually edit disk via Render shell

## 🔧 Troubleshooting

### Deployment Fails

**Check build logs:**
1. Go to Render dashboard
2. Click service → **"Events"**
3. Look for error messages

**Common issues:**
- Missing dependencies in `TrialBalanceClassifier.csproj`
- Invalid Dockerfile syntax
- Port mismatch

### Service Not Responding

**Check service status:**
1. Render dashboard → Service status
2. View logs for errors
3. Check environment variables

**Common issues:**
- API key mismatch
- Port not set to 5000
- Memory/CPU limits exceeded

### Memory Full

**Solution:**
1. Increase disk size in Render
2. Clean up old memory files
3. Implement data retention policy

## 📈 Scaling

### Free Tier

- CPU: 0.5 vCPU
- RAM: 512 MB
- Disk: 1 GB (paid)
- **Limitation:** Spins down after 15 min of inactivity

### Paid Tiers

Upgrade for:
- No spin-down
- More CPU/RAM
- Larger disk
- Priority support

### Performance Tips

1. **Optimize Docker image:**
   - Multi-stage build (already implemented)
   - Minimal base image
   - Cache dependencies

2. **Memory management:**
   - Regular cleanup of old mappings
   - Limit memory per client
   - Archive old data

3. **Configuration:**
   - Adjust confidence thresholds
   - Optimize fuzzy matching
   - Cache frequently used data

## 🌍 Custom Domain

### Add Custom Domain

1. Purchase domain (e.g., `tbclassifier.com`)
2. In Render dashboard:
   - Click service → **"Settings"**
   - Scroll to **"Custom Domains"**
   - Click **"Add Custom Domain"**
   - Enter domain
   - Update DNS records as instructed
3. Render provisions SSL automatically

### DNS Configuration

Add these records to your DNS provider:

```
Type: CNAME
Name: www
Value: tb-classifier.onrender.com

Type: A
Name: @
Value: [Render provides IP]
```

## 📱 Mobile Access

Your Render URL is accessible from:
- Desktop browsers
- Mobile browsers (iOS/Android)
- Mobile apps via API
- ChatGPT plugins (future)

**No additional configuration needed!**

## 🔄 Rollback

If deployment fails or has issues:

1. Go to Render dashboard
2. Click service → **"Events"**
3. Find previous successful deployment
4. Click **"Rollback to this deploy"**

Or rollback via Git:

```bash
# Revert to previous commit
git revert HEAD
git push origin main

# Render auto-deploys previous version
```

## 🧪 Testing Before Deployment

**Always test locally before pushing:**

```bash
# Test with Docker locally
docker-compose up --build

# Verify UI
http://localhost:5000

# Test API
curl http://localhost:5000/config

# Test classification
# Upload files via UI or use API
```

**Staging environment (optional):**
- Create separate Render service for staging
- Connect to `develop` branch
- Test before merging to `main`

## 📝 Deployment Checklist

Before deploying to production:

- [ ] Changed API key from default
- [ ] Tested locally with Docker
- [ ] Verified all endpoints work
- [ ] Configured persistent disk for data
- [ ] Set environment variables correctly
- [ ] Updated README with production URL
- [ ] Set up monitoring and alerts
- [ ] Tested mobile access
- [ ] Verified SSL certificate
- [ ] Documented backup procedure
- [ ] Tested rollback procedure

## 🆘 Support

### Render Support

- Documentation: https://render.com/docs
- Community: https://community.render.com
- Support: support@render.com (paid plans)

### Application Issues

- Check application logs in Render
- Review `config.yaml` configuration
- Test API endpoints
- Verify data directory permissions

## 💰 Cost Estimate

### Free Tier
- Web service: Free (with limitations)
- Disk: $0.25/GB/month
- **Total:** ~$0.25/month for 1GB disk

### Starter Plan
- Web service: $7/month
- Disk: $0.25/GB/month
- No spin-down
- **Total:** ~$7.25/month

### Pro Plan
- Web service: $25/month
- More resources
- Priority support
- **Total:** $25+/month

## 🎯 Post-Deployment

After successful deployment:

1. **Test thoroughly:**
   - Upload sample files
   - Test all features
   - Verify mobile access
   - Check API endpoints

2. **Update documentation:**
   - Add production URL to README
   - Update API examples
   - Document any custom configuration

3. **Monitor:**
   - Check logs regularly
   - Monitor resource usage
   - Set up alerts

4. **Backup:**
   - Schedule regular backups
   - Test restore procedure
   - Document backup location

5. **Share:**
   - Share URL with team
   - Provide API documentation
   - Train users on UI

---

## 🚀 Ready to Deploy!

Your application is ready for Render.com deployment. Follow the steps above and you'll be live in minutes!

**Your deployment URL will be:**
```
https://tb-classifier.onrender.com
```

**Questions?**
- Check Render.com documentation
- Review application logs
- Test locally first
- Use staging environment

**Happy deploying! ⚽**

