# API Reference

Complete REST API documentation for Trial Balance Classifier.

## Base URL

```
http://localhost:5000
```

## Authentication

All endpoints require API Key authentication.

**Header:**
```
Authorization: ApiKey tbx-dev-key-12345
```

**Default API Key:** `tbx-dev-key-12345` (change in production!)

---

## Endpoints

### 1. POST /classify

Classify accounts between two trial balance files.

#### Request

**Method:** `POST`  
**Content-Type:** `multipart/form-data`

**Parameters:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `prev` | file | Yes | Previous trial balance CSV file |
| `curr` | file | Yes | Current trial balance CSV file |
| `client_id` | string | Yes | Client identifier for memory management |
| `format` | string | No | Response format: "json" (default) or "csv" |

#### Example - cURL

```bash
curl -X POST http://localhost:5000/classify \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -F "prev=@TB_Previous.csv" \
  -F "curr=@TB_Current.csv" \
  -F "client_id=ACME"
```

#### Example - PowerShell

```powershell
$headers = @{ "Authorization" = "ApiKey tbx-dev-key-12345" }
$form = @{
    prev = Get-Item -Path "TB_Previous.csv"
    curr = Get-Item -Path "TB_Current.csv"
    client_id = "ACME"
}
Invoke-RestMethod -Uri "http://localhost:5000/classify" `
    -Method Post -Headers $headers -Form $form
```

#### Example - JavaScript

```javascript
const formData = new FormData();
formData.append('prev', prevFile);
formData.append('curr', currFile);
formData.append('client_id', 'ACME');

const response = await fetch('http://localhost:5000/classify', {
    method: 'POST',
    headers: {
        'Authorization': 'ApiKey tbx-dev-key-12345'
    },
    body: formData
});

const data = await response.json();
```

#### Response (JSON)

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
      "evidence": "[{\"method\":\"keyword\",\"score\":0.85,\"matched_keyword\":\"cash\",\"category\":\"Assets>Cash\"}]"
    }
  ]
}
```

#### Response (CSV)

Request with `?format=csv`:

```bash
curl -X POST "http://localhost:5000/classify?format=csv" \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -F "prev=@TB_Previous.csv" \
  -F "curr=@TB_Current.csv" \
  -F "client_id=ACME" \
  --output NewAccounts.csv
```

Returns CSV file with columns:
- account_code
- account_name
- parent_code
- status
- suggested_category
- confidence
- needs_review
- renumbered_from_code
- renumbered_from_name

---

### 2. POST /memory/update

Update or add a memory mapping for a client.

#### Request

**Method:** `POST`  
**Content-Type:** `application/json`

**Body:**

```json
{
  "client_id": "ACME",
  "name_norm": "cashonhand",
  "parent_norm": "cashandcashequivalents",
  "category": "Assets>Cash",
  "source": "ui",
  "updated_at": "2025-10-02T12:00:00Z"
}
```

**Fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `client_id` | string | Yes | Client identifier |
| `name_norm` | string | Yes | Normalized account name |
| `parent_norm` | string | No | Normalized parent account name |
| `category` | string | Yes | Category path (e.g., "Assets>Cash") |
| `source` | string | No | Source of mapping: "ui", "api", "manual", "agent" |
| `updated_at` | string | No | ISO 8601 timestamp (auto-generated if not provided) |

#### Example - cURL

```bash
curl -X POST http://localhost:5000/memory/update \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -H "Content-Type: application/json" \
  -d '{
    "client_id": "ACME",
    "name_norm": "cashonhand",
    "parent_norm": "cashandcashequivalents",
    "category": "Assets>Cash",
    "source": "api"
  }'
```

#### Example - JavaScript

```javascript
const mapping = {
  client_id: "ACME",
  name_norm: "cashonhand",
  parent_norm: "cashandcashequivalents",
  category: "Assets>Cash",
  source: "ui"
};

const response = await fetch('http://localhost:5000/memory/update', {
  method: 'POST',
  headers: {
    'Authorization': 'ApiKey tbx-dev-key-12345',
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(mapping)
});

const result = await response.json();
```

#### Response

```json
{
  "success": true,
  "message": "Memory updated successfully"
}
```

---

### 3. GET /memory/{client_id}

Retrieve all memory mappings for a specific client.

#### Request

**Method:** `GET`  
**Path Parameter:** `client_id` - Client identifier

#### Example - cURL

```bash
curl -X GET http://localhost:5000/memory/ACME \
  -H "Authorization: ApiKey tbx-dev-key-12345"
```

#### Example - PowerShell

```powershell
$headers = @{ "Authorization" = "ApiKey tbx-dev-key-12345" }
Invoke-RestMethod -Uri "http://localhost:5000/memory/ACME" `
    -Method Get -Headers $headers
```

#### Response

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
    },
    {
      "client_id": "ACME",
      "name_norm": "bankaccountmain",
      "parent_norm": "cashandcashequivalents",
      "category": "Assets>Cash",
      "source": "memory",
      "updated_at": "2025-10-01T08:30:00Z"
    }
  ]
}
```

---

### 4. GET /config

Retrieve current configuration (thresholds, weights, keywords, ontology).

#### Request

**Method:** `GET`

#### Example - cURL

```bash
curl -X GET http://localhost:5000/config \
  -H "Authorization: ApiKey tbx-dev-key-12345"
```

#### Response

```json
{
  "thresholds": {
    "renumberedSimilarity": 0.92,
    "memoryExactMatch": 1.0,
    "keywordMatch": 0.85,
    "parentMatch": 0.80,
    "fuzzyMatch": 0.75,
    "needsReviewBelow": 70
  },
  "weights": {
    "memory": 0.50,
    "keyword": 0.25,
    "parent": 0.15,
    "fuzzy": 0.10
  },
  "keywords": {
    "Assets>Cash": ["cash", "bank", "checking"],
    "Assets>Receivables": ["receivable", "debtors", "ar"]
  },
  "ontology": {
    "1": "Assets",
    "10": "Assets>Current Assets",
    "11": "Assets>Current Assets>Cash"
  }
}
```

---

## Error Responses

### 401 Unauthorized

Missing or invalid API key.

```json
{
  "error": "Unauthorized"
}
```

### 400 Bad Request

Missing required fields or validation error.

```json
{
  "error": "Missing required fields: prev, curr, client_id"
}
```

### 500 Internal Server Error

Server error during processing.

```json
{
  "error": "Internal server error: <details>"
}
```

---

## Data Models

### ClassificationResult

```typescript
{
  account_code: string;
  account_name: string;
  parent_code?: string;
  status: "new" | "likely_renumbered";
  suggested_category?: string;
  confidence: number;  // 0-100
  needs_review: boolean;
  renumbered_from_code?: string;
  renumbered_from_name?: string;
  evidence: string;  // JSON array
}
```

### MemoryMapping

```typescript
{
  client_id: string;
  name_norm: string;
  parent_norm?: string;
  category: string;
  source: string;
  updated_at: string;  // ISO 8601
}
```

### Evidence

```typescript
{
  method: "memory" | "keyword" | "parent" | "fuzzy" | "uncategorized";
  score: number;
  matched_name?: string;
  matched_parent?: string;
  matched_keyword?: string;
  category?: string;
}
```

---

## Rate Limiting

Currently no rate limiting implemented. For production:
- Recommended: 100 requests per minute per API key
- Large file processing may take 5-10 seconds

---

## Best Practices

### 1. Client ID Naming

Use consistent, descriptive client IDs:
- ✅ Good: `ACME`, `CLIENT_001`, `CompanyName`
- ❌ Avoid: `test`, `123`, `temp`

### 2. Memory Management

- Update memory after manual review
- Use meaningful category hierarchies
- Keep source field descriptive
- Regular backups of `./data/` directory

### 3. Performance

- Process files in batches if possible
- Use CSV format for output when appropriate
- Monitor confidence scores to adjust thresholds
- Pre-configure keywords for common account types

### 4. Security

- **Never commit API keys to version control**
- Use environment variables for API keys
- Enable HTTPS for production deployments
- Implement rate limiting
- Log all API access for audit

---

## Integration Examples

### ChatGPT Plugin

```json
{
  "openapi": "3.0.0",
  "info": {
    "title": "Trial Balance Classifier API",
    "version": "2.0.0"
  },
  "servers": [
    {
      "url": "http://localhost:5000"
    }
  ],
  "paths": {
    "/classify": {
      "post": {
        "operationId": "classifyTrialBalance",
        "summary": "Classify trial balance accounts"
      }
    }
  }
}
```

### Python Client

```python
import requests

API_URL = "http://localhost:5000"
API_KEY = "tbx-dev-key-12345"

def classify_trial_balance(prev_file, curr_file, client_id):
    files = {
        'prev': open(prev_file, 'rb'),
        'curr': open(curr_file, 'rb')
    }
    data = {'client_id': client_id}
    headers = {'Authorization': f'ApiKey {API_KEY}'}
    
    response = requests.post(
        f"{API_URL}/classify",
        files=files,
        data=data,
        headers=headers
    )
    return response.json()

result = classify_trial_balance('TB_Previous.csv', 'TB_Current.csv', 'ACME')
print(f"Found {result['count']} changes")
```

### Node.js Client

```javascript
const FormData = require('form-data');
const fs = require('fs');
const axios = require('axios');

async function classifyTrialBalance(prevFile, currFile, clientId) {
  const form = new FormData();
  form.append('prev', fs.createReadStream(prevFile));
  form.append('curr', fs.createReadStream(currFile));
  form.append('client_id', clientId);

  const response = await axios.post('http://localhost:5000/classify', form, {
    headers: {
      ...form.getHeaders(),
      'Authorization': 'ApiKey tbx-dev-key-12345'
    }
  });

  return response.data;
}
```

---

## Testing

### Health Check

```bash
curl http://localhost:5000/
# Should return 200 OK
```

### Full Workflow Test

```bash
# 1. Classify accounts
curl -X POST http://localhost:5000/classify \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -F "prev=@TB_Previous.csv" \
  -F "curr=@TB_Current.csv" \
  -F "client_id=TEST" > results.json

# 2. Update memory
curl -X POST http://localhost:5000/memory/update \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -H "Content-Type: application/json" \
  -d '{"client_id":"TEST","name_norm":"testaccount","category":"Assets>Cash","source":"api"}'

# 3. Verify memory
curl -X GET http://localhost:5000/memory/TEST \
  -H "Authorization: ApiKey tbx-dev-key-12345"

# 4. Re-classify to see memory in action
curl -X POST http://localhost:5000/classify \
  -H "Authorization: ApiKey tbx-dev-key-12345" \
  -F "prev=@TB_Previous.csv" \
  -F "curr=@TB_Current.csv" \
  -F "client_id=TEST"
```

---

For more information, see [README.md](README.md) and [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md).

