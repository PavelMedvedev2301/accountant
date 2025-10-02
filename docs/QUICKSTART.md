# Quick Start Guide

## Run the Tool (3 Simple Steps)

### 1. Build the Project
```bash
dotnet build -c Release
```

### 2. Run with Sample Data
```bash
dotnet run -- classify --prev SampleData/TB_Previous.csv --curr SampleData/TB_Current.csv --out NewAccounts.csv
```

### 3. View Results
Open `NewAccounts.csv` to see detected new and renumbered accounts.

## Expected Output

```
Trial Balance Classifier v1.0
================================

Reading previous trial balance: SampleData/TB_Previous.csv
  ✓ Loaded 13 accounts

Reading current trial balance: SampleData/TB_Current.csv
  ✓ Loaded 18 accounts

Analyzing accounts...
  ✓ Found 5 new account(s)
  ✓ Found 1 likely renumbered account(s)

Writing results to: NewAccounts.csv
  ✓ Successfully written 6 record(s)

Completed in 137ms
```

## Use with Your Own Files

```bash
dotnet run -- classify --prev YourPrevious.csv --curr YourCurrent.csv --out Results.csv
```

## Create a Standalone Executable

### Windows
```bash
dotnet publish -c Release -r win-x64 --self-contained
```
Executable will be in: `bin\Release\net8.0\win-x64\publish\tbx.exe`

### macOS (Intel)
```bash
dotnet publish -c Release -r osx-x64 --self-contained
```

### macOS (Apple Silicon)
```bash
dotnet publish -c Release -r osx-arm64 --self-contained
```

### Linux
```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

Then you can run it directly:
```bash
./tbx classify --prev TB_Previous.csv --curr TB_Current.csv --out NewAccounts.csv
```

## Troubleshooting

### "File not found"
- Verify file paths are correct
- Use absolute paths if needed: `C:\Data\TB_Previous.csv`

### "Missing required column"
- Ensure CSV has headers: `account_code` and `account_name`
- Check spelling and case (must be lowercase with underscores)

### "Duplicate account_code"
- Each account_code must be unique within a file
- Check for duplicate rows in your CSV

## Performance Tips

- For files with 5,000+ rows, expected runtime is 2-5 seconds
- Larger files (10,000+ rows) should complete in under 10 seconds
- No file size limit, but memory usage scales linearly with row count

