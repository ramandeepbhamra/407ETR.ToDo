# Xray + Jira Setup Guide

Complete guide for setting up Xray with Jira Cloud and generating API credentials.

---

## Prerequisites

- **Jira Cloud Account:** https://www.atlassian.com/software/jira
- **Xray for Jira Cloud:** Install from Atlassian Marketplace

---

## Part 1: Jira Cloud Setup

### 1.1 Create Jira Account & Project

1. **Sign up for Jira Cloud:**
   - Visit https://www.atlassian.com/software/jira/free
   - Create account or sign in
   - Your site will be: `https://your-site.atlassian.net`

2. **Create a Project:**
   - Click **Projects** → **Create project**
   - Select **Software Development** template
   - Choose **Scrum** or **Kanban**
   - Project Key: `ETR` (or your choice — must be uppercase)
   - Project Name: `407 ETR ToDo`

3. **Configure Issue Types:**
   - Ensure these issue types exist:
     - Epic
     - Story
     - Task
     - Sub-task
   - Go to **Project settings** → **Issue types** to verify

---

### 1.2 Generate Jira API Token

**⚠️ API Tokens are required for REST API authentication.**

1. **Navigate to API Tokens page:**
   - Visit: https://id.atlassian.com/manage-profile/security/api-tokens
   - Or: Profile → **Manage account** → **Security** → **API tokens**

2. **Create API Token:**
   - Click **Create API token**
   - Label: `ETR.ToDo Automation` (or any descriptive name)
   - Click **Create**
   - **Copy the token immediately** (you cannot view it again!)

3. **Store Securely:**
   - Token format: `ATATT3xFfGF0sP...` (long alphanumeric string)
   - Save to `appsettings.Development.json`:
     ```json
     "Jira": {
       "BaseUrl": "https://your-site.atlassian.net",
       "Email": "your-email@example.com",
       "ApiToken": "ATATT3xFfGF0sP...",
       "ProjectKey": "ETR"
     }
     ```

4. **Authentication Method:**
   - **Basic Auth** with email + API token
   - PowerShell example:
     ```powershell
     $token = "ATATT3xFfGF0sP..."
     $auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("your-email@example.com:$token"))
     $headers = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json" }
     ```

---

## Part 2: Xray for Jira Cloud Setup

### 2.1 Install Xray App

1. **Install from Marketplace:**
   - Visit: https://marketplace.atlassian.com/apps/1211769/xray-test-management-for-jira
   - Click **Try it free** or **Buy now**
   - Select your Jira site
   - Authorize installation

2. **Verify Installation:**
   - In Jira, top menu should now show **Xray** dropdown
   - Project sidebar should show **Test Repository**, **Test Plans**, **Test Runs**

3. **Configure Xray Settings (Optional):**
   - Go to **Xray** → **Settings**
   - Enable/disable features as needed

---

### 2.2 Generate Xray API Credentials

**⚠️ Xray GraphQL API requires Client ID + Client Secret (not the same as Jira API token).**

1. **Navigate to API Keys page:**
   - In Jira, click **Xray** (top menu) → **API Keys**
   - Or visit: https://xray.cloud.getxray.app/api/v2/keys

2. **Create API Key:**
   - Click **Create API Key**
   - Name: `ETR.ToDo Automation`
   - Click **Generate**
   - **Copy both values immediately:**
     - **Client ID:** `1B7AA542BFB54D5C8B4C6E447EC2A81D` (example)
     - **Client Secret:** `5f78a5b6633504d956f750ace3a0483452588e4d5773a662863acf274a9b6deb` (example)

3. **Store Securely:**
   - Save to `appsettings.Development.json`:
     ```json
     "Xray": {
       "BaseUrl": "https://xray.cloud.getxray.app/api/v2",
       "ClientId": "1B7AA542BFB54D5C8B4C6E447EC2A81D",
       "ClientSecret": "5f78a5b6633504d956f750ace3a0483452588e4d5773a662863acf274a9b6deb"
     }
     ```

4. **Authentication Method:**
   - **Bearer Token** via `/authenticate` endpoint
   - Token expires after ~60 minutes
   - PowerShell example:
     ```powershell
     $clientId = "1B7AA542BFB54D5C8B4C6E447EC2A81D"
     $clientSecret = "5f78a5b6633504d956f750ace3a0483452588e4d5773a662863acf274a9b6deb"
     $authBody = @{ client_id = $clientId; client_secret = $clientSecret } | ConvertTo-Json
     $authResponse = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/authenticate" -Method Post -Body $authBody -ContentType "application/json"
     $bearerToken = $authResponse
     $headers = @{ Authorization = "Bearer $bearerToken"; "Content-Type" = "application/json" }
     ```

---

## Part 3: Complete Workflow — ETR-6 Example

This section documents the exact steps we followed to create 7 test cases for story ETR-6.

### 3.1 Authenticate with Xray

```powershell
$clientId = "YOUR_CLIENT_ID"
$clientSecret = "YOUR_CLIENT_SECRET"
$authBody = @{ client_id = $clientId; client_secret = $clientSecret } | ConvertTo-Json
$authResponse = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/authenticate" -Method Post -Body $authBody -ContentType "application/json"
$bearerToken = $authResponse
$headers = @{ Authorization = "Bearer $bearerToken"; "Content-Type" = "application/json" }
```

---

### 3.2 Create Preconditions (4 total)

**Why:** Preconditions define test environment setup (authentication state, viewport, etc.)

```powershell
$preconditions = @(
    @{ key = "ETR-28"; summary = "User is not authenticated"; description = "Browser has no auth token, user is logged out" },
    @{ key = "ETR-29"; summary = "User is logged in as Basic user"; description = "Valid JWT token in localStorage, role = Basic" },
    @{ key = "ETR-30"; summary = "Desktop viewport"; description = "Browser width >= 1024px" },
    @{ key = "ETR-31"; summary = "Mobile viewport"; description = "Browser width < 768px" }
)

foreach ($pc in $preconditions) {
    $mutation = @"
mutation {
  createPrecondition(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "$($pc.summary)"
        description: "$($pc.description)"
        labels: ["ETR-6", "precondition"]
      }
    }
  ) {
    precondition {
      issueId
      jira(fields: ["key", "summary"])
    }
  }
}
"@
    $body = @{ query = $mutation } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
    $key = $result.data.createPrecondition.precondition.jira.key
    Write-Host "✓ Created $key - $($pc.summary)" -ForegroundColor Green
}
```

---

### 3.3 Create Test Cases (Manual type with steps)

**⚠️ Important:** Use `Manual` test type (not `Generic`) for step-by-step tests.

```powershell
$tests = @(
    @{
        summary = "Landing page loads for unauthenticated user"
        label = "TC_ETR_6_001"
        tags = @("smoke", "regression")
        steps = @(
            @{ action = "Navigate to /"; data = ""; result = "Landing page visible with login CTA" },
            @{ action = "Verify page title"; data = ""; result = "Title shows 'ETR ToDo'" }
        )
    },
    # Add remaining 6 tests...
)

foreach ($test in $tests) {
    $stepsJson = ($test.steps | ForEach-Object {
        "{ action: `"$($_.action)`", data: `"$($_.data)`", result: `"$($_.result)`" }"
    }) -join ", "
    
    $labelsJson = ((@($test.label, "ETR-6") + $test.tags) | ForEach-Object { "`"$_`"" }) -join ", "
    
    $mutation = @"
mutation {
  createTest(
    testType: { name: "Manual" }
    steps: [$stepsJson]
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "$($test.summary)"
        labels: [$labelsJson]
      }
    }
  ) {
    test {
      issueId
      jira(fields: ["key", "summary"])
    }
  }
}
"@
    $body = @{ query = $mutation } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
    $key = $result.data.createTest.test.jira.key
    Write-Host "✓ Created $key - $($test.summary)" -ForegroundColor Green
}
```

---

### 3.4 Link Tests to Parent Story (Jira REST API)

**Why:** Create "Tests" relationship so story shows linked test cases.

```powershell
$token = "YOUR_JIRA_API_TOKEN"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("your-email@example.com:$token"))
$headers = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json" }

$testKeys = @("ETR-16", "ETR-17", "ETR-19", "ETR-20", "ETR-21", "ETR-26", "ETR-27")

foreach ($testKey in $testKeys) {
    $body = @{
        type = @{ name = "Test" }
        inwardIssue = @{ key = $testKey }
        outwardIssue = @{ key = "ETR-6" }
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "https://ai-autopilot.atlassian.net/rest/api/2/issueLink" -Method Post -Headers $headers -Body $body
    Write-Host "✓ Linked $testKey → ETR-6" -ForegroundColor Green
}
```

---

### 3.5 Link Preconditions to Tests

**⚠️ CRITICAL:** Must use **numeric `issueId`**, not Jira keys!

**Step 1:** Get numeric IDs:
```powershell
$query = @"
{
  getTests(jql: "key IN (ETR-16, ETR-17, ETR-19, ETR-20, ETR-21, ETR-26, ETR-27)", limit: 10) {
    results {
      issueId
      jira(fields: ["key"])
    }
  }
  getPreconditions(jql: "key IN (ETR-28, ETR-29, ETR-30, ETR-31)", limit: 10) {
    results {
      issueId
      jira(fields: ["key"])
    }
  }
}
"@
$body = @{ query = $query } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body

# Map keys to numeric IDs
$testMap = @{}
$response.data.getTests.results | ForEach-Object { $testMap[$_.jira.key] = $_.issueId }
$preMap = @{}
$response.data.getPreconditions.results | ForEach-Object { $preMap[$_.jira.key] = $_.issueId }
```

**Step 2:** Link using numeric IDs:
```powershell
$links = @(
    @{ test = $testMap["ETR-16"]; preconditions = @($preMap["ETR-28"]) },
    @{ test = $testMap["ETR-17"]; preconditions = @($preMap["ETR-28"]) },
    @{ test = $testMap["ETR-20"]; preconditions = @($preMap["ETR-29"], $preMap["ETR-30"]) }
    # Add remaining links...
)

foreach ($link in $links) {
    $preIds = ($link.preconditions | ForEach-Object { "`"$_`"" }) -join ", "
    $mutation = @"
mutation {
  addPreconditionsToTest(
    issueId: "$($link.test)"
    preconditionIssueIds: [$preIds]
  ) {
    addedPreconditions
    warning
  }
}
"@
    $body = @{ query = $mutation } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
    Write-Host "✓ Linked test ID $($link.test) to preconditions" -ForegroundColor Green
}
```

---

### 3.6 Create Test Set

**Why:** Group all tests for a story/feature together.

```powershell
$mutation = @"
mutation {
  createTestSet(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "ETR-6 Landing Page Tests"
        description: "All test cases for story ETR-6 (Public Landing Page)"
        labels: ["ETR-6", "test-set"]
      }
    }
  ) {
    testSet {
      issueId
      jira(fields: ["key", "summary"])
    }
  }
}
"@
$body = @{ query = $mutation } | ConvertTo-Json
$result = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
$testSetKey = $result.data.createTestSet.testSet.jira.key
Write-Host "✓ Created Test Set: $testSetKey" -ForegroundColor Green
```

**Add tests to Test Set:**
```powershell
$testKeys = @("ETR-16", "ETR-17", "ETR-19", "ETR-20", "ETR-21", "ETR-26", "ETR-27")
$testIdsJson = ($testKeys | ForEach-Object { "`"$_`"" }) -join ", "

$mutation = @"
mutation {
  addTestsToTestSet(
    issueId: "$testSetKey"
    testIssueIds: [$testIdsJson]
  ) {
    addedTests
    warning
  }
}
"@
$body = @{ query = $mutation } | ConvertTo-Json
Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
```

---

### 3.7 Create Test Plan

**Why:** Organize tests by sprint/release/milestone.

```powershell
$mutation = @"
mutation {
  createTestPlan(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "Sprint 1 - Dashboard Tests"
        description: "Test plan for Sprint 1 covering all Dashboard epic tests"
        labels: ["sprint-1", "test-plan"]
      }
    }
  ) {
    testPlan {
      issueId
      jira(fields: ["key", "summary"])
    }
  }
}
"@
$body = @{ query = $mutation } | ConvertTo-Json
$result = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
$testPlanKey = $result.data.createTestPlan.testPlan.jira.key
Write-Host "✓ Created Test Plan: $testPlanKey" -ForegroundColor Green
```

**Add tests to Test Plan:**
```powershell
$mutation = @"
mutation {
  addTestsToTestPlan(
    issueId: "$testPlanKey"
    testIssueIds: [$testIdsJson]
  ) {
    addedTests
    warning
  }
}
"@
$body = @{ query = $mutation } | ConvertTo-Json
Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" -Method Post -Headers $headers -Body $body
```

---

## Part 4: Key Learnings & Troubleshooting

### 4.1 Critical Issues Encountered

| Issue | Solution |
|---|---|
| **Generic test type doesn't support steps** | Use `Manual` test type instead |
| **Preconditions not linking** | Must use numeric `issueId` (e.g., `"10016"`), not Jira keys (e.g., `"ETR-16"`) |
| **PowerShell quote escaping errors** | Use here-strings `@"..."@` for GraphQL queries |
| **"Tests" link type not found in Xray** | Use Jira REST API `/issueLink` endpoint with link type `"Test"` |
| **Token expires during long sessions** | Re-authenticate every 60 minutes or before each operation |

---

### 4.2 Best Practices

✅ **Always verify before creating:** Use `getTests(jql: "key = ETR-16")` to check for duplicates  
✅ **Use labels for filtering:** Add story key (`ETR-6`), test ID (`TC_ETR_6_001`), and tags (`smoke`)  
✅ **Numeric IDs for preconditions:** Always query for `issueId` before linking  
✅ **Test type matters:** `Manual` for step-by-step, `Generic` for automated (no steps UI)  
✅ **Bidirectional links:** Link tests → story AND story → tests for full visibility  

---

## Part 5: Future Auto-Generation Tool

### 5.1 Recommended Mapping: Xray Steps → Gherkin

| Xray Field | Gherkin Keyword | Logic |
|---|---|---|
| **Action** (step 1) | `Given` | If precondition-like ("Navigate to", "User is logged in") |
| **Action** (step 2+) | `When` | If action-like ("Click", "Enter", "Select") |
| **Result** | `Then` | Always maps to assertion/verification |
| **Data** | Inline parameter | Use as data table or `<parameter>` in step |

**Example:**
```
Xray Step 1:
  Action: Navigate to /
  Data: 
  Result: Landing page visible with login CTA

Gherkin:
  Given I navigate to "/"
  Then I should see the landing page with login CTA
```

**Recommended Strategy:**
- Use **AI/LLM** to infer Given/When/Then from natural language
- Or add **custom field** in Xray steps: `stepType: given|when|then`
- **Fallback:** First step = Given, middle steps = When, last step = Then

---

### 5.2 Tool Architecture (.NET Console + API)

```
ETR.ToDo.TestGen/
├── ETR.ToDo.TestGen.Core/          # Business logic
│   ├── Services/
│   │   ├── JiraService.cs          # Fetch Jira story context
│   │   ├── XrayService.cs          # Fetch test cases
│   │   ├── PlaywrightAnalyzer.cs   # Parse existing .feature + .ts files
│   │   └── GherkinGenerator.cs     # Generate .feature files
│   ├── Models/
│   │   ├── XrayTest.cs
│   │   ├── XrayStep.cs
│   │   └── GherkinScenario.cs
│   └── Mappers/
│       └── XrayToGherkinMapper.cs  # Mapperly mapper
├── ETR.ToDo.TestGen.CLI/           # Console app
│   └── Program.cs                  # `dotnet run --story ETR-6`
└── ETR.ToDo.TestGen.Api/           # ASP.NET Core API
    └── Controllers/
        └── TestGenController.cs    # POST /api/testgen/generate?story=ETR-6
```

---

## Part 6: Reference Links

| Resource | URL |
|---|---|
| **Jira REST API Docs** | https://developer.atlassian.com/cloud/jira/platform/rest/v2/ |
| **Xray GraphQL API Docs** | https://docs.getxray.app/display/XRAYCLOUD/GraphQL+API |
| **Xray API Keys** | https://xray.cloud.getxray.app/api/v2/keys |
| **Jira API Tokens** | https://id.atlassian.com/manage-profile/security/api-tokens |
| **Xray Marketplace** | https://marketplace.atlassian.com/apps/1211769/xray-test-management-for-jira |
| **Project API Reference** | [docs/xray/api-reference.md](api-reference.md) |
| **Project Template** | [docs/xray/template-story-tests.md](template-story-tests.md) |

---

## Summary

This guide covered:
1. ✅ Jira Cloud account setup + API token generation
2. ✅ Xray app installation + API credentials generation
3. ✅ Complete workflow: Create preconditions → tests → Test Set → Test Plan → link everything
4. ✅ Key learnings (numeric IDs, Manual test type, etc.)
5. ✅ Future tool architecture recommendation

**Ready to proceed with auto-generation tool when you are!**
