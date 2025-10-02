# Trial Balance Classifier (TBX) v2.0

A comprehensive command-line and web-based tool for detecting new and renumbered accounts between trial balance files, with advanced classification using memory, keywords, and fuzzy matching.

## ✨ Features

### Core Capabilities
- ✅ **Compare trial balance files** (previous vs current quarter)
- ✅ **Detect new accounts** (accounts that don't exist in previous period)
- ✅ **Detect likely renumbered accounts** using fuzzy string matching (≥92% similarity)
- ✅ **Intelligent classification** using memory, keywords, parent codes, and fuzzy matching
- ✅ **Confidence scoring** with configurable thresholds
- ✅ **Memory management** per client for continuous learning
- ✅ **Fast processing** (~5000 rows in under 5 seconds)

### Enhanced Classification System
- 🧠 **Memory-based classification** - Learns from previous categorizations per client
- 🔍 **Keyword matching** - Pre-configured keywords for common account categories
- 🌳 **Parent code hierarchy** - Uses account code prefixes and ontology
- 🔮 **Fuzzy matching** - Finds similar accounts across memory
- 📊 **Confidence scoring** - Weighted aggregation of all methods
- ⚠️ **Review flags** - Automatically marks accounts needing human review

### Deployment Options
- 💻 **CLI Mode** - Command-line interface for batch processing
- 🌐 **Web UI** - Beautiful Barcelona-themed web interface
- 🐳 **Docker** - Containerized deployment for easy setup
- 📱 **Mobile-friendly** - Access from desktop or mobile devices

## 🚀 Quick Start

### Option 1: Docker (Recommended)

```bash
# Build and start
docker-compose up -d --build

# Access UI
http://localhost:5000

# From mobile (same Wi-Fi)
http://<YOUR_PC_IP>:5000
```

See [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md) for complete Docker guide.

### Option 2: Local .NET

```bash
# Restore and build
dotnet restore
dotnet build -c Release

# Run web server
dotnet run serve

# Or use CLI mode
dotnet run classify --prev TB_Previous.csv --curr TB_Current.csv --out Results.csv --client ACME
```

## 📋 Requirements

- **.NET 8.0 SDK** or later
- **Docker Desktop** (for containerized deployment)
- Windows, macOS, or Linux

## 🎨 Web UI

### Features
- 📤 **Upload trial balance files** (CSV format)
- 📊 **Interactive results table** with sorting and filtering
- ✏️ **Inline editing** of categories and review flags
- 📈 **Confidence bars** with color-coded indicators
- 🔍 **Expandable evidence** showing classification reasoning
- 💾 **Apply changes** to update memory
- ⬇️ **Download results** as CSV
- 📚 **Memory viewer** to inspect learned mappings

### Barcelona Theme
- **Blaugrana Blue** (#004D98) - Headers and primary elements
- **Blaugrana Red** (#A50044) - Action buttons
- **Blaugrana Yellow** (#FFED02) - Accents and highlights
- **Pink highlight** (#ffe5ec) - Accounts needing review

### Screenshots

Access at `http://localhost:5000`:

1. **Upload Form** - Select files and client ID
2. **Results Grid** - View and edit classifications
3. **Memory Viewer** - Inspect learned patterns

## 🔌 REST API / MCP Endpoints

### Authentication

All endpoints require API key authentication:

```bash
Authorization: ApiKey tbx-dev-key-12345
```

### Endpoints

#### POST /classify

Classify accounts between two trial balance files.

**Request:**
```bash
curl -X POST http://localhost:5000/classify \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -F "prev=@TB_Previous.csv" \
  -F "curr=@TB_Current.csv" \
  -F "client_id=ACME"
```

**Response:**
```json
{
  "client_id": "ACME",
  "count": 6,
  "new_accounts": 5,
  "renumbered_accounts": 1,
  "results": [
    {
      "account_code": "1250",
      "account_name": "Cash on Hand",
      "parent_code": "1000",
      "status": "new",
      "suggested_category": "Assets>Cash",
      "confidence": 85,
      "needs_review": false,
      "renumbered_from_code": null,
      "renumbered_from_name": null,
      "evidence": "[{\"method\":\"keyword\",\"score\":0.85,...}]"
    }
  ]
}
```

#### POST /memory/update

Update memory mapping for a client.

**Request:**
```bash
curl -X POST http://localhost:5000/memory/update \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -H "Content-Type: application/json" \
  -d '{
    "client_id": "ACME",
    "name_norm": "cashonhand",
    "parent_norm": "cashandcashequivalents",
    "category": "Assets>Cash",
    "source": "ui",
    "updated_at": "2025-10-02T12:00:00Z"
  }'
```

#### GET /memory/{client_id}

Retrieve all memory mappings for a client.

**Request:**
```bash
curl -X GET http://localhost:5000/memory/ACME \
  -H "Authorization: ApiKey tbx-dev-key-12345"
```

**Response:**
```json
{
  "client_id": "ACME",
  "count": 15,
  "mappings": [
    {
      "client_id": "ACME",
      "name_norm": "cashonhand",
      "parent_norm": "cashandcashequivalents",
      "category": "Assets>Cash",
      "source": "ui",
      "updated_at": "2025-10-02T12:00:00Z"
    }
  ]
}
```

#### GET /config

Get current configuration (thresholds, weights, keywords, ontology).

**Request:**
```bash
curl -X GET http://localhost:5000/config \
  -H "Authorization: ApiKey tbx-dev-key-12345"
```

## 📁 Input File Format

### Required Columns

Both input files must contain:

- `account_code` (string) - Unique account identifier
- `account_name` (string) - Account name/description

### Optional Columns

These columns are included if present:

- `parent_code` - Parent account code
- `level` - Account hierarchy level
- `opening_balance` - Opening balance
- `debit` - Debit amount
- `credit` - Credit amount
- `closing_balance` - Closing balance
- `currency` - Currency code

### Example Input

```csv
account_code,account_name,parent_code,level,opening_balance,debit,credit,closing_balance,currency
1000,Cash and Cash Equivalents,0,1,50000.00,25000.00,15000.00,60000.00,USD
1100,Bank Account - Main,1000,2,35000.00,20000.00,10000.00,45000.00,USD
2000,Accounts Receivable,0,1,85000.00,45000.00,30000.00,100000.00,USD
```

## 📤 Output Format

### ClassificationResult Fields

| Column | Type | Description |
|--------|------|-------------|
| `account_code` | string | Account code from current file |
| `account_name` | string | Account name from current file |
| `parent_code` | string? | Parent code (if available) |
| `status` | string | "new" or "likely_renumbered" |
| `suggested_category` | string? | Suggested category path |
| `confidence` | int | Confidence score (0-100%) |
| `needs_review` | bool | True if confidence < threshold |
| `renumbered_from_code` | string? | Original account code (if renumbered) |
| `renumbered_from_name` | string? | Original account name (if renumbered) |
| `evidence` | string | JSON array of classification evidence |

### Example Output

```csv
account_code,account_name,parent_code,status,suggested_category,confidence,needs_review,renumbered_from_code,renumbered_from_name
1250,Cash on Hand,1000,new,Assets>Cash,85,false,,
4150,Office-Equipment,4000,likely_renumbered,Assets>Fixed Assets,92,false,4100,Office Equipment
```

## ⚙️ Configuration

### config.yaml

Configure thresholds, weights, keywords, and ontology:

```yaml
thresholds:
  renumberedSimilarity: 0.92
  memoryExactMatch: 1.0
  keywordMatch: 0.85
  parentMatch: 0.80
  fuzzyMatch: 0.75
  needsReviewBelow: 70

weights:
  memory: 0.50
  keyword: 0.25
  parent: 0.15
  fuzzy: 0.10

keywords:
  "Assets>Cash":
    - cash
    - bank
    - checking
  "Assets>Receivables":
    - receivable
    - debtors
    - ar

ontology:
  "1": "Assets"
  "10": "Assets>Current Assets"
  "11": "Assets>Current Assets>Cash"
```

### appsettings.json

Configure API key and paths:

```json
{
  "ApiKey": "tbx-dev-key-12345",
  "DataDirectory": "./data",
  "ConfigPath": "./config.yaml"
}
```

## 🧠 Classification Logic

### Priority Order

1. **Memory Match** (Weight: 0.50)
   - Exact match on normalized name + parent
   - Uses per-client memory CSV files

2. **Keyword Match** (Weight: 0.25)
   - Searches account name for predefined keywords
   - Configurable in config.yaml

3. **Parent Code Match** (Weight: 0.15)
   - Uses account code prefix hierarchy
   - Configurable ontology mapping

4. **Fuzzy Match** (Weight: 0.10)
   - Finds similar accounts in memory (≥75% similarity)
   - Uses Jaro-Winkler and Token Sort algorithms

5. **Uncategorized**
   - No match found, confidence = 0

### Confidence Calculation

```
confidence = Σ(weight_i × score_i) × 100
```

Accounts with confidence < 70% are flagged for review.

### Name Normalization

Before comparison, account names are normalized:
- Convert to lowercase
- Remove: `-`, `_`, `/`, `(`, `)`, `.`, `,`, `"`, spaces

Example:
- `"Office Equipment (Main)"` → `officeequipmentmain`
- `"Bank-Account_Checking"` → `bankaccountchecking`

## 📊 Sample Data

Sample files are provided in `SampleData/`:
- `TB_Previous.csv` - Previous quarter (13 accounts)
- `TB_Current.csv` - Current quarter (18 accounts)

Test the system:

```bash
# CLI mode
dotnet run classify --prev SampleData/TB_Previous.csv --curr SampleData/TB_Current.csv --out NewAccounts.csv --client TEST

# Or use the web UI to upload these files
```

## 🐳 Docker Deployment

Complete Docker setup for local and production deployment.

### Quick Start

```bash
# Build and run
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

### Data Persistence

```
./data/          # Memory CSV files per client
./logs/          # Application logs
```

See [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md) for comprehensive guide.

## 🔐 Security

### API Key Authentication

Change the default API key in production:

```json
{
  "ApiKey": "your-strong-random-key-here"
}
```

### Network Access

- **Localhost only:** `127.0.0.1:5000` in docker-compose.yml
- **Network access:** `0.0.0.0:5000` for mobile access
- **Production:** Use reverse proxy with SSL/TLS

### Audit Logging

All API operations are logged:
- Timestamp
- Client ID
- Operation type
- Results

Logs stored in `./logs/` directory.

## 📈 Performance

### Benchmarks

| Accounts | Processing Time |
|----------|----------------|
| 1,000 | < 1 second |
| 5,000 | 2-5 seconds |
| 10,000 | 5-10 seconds |

### Optimization Tips

- Use memory mappings to improve accuracy and speed
- Configure appropriate confidence thresholds
- Run in Release mode for production
- Monitor resource usage with `docker stats`

## 🛠️ Development

### Project Structure

```
C:\Dev\accountant\
├── Models/                    # Data models
│   ├── TrialBalanceAccount.cs
│   ├── ClassificationResult.cs
│   ├── MemoryMapping.cs
│   ├── Evidence.cs
│   └── ClassificationConfig.cs
├── Services/                  # Business logic
│   ├── CsvService.cs
│   ├── MemoryService.cs
│   ├── ConfigService.cs
│   ├── EnhancedClassifierService.cs
│   └── AccountComparisonService.cs
├── Api/                       # REST API
│   └── ApiService.cs
├── wwwroot/                   # Web UI
│   ├── index.html
│   ├── styles.css
│   └── app.js
├── SampleData/                # Test data
├── Program.cs                 # Entry point
├── Dockerfile                 # Docker build
├── docker-compose.yml         # Docker compose
└── config.yaml               # Configuration
```

### Build

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests (if any)
dotnet test

# Publish
dotnet publish -c Release -o ./publish
```

## 📚 Additional Documentation

- [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md) - Complete Docker guide
- [QUICKSTART.md](QUICKSTART.md) - Quick start guide
- `config.yaml` - Configuration reference
- API documentation (inline in this README)

## 🤝 Support

For issues or questions:

1. Check logs: `docker-compose logs` or `./logs/`
2. Verify configuration: `config.yaml`
3. Test API: `curl http://localhost:5000/config`
4. Review documentation

## 📝 License

This tool is provided as-is for trial balance analysis purposes.

## 🎯 Roadmap

- [ ] Excel file support (XLSX)
- [ ] Multi-language support (HE/EN variants)
- [ ] Advanced filtering and search in UI
- [ ] Export to PDF reports
- [ ] Database backend (SQLite/PostgreSQL)
- [ ] User authentication and multi-tenancy
- [ ] Real-time collaboration
- [ ] ChatGPT plugin integration

---

**Built with ❤️ using .NET 8, ASP.NET Core, and Barcelona colors ⚽**
