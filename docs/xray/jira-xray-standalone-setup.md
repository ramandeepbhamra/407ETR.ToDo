# Jira + Xray Setup Guide — Adding a New Project

Complete guide for adding a new project space to an existing Jira Cloud instance and configuring Xray Test Management.

---

## Overview

This guide covers:
- Adding a new project (workspace) to your existing Jira Cloud site
- Installing and configuring Xray for that project
- Generating API credentials for automation
- Basic API examples for creating test artifacts

**Example Project:** 407 ETR (Project Key: `ETR`)

---

## Prerequisites

- ✅ **Jira Cloud account exists** (e.g., `https://your-company.atlassian.net`)
- ✅ **Admin access** to create new projects
- ✅ **Billing/subscription** for additional projects (if required)

---

## Part 1: Create New Project in Jira

### Step 1: Navigate to Projects

1. Log in to your Jira Cloud site: `https://your-company.atlassian.net`
2. Click **Projects** in the top navigation
3. Click **Create project**

---

### Step 2: Choose Project Template

1. **Select Template:**
   - Choose: **Software Development** → **Scrum**
   - **Project type:** Team-managed

2. **Configure Project Details:**
   - **Project name:** `407 ETR` (displayed in UI)
   - **Project key:** `ETR` (used in issue keys like ETR-1, ETR-2)
     - ⚠️ Must be 2-10 uppercase letters
     - ⚠️ Cannot be changed after creation

3. Click **Create project**

---

### Step 3: Verify Default Issue Types

After project creation, verify these **default Jira issue types** are available:
- **Epic** — Large body of work (e.g., "User Authentication")
- **Story** — User-facing feature (e.g., "User can log in")
- **Task** — Non-user-facing work (e.g., "Setup database")
- **Bug** — Defect or issue
- **Sub-task** — Child of Story/Task

**Note:** Xray test management types (Precondition, Test, Test Set, etc.) will be added after installing Xray in Part 2.

---

## Part 2: Install Xray for Jira

### Step 4: Install from Atlassian Marketplace

1. **Navigate to Marketplace:**
   - In Jira, click **Apps** (top menu) → **Find new apps**
   - Or visit: https://marketplace.atlassian.com/apps/1211769/xray-test-management-for-jira

2. **Install Xray:**
   - Click **Try it free** (30-day trial) or **Buy now**
   - Select your Jira site from dropdown
   - Click **Install app**
   - Authorize the installation

3. **Verify Installation:**
   - Top menu should now show **Xray** dropdown
   - Project sidebar should show Xray navigation:
     - **Test Repository** (browse all tests)
     - **Test Plans** (organize tests by release)
     - **Test Executions** (view test run results)

---

### Step 5: Add Xray Issue Types to Your Project

Xray creates 5 new issue types globally, but they must be manually added to your Team-managed project:

1. **Go to Project Settings:**
   - Navigate to your project (407 ETR)
   - Click **Project settings** (bottom left sidebar)

2. **Add Issue Types:**
   - Click **Work types** (or **Issue types**)
   - Click **Add work type** or **+** icon
   - Search for and add each Xray type:
     - ✅ Precondition
     - ✅ Test
     - ✅ Test Execution
     - ✅ Test Plan
     - ✅ Test Set
   - Each will show a `</>` icon indicating it's an Xray type

3. **Verify:**
   - All 5 Xray types should now appear in your project's work types list
   - Each has a `</>` icon indicating it's an Xray type

**Xray issue types now available:**
- **Precondition** — Test environment setup requirements
- **Test** — Manual or automated test case
- **Test Execution** — Record of test run results
- **Test Plan** — Collection of tests for release/sprint
- **Test Set** — Logical grouping of related tests

---

### Step 6: Map Xray Issue Types in Xray Settings

After adding the issue types to your project, you must map them in Xray:

1. **Navigate to Xray Settings:**
   - Go to **Space settings** (click gear icon or **Project settings**)
   - In the left sidebar, find **Xray Settings**
   - Click **Issue Types Mapping**

2. **Map Each Type:**
   - You'll see "Team-Managed Project Issue Types Mapping" section
   - Map each Xray type to the corresponding issue type you added:
     - **Test** → Select "Test" from dropdown
     - **Precondition** → Select "Precondition"
     - **Test Set** → Select "Test Set"
     - **Test Plan** → Select "Test Plan"
     - **Test Execution** → Select "Test Execution"

3. **Save:**
   - Click **Save** button at the bottom
   - Xray will now recognize these issue types for your project

4. **Verify:**
   - Try creating a Test issue via **Create** button
   - You should see "Test" in the issue type dropdown
   - Test-specific fields (steps, preconditions) should appear

---

### Step 7: Configure Xray Settings (Optional)

1. Click **Xray** (top menu) → **Settings**
2. Configure (all optional):
   - Test issue types visibility
   - Default test repository settings
   - Test execution settings
   - Cucumber integration settings

---

## Part 3: Generate API Credentials

### Step 8: Jira REST API Token

**Purpose:** Authenticate with Jira REST API to create issues, links, etc.

#### Generate Token:

1. **Navigate to API Tokens:**
   - Visit: https://id.atlassian.com/manage-profile/security/api-tokens
   - Or: Click your profile → **Manage account** → **Security** → **API tokens**

2. **Create Token:**
   - Click **Create API token**
   - Label: `ETR Project Automation` (or any name)
   - Click **Create**
   - **⚠️ Copy the token immediately** — you cannot view it again!

3. **Token Format:**
   ```
   ATATT3xFfGF0sPlImuggrr3M43_NLvWP4Ltk0LINvVOdcHEQp3tTUOI...
   ```

#### Usage Example (PowerShell):

```powershell
# Authentication: Basic Auth with email + API token
$apiToken = "ATATT3xFfGF0sP..."
$email = "your-email@example.com"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${email}:${apiToken}"))
$headers = @{
    Authorization = "Basic $auth"
    "Content-Type" = "application/json"
}

# Example: Get all issues in ETR project
$response = Invoke-RestMethod `
    -Uri "https://your-company.atlassian.net/rest/api/2/search?jql=project=ETR" `
    -Headers $headers
```

---

### Step 9: Xray GraphQL API Credentials

**Purpose:** Authenticate with Xray Cloud API to create/query test artifacts.

#### Generate Credentials:

1. **Navigate to API Keys:**
   - In Jira, click **Xray** (top menu) → **API Keys**
   - Or visit: https://xray.cloud.getxray.app/api/v2/keys

2. **Create API Key:**
   - Click **Create API Key**
   - Name: `ETR Automation` (or any name)
   - Click **Generate**

3. **Copy Both Values:**
   - **Client ID:** `XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX` (32-character alphanumeric string)
   - **Client Secret:** `xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx` (64-character hex string)
   - **⚠️ Save both immediately** — you cannot view them again!

#### Usage Example (PowerShell):

```powershell
# Step 1: Authenticate to get Bearer token
$clientId = "YOUR_CLIENT_ID"
$clientSecret = "YOUR_CLIENT_SECRET"

$authBody = @{
    client_id = $clientId
    client_secret = $clientSecret
} | ConvertTo-Json

$authResponse = Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/authenticate" `
    -Method Post `
    -Body $authBody `
    -ContentType "application/json"

$bearerToken = $authResponse  # Token is the direct response (string)

# Step 2: Use token for API calls
$headers = @{
    Authorization = "Bearer $bearerToken"
    "Content-Type" = "application/json"
}

# Token expires after ~60 minutes — re-authenticate as needed
```

---

## Part 4: Store Credentials Securely

### Option 1: Environment Variables (Recommended for CI/CD)

```powershell
# Windows
[System.Environment]::SetEnvironmentVariable('JIRA_API_TOKEN', 'ATATT3x...', 'User')
[System.Environment]::SetEnvironmentVariable('XRAY_CLIENT_ID', 'YOUR_CLIENT_ID', 'User')
[System.Environment]::SetEnvironmentVariable('XRAY_CLIENT_SECRET', 'YOUR_CLIENT_SECRET', 'User')

# Usage
$jiraToken = $env:JIRA_API_TOKEN
$xrayClientId = $env:XRAY_CLIENT_ID
```

### Option 2: Configuration File (Recommended for local dev)

```json
{
  "Jira": {
    "BaseUrl": "https://your-company.atlassian.net",
    "Email": "your-email@example.com",
    "ApiToken": "ATATT3xFfGF0sP...",
    "ProjectKey": "ETR"
  },
  "Xray": {
    "BaseUrl": "https://xray.cloud.getxray.app/api/v2",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
  }
}
```

**⚠️ Security:** Never commit credentials to source control!

---

## Part 5: Basic API Examples

### 5.1 Create a Precondition (Xray GraphQL)

**Preconditions** define test environment setup (e.g., "User is logged in").

```powershell
# Authenticate with Xray
$bearerToken = "..." # From Part 3.2
$headers = @{ Authorization = "Bearer $bearerToken"; "Content-Type" = "application/json" }

# Create precondition
$mutation = @"
mutation {
  createPrecondition(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "User is not authenticated"
        description: "Browser has no auth token, user is logged out"
        labels: ["precondition"]
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
$response = Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

$preconditionKey = $response.data.createPrecondition.precondition.jira.key
Write-Host "Created Precondition: $preconditionKey"
```

---

### 5.2 Create a Manual Test (Xray GraphQL)

**Manual Tests** have step-by-step instructions for testers.

```powershell
$mutation = @"
mutation {
  createTest(
    testType: { name: "Manual" }
    steps: [
      { action: "Navigate to homepage", data: "", result: "Homepage loads successfully" }
      { action: "Click login button", data: "", result: "Login dialog opens" }
    ]
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "Verify login dialog opens"
        description: "Test that unauthenticated users can open login dialog"
        labels: ["TC_001", "smoke"]
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
$response = Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

$testKey = $response.data.createTest.test.jira.key
Write-Host "Created Test: $testKey"
```

---

### 5.3 Link Test to Story (Jira REST API)

**"Tests" relationship** links test cases to user stories.

```powershell
# Jira REST API authentication
$jiraToken = "ATATT3x..."
$email = "your-email@example.com"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${email}:${jiraToken}"))
$headers = @{ Authorization = "Basic $auth"; "Content-Type" = "application/json" }

# Link test to story
$body = @{
    type = @{ name = "Test" }
    inwardIssue = @{ key = "ETR-10" }   # Test case
    outwardIssue = @{ key = "ETR-5" }   # Story
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://your-company.atlassian.net/rest/api/2/issueLink" `
    -Method Post `
    -Headers $headers `
    -Body $body

Write-Host "✓ Linked ETR-10 (Test) → ETR-5 (Story)"
```

---

### 5.4 Link Precondition to Test (Xray GraphQL)

**⚠️ IMPORTANT:** Must use **numeric `issueId`**, not Jira keys!

```powershell
# Step 1: Get numeric IDs
$query = @"
{
  getTests(jql: "key = ETR-10", limit: 1) {
    results {
      issueId
      jira(fields: ["key"])
    }
  }
  getPreconditions(jql: "key = ETR-8", limit: 1) {
    results {
      issueId
      jira(fields: ["key"])
    }
  }
}
"@

$body = @{ query = $query } | ConvertTo-Json
$response = Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

$testId = $response.data.getTests.results[0].issueId
$preconditionId = $response.data.getPreconditions.results[0].issueId

# Step 2: Link using numeric IDs
$mutation = @"
mutation {
  addPreconditionsToTest(
    issueId: "$testId"
    preconditionIssueIds: ["$preconditionId"]
  ) {
    addedPreconditions
    warning
  }
}
"@

$body = @{ query = $mutation } | ConvertTo-Json
Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

Write-Host "✓ Linked Precondition to Test"
```

---

### 5.5 Create Test Set (Xray GraphQL)

**Test Sets** group related test cases together.

```powershell
$mutation = @"
mutation {
  createTestSet(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "Sprint 1 - Authentication Tests"
        description: "All authentication test cases for Sprint 1"
        labels: ["sprint-1", "auth"]
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
$response = Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

$testSetKey = $response.data.createTestSet.testSet.jira.key

# Add tests to Test Set
$mutation = @"
mutation {
  addTestsToTestSet(
    issueId: "$testSetKey"
    testIssueIds: ["ETR-10", "ETR-11", "ETR-12"]
  ) {
    addedTests
    warning
  }
}
"@

$body = @{ query = $mutation } | ConvertTo-Json
Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

Write-Host "Created Test Set: $testSetKey"
```

---

### 5.6 Create Test Plan (Xray GraphQL)

**Test Plans** organize tests by release/sprint/milestone.

```powershell
$mutation = @"
mutation {
  createTestPlan(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "Release 1.0 - Test Plan"
        description: "All tests for Release 1.0"
        labels: ["release-1.0"]
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
$response = Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

$testPlanKey = $response.data.createTestPlan.testPlan.jira.key

# Add tests to Test Plan
$mutation = @"
mutation {
  addTestsToTestPlan(
    issueId: "$testPlanKey"
    testIssueIds: ["ETR-10", "ETR-11", "ETR-12"]
  ) {
    addedTests
    warning
  }
}
"@

$body = @{ query = $mutation } | ConvertTo-Json
Invoke-RestMethod `
    -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post `
    -Headers $headers `
    -Body $body

Write-Host "Created Test Plan: $testPlanKey"
```

---

## Part 6: Key Concepts & Best Practices

### Xray Issue Types

| Issue Type | Purpose | Created Via |
|---|---|---|
| **Precondition** | Test environment setup requirements | Xray GraphQL API |
| **Test** | Manual or automated test case | Xray GraphQL API |
| **Test Set** | Logical grouping of tests (e.g., by feature) | Xray GraphQL API |
| **Test Plan** | Collection of tests for a release/sprint | Xray GraphQL API |
| **Test Execution** | Record of test run results | Xray API (after execution) |

---

### Relationships

```
Epic (ETR-1)
  ├── Story (ETR-5)
  │     ├── Tests → Test (ETR-10)
  │     ├── Tests → Test (ETR-11)
  │     └── Tests → Test (ETR-12)
  │
  └── Task (ETR-6)

Precondition (ETR-8)
  ├── Test (ETR-10)
  ├── Test (ETR-11)
  └── Test (ETR-12)

Test Set (ETR-20)
  ├── Test (ETR-10)
  ├── Test (ETR-11)
  └── Test (ETR-12)

Test Plan (ETR-25)
  ├── Test Set (ETR-20)
  └── Test (ETR-10, ETR-11, ETR-12) — direct inclusion
```

---

### Best Practices

✅ **Use labels for filtering:**
- Test ID: `TC_001`, `TC_002`
- Tags: `smoke`, `regression`, `ui`, `api`
- Sprint: `sprint-1`, `release-1.0`

✅ **Know when to use numeric IDs vs Jira keys:**
- **Numeric IDs required:** `addPreconditionsToTest` mutation
  - Must query for `issueId` first (e.g., `"10016"`)
- **Jira keys accepted:** `addTestsToTestSet`, `addTestsToTestPlan`
  - Can use Jira keys directly (e.g., `"ETR-16"`)
- When in doubt, use numeric IDs

✅ **Test type matters:**
- **Manual** — Step-by-step instructions visible in Xray UI
- **Generic** — For automated tests (no steps UI)

✅ **Bidirectional links:**
- Link tests to stories for traceability
- Link preconditions to tests for environment clarity

✅ **Token expiry:**
- Xray Bearer tokens expire after ~60 minutes
- Re-authenticate before each batch operation

---

## Part 7: Troubleshooting

### Common Issues

| Issue | Solution |
|---|---|
| **"issueId provided is not valid"** | Use numeric `issueId` (e.g., `"10016"`), not Jira keys (e.g., `"ETR-16"`) |
| **Generic test type has no steps** | Use `Manual` test type for step-by-step tests |
| **PowerShell quote escaping errors** | Use here-strings `@"..."@` for GraphQL queries |
| **"Field 'tests' argument 'limit' required"** | Add `limit` parameter to all nested queries |
| **401 Unauthorized** | Token expired — re-authenticate |
| **"Tests" link type not found** | Use Jira REST API `/issueLink`, not Xray GraphQL |

---

## Part 8: Reference Links

| Resource | URL |
|---|---|
| **Jira REST API Docs** | https://developer.atlassian.com/cloud/jira/platform/rest/v2/ |
| **Xray GraphQL API Docs** | https://docs.getxray.app/display/XRAYCLOUD/GraphQL+API |
| **Xray API Keys** | https://xray.cloud.getxray.app/api/v2/keys |
| **Jira API Tokens** | https://id.atlassian.com/manage-profile/security/api-tokens |
| **Xray Marketplace** | https://marketplace.atlassian.com/apps/1211769/xray-test-management-for-jira |

---

## Summary

You've now completed:
1. ✅ Created a new project in Jira Cloud (407 ETR)
2. ✅ Installed Xray Test Management app
3. ✅ Added Xray issue types to your project
4. ✅ Mapped Xray issue types in Xray Settings
5. ✅ Generated Jira API token for REST API
6. ✅ Generated Xray Client ID + Secret for GraphQL API
7. ✅ Learned how to create test artifacts via API (Preconditions, Tests, Test Sets, Test Plans)
8. ✅ Learned how to link tests to stories and preconditions

**Your project is now ready for test case management and automation!**
