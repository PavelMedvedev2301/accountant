# 🎮 MCP Setup Guide - Render Integration

This guide explains how to use the Render MCP Server with Cursor for deploying Motify.

## 📋 What is MCP?

**Model Context Protocol (MCP)** allows AI assistants like Claude/Cursor to interact with external services through standardized tools. The Render MCP Server gives you:

- 🚀 Deploy services with natural language
- 📊 Monitor logs and metrics
- 🔧 Update configurations
- 🔄 Trigger redeployments
- 📈 Scale services

## 🔧 Configuration

### 1. Create MCP Config File

The config is already set up in `.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "render": {
      "command": "npx",
      "args": ["-y", "@render/mcp-server"],
      "env": {
        "RENDER_API_KEY": "your_api_key_here"
      }
    }
  }
}
```

### 2. Get Your Render API Key

1. Go to https://dashboard.render.com/u/settings#api-keys
2. Click **"Create API Key"**
3. Copy the key (starts with `rnd_`)
4. Add it to `.cursor/mcp.json`

### 3. Restart Cursor

- Close and reopen Cursor
- The Render MCP server will be available

## 🎯 Available Tools

### Deployment

**create_service** - Create new service
```
Create a Render service called "motify-classifier" using:
- Repo: PavelMedvedev2301/accountant
- Branch: main
- Type: web_service
- Docker: use Dockerfile
- Region: oregon
- Port: 5000
- Env: PORT=5000, API_KEY=motify-secure-key
```

**deploy_service** - Trigger deployment
```
Deploy the latest changes to motify-classifier
```

**list_services** - List all services
```
Show me all my Render services
```

**get_service** - Get service details
```
Get details for motify-classifier service
```

### Monitoring

**query_logs** - View logs
```
Show me the latest logs for motify-classifier
```

**get_metrics** - Performance metrics
```
Get CPU and memory metrics for motify-classifier from the last hour
```

### Management

**update_service** - Update configuration
```
Update motify-classifier environment variable API_KEY to "new-secure-key"
```

**restart_service** - Restart service
```
Restart the motify-classifier service
```

**scale_service** - Scale instances
```
Scale motify-classifier to 2 instances
```

## 🚀 Usage Examples

### Deploy Motify for the First Time

**In Cursor Chat:**
```
Create a new Render web service with these specs:
- Name: motify-classifier
- GitHub repo: PavelMedvedev2301/accountant
- Branch: main
- Build: Use Dockerfile
- Region: Oregon
- Environment variables:
  * PORT=5000
  * DOTNET_ENVIRONMENT=Production
  * API_KEY=motify-prod-key-2025
```

### Check Deployment Status

**In Cursor Chat:**
```
Show me the status and URL for motify-classifier
```

### View Logs

**In Cursor Chat:**
```
Show me the last 50 log entries for motify-classifier
```

### Monitor Performance

**In Cursor Chat:**
```
Get CPU and memory usage for motify-classifier from the last 2 hours
```

### Update Environment Variables

**In Cursor Chat:**
```
Update the API_KEY environment variable for motify-classifier 
to "motify-new-secure-key-2025"
```

### Redeploy After Push

**In Cursor Chat:**
```
Trigger a new deployment for motify-classifier to deploy the latest changes
```

## 🔄 Workflow

### Typical Deployment Flow

1. **Make code changes locally**
   ```bash
   git add .
   git commit -m "Add new feature"
   git push origin main
   ```

2. **Ask Cursor to deploy**
   ```
   Deploy the latest changes to motify-classifier
   ```

3. **Monitor deployment**
   ```
   Show me the deployment logs for motify-classifier
   ```

4. **Check service health**
   ```
   Get the current status and metrics for motify-classifier
   ```

## 🔐 Security Best Practices

### 1. Protect Your API Key

✅ **DO:**
- Store API key in `.cursor/mcp.json`
- Add `.cursor/mcp.json` to `.gitignore`
- Use example file for sharing: `mcp.json.example`
- Rotate keys regularly

❌ **DON'T:**
- Commit API keys to Git
- Share API keys in chat logs
- Use same key for multiple environments

### 2. Environment-Specific Keys

Create separate API keys for:
- Development
- Staging
- Production

### 3. Key Rotation

```
1. Create new API key in Render dashboard
2. Update .cursor/mcp.json with new key
3. Restart Cursor
4. Delete old API key
```

## 🐛 Troubleshooting

### MCP Server Not Found

**Problem:** "Render MCP server not available"

**Solution:**
1. Ensure Node.js is installed: `node --version`
2. Check internet connection
3. Restart Cursor
4. Try manual install: `npm install -g @render/mcp-server`

### API Key Invalid

**Problem:** "Authentication failed"

**Solution:**
1. Verify API key in Render dashboard
2. Check for extra spaces in `.cursor/mcp.json`
3. Ensure key starts with `rnd_`
4. Generate new key if needed

### Command Timeout

**Problem:** "MCP command timed out"

**Solution:**
1. Check Render API status: https://status.render.com
2. Retry the command
3. Increase timeout in MCP config (if available)

### Service Not Found

**Problem:** "Service ID not found"

**Solution:**
1. List all services: "Show me all my Render services"
2. Verify service name/ID
3. Check if service was deleted

## 📚 Additional Resources

- **Render MCP Server**: https://github.com/render-oss/render-mcp-server
- **Render API Docs**: https://render.com/docs/api
- **MCP Protocol**: https://modelcontextprotocol.io
- **Render Dashboard**: https://dashboard.render.com

## 🎯 Quick Reference

### Common Commands

| Action | Cursor Prompt |
|--------|---------------|
| Create service | "Create Render service motify-classifier from PavelMedvedev2301/accountant" |
| Deploy | "Deploy motify-classifier" |
| View logs | "Show logs for motify-classifier" |
| Get metrics | "Get metrics for motify-classifier" |
| Update env | "Update API_KEY for motify-classifier to 'newkey'" |
| Restart | "Restart motify-classifier" |
| Scale | "Scale motify-classifier to 3 instances" |
| Check status | "What's the status of motify-classifier?" |

---

**🎉 You're all set!** Start deploying Motify with natural language commands in Cursor!

