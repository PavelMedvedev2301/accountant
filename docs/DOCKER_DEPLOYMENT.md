# 🐳 Docker Deployment Guide

Complete guide for deploying Trial Balance Classifier with Docker.

## 🚀 Quick Start

### Prerequisites

- Docker Desktop installed (Windows/Mac/Linux)
- At least 2GB free disk space
- Port 5000 available

### 1. Build and Start

```bash
# Build and start the container
docker-compose up -d --build

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

### 2. Access the Application

**From Desktop:**
```
http://localhost:5000
```

**From Mobile (Same Wi-Fi Network):**
```
http://<YOUR_PC_IP>:5000
```

To find your PC IP:
- **Windows:** `ipconfig` → look for IPv4 Address
- **Mac/Linux:** `ifconfig` or `ip addr`

## 📋 Complete Commands

### Build & Run

```bash
# Build the Docker image
docker-compose build

# Start in detached mode
docker-compose up -d

# Start with logs visible
docker-compose up

# Rebuild and start (after code changes)
docker-compose up -d --build
```

### Management

```bash
# Stop the container
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop and remove containers + volumes (clears all data)
docker-compose down -v

# Restart the container
docker-compose restart

# View logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View resource usage
docker stats tbx-classifier
```

### Container Access

```bash
# Access container shell
docker exec -it tbx-classifier /bin/bash

# View files inside container
docker exec tbx-classifier ls -la /app

# Check memory directory
docker exec tbx-classifier ls -la /app/data
```

## 📁 Data Persistence

### Volume Mounts

The application uses Docker volumes for persistent storage:

```yaml
volumes:
  - ./data:/app/data      # Memory mappings per client
  - ./logs:/app/logs      # Application logs
```

### Directory Structure

```
C:\Dev\accountant\
├── data/                      # Persisted data (created automatically)
│   └── ACME_memory.csv       # Client memory mappings
├── logs/                      # Application logs (created automatically)
│   └── tbx-20250102.log
├── SampleData/
│   ├── TB_Previous.csv
│   └── TB_Current.csv
└── docker-compose.yml
```

### Backup Memory Data

```bash
# Backup all memory files
cp -r ./data ./data_backup_$(date +%Y%m%d)

# Windows PowerShell
Copy-Item -Path .\data -Destination ".\data_backup_$(Get-Date -Format 'yyyyMMdd')" -Recurse
```

## 🔧 Configuration

### Environment Variables

Edit `docker-compose.yml` to customize:

```yaml
environment:
  - DOTNET_ENVIRONMENT=Development
  - PORT=5000
  - TZ=UTC
```

### Change Port

To use a different port (e.g., 8080):

```yaml
ports:
  - "8080:5000"  # External:Internal
```

Then access at: `http://localhost:8080`

### API Key

API key is configured in `appsettings.json`:

```json
{
  "ApiKey": "tbx-dev-key-12345"
}
```

**For production:** Change this to a strong random key!

## 🌐 Network Access

### Allow Firewall Access (Windows)

```powershell
# Allow Docker traffic
New-NetFirewallRule -DisplayName "Docker TBX" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

### Mobile Access Setup

1. Ensure PC and mobile are on **same Wi-Fi network**
2. Find your PC's IP address:
   - Windows: `ipconfig`
   - Mac: System Preferences → Network
   - Linux: `ip addr`
3. On mobile browser, navigate to: `http://<PC_IP>:5000`

Example: `http://192.168.1.100:5000`

## 🧪 Testing

### Using Sample Data

```bash
# Copy sample files to a test directory
mkdir test_upload
cp SampleData/TB_Previous.csv test_upload/
cp SampleData/TB_Current.csv test_upload/

# Access UI and upload files
# Navigate to http://localhost:5000
```

### API Testing with curl

```bash
# Test classify endpoint
curl -X POST http://localhost:5000/classify \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -F "prev=@SampleData/TB_Previous.csv" \
  -F "curr=@SampleData/TB_Current.csv" \
  -F "client_id=TEST"

# Get memory for client
curl -X GET http://localhost:5000/memory/TEST \
  -H "Authorization: ApiKey tbx-dev-key-12345"

# Get configuration
curl -X GET http://localhost:5000/config \
  -H "Authorization: ApiKey tbx-dev-key-12345"
```

### API Testing with PowerShell

```powershell
# Test classify endpoint
$headers = @{
    "Authorization" = "ApiKey tbx-dev-key-12345"
}

$form = @{
    prev = Get-Item -Path "SampleData\TB_Previous.csv"
    curr = Get-Item -Path "SampleData\TB_Current.csv"
    client_id = "TEST"
}

Invoke-RestMethod -Uri "http://localhost:5000/classify" -Method Post -Headers $headers -Form $form
```

## 🔍 Troubleshooting

### Container Won't Start

```bash
# Check if port is already in use
netstat -ano | findstr :5000

# Check Docker logs
docker-compose logs

# Check Docker daemon status
docker ps
```

### Can't Access from Mobile

1. **Check firewall:** Ensure port 5000 is open
2. **Check network:** Verify same Wi-Fi network
3. **Check IP:** Confirm correct PC IP address
4. **Test connectivity:** 
   ```bash
   # From PC
   curl http://localhost:5000
   
   # From mobile browser
   http://<PC_IP>:5000
   ```

### Memory Not Persisting

```bash
# Check volume mounts
docker inspect tbx-classifier | grep Mounts -A 20

# Verify data directory exists
ls -la ./data

# Check permissions
docker exec tbx-classifier ls -la /app/data
```

### High Memory Usage

```bash
# Check resource usage
docker stats tbx-classifier

# Restart container
docker-compose restart

# Limit resources in docker-compose.yml
deploy:
  resources:
    limits:
      cpus: '1.0'
      memory: 1G
```

## 📊 Performance

### Expected Performance

- Startup time: 2-5 seconds
- 1,000 accounts: < 1 second
- 5,000 accounts: 2-5 seconds
- 10,000 accounts: 5-10 seconds

### Monitoring

```bash
# Real-time resource monitoring
docker stats tbx-classifier

# Check logs for performance
docker-compose logs | grep "Completed in"
```

## 🔐 Security Best Practices

### Production Deployment

1. **Change API Key:**
   ```json
   "ApiKey": "your-strong-random-key-here"
   ```

2. **Enable HTTPS:**
   - Use a reverse proxy (nginx, Traefik)
   - Add SSL certificate

3. **Restrict Network Access:**
   ```yaml
   ports:
     - "127.0.0.1:5000:5000"  # Localhost only
   ```

4. **Regular Backups:**
   ```bash
   # Automated backup script
   tar -czf backup_$(date +%Y%m%d).tar.gz ./data
   ```

## 🆙 Updates

### Update to Latest Version

```bash
# Pull latest code
git pull

# Rebuild and restart
docker-compose down
docker-compose up -d --build

# Verify
docker-compose logs -f
```

### Clean Rebuild

```bash
# Remove everything and rebuild
docker-compose down -v
docker system prune -a
docker-compose up -d --build
```

## 📝 Maintenance

### View Logs

```bash
# All logs
docker-compose logs

# Last 100 lines
docker-compose logs --tail=100

# Follow logs
docker-compose logs -f

# Logs from specific time
docker-compose logs --since 2024-10-02T10:00:00
```

### Clear Logs

```bash
# Truncate log files
truncate -s 0 logs/*.log

# Windows PowerShell
Clear-Content -Path .\logs\*.log
```

### Database Maintenance

```bash
# Check memory file size
ls -lh data/*.csv

# Backup before cleanup
cp data/ACME_memory.csv data/ACME_memory_backup.csv

# Clean old mappings (manual edit)
# Remove outdated entries from CSV files
```

## 🎯 Production Checklist

- [ ] Changed API key from default
- [ ] Configured firewall rules
- [ ] Set up automated backups
- [ ] Tested mobile access
- [ ] Configured log rotation
- [ ] Set resource limits
- [ ] Tested disaster recovery
- [ ] Documented custom configurations

## 📞 Support

For issues:
1. Check logs: `docker-compose logs`
2. Verify configuration: `config.yaml`
3. Test API: `curl http://localhost:5000/config`
4. Review documentation: `README.md`

---

**Happy Classifying! ⚽**

