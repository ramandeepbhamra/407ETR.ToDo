# Xray & Jira API Reference

This document provides working API examples for creating and managing Jira issues and Xray test artifacts programmatically.

---

## Authentication

### Xray Cloud API
```powershell
$authBody = @{
    client_id = $xrayClientId
    client_secret = $xrayClientSecret
} | ConvertTo-Json

$token = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/authenticate" `
    -Method Post `
    -ContentType "application/json" `
    -Body $authBody
```

**Returns:** Bearer token (valid for ~60 minutes)

**Endpoint:** `https://xray.cloud.getxray.app/api/v2/graphql`

**Headers:** `Authorization: Bearer $token`

---

### Jira REST API
```powershell
$jiraAuth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("email:apiToken"))
```

**Endpoint:** `https://your-domain.atlassian.net/rest/api/2/`

**Headers:** `Authorization: Basic $jiraAuth`

---

## Xray Test Artifacts

### 1. Create Precondition
```graphql
mutation {
  createPrecondition(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "User is not authenticated"
        description: "Browser has no auth token, user is logged out"
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
```

**PowerShell:**
```powershell
$mutationBody = @"
{
  "query": "mutation { createPrecondition(jira: { fields: { project: { key: \"ETR\" }, summary: \"User is not authenticated\", description: \"Browser has no auth token\", labels: [\"ETR-6\", \"precondition\"] } }) { precondition { issueId jira(fields: [\"key\", \"summary\"]) } } }"
}
"@
$response = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $mutationBody
$preconditionKey = $response.data.createPrecondition.precondition.jira.key
```

---

### 2. Create Test (Manual with Steps)
```graphql
mutation {
  createTest(
    testType: { name: "Manual" }
    steps: [
      { action: "Open browser", data: "Browser opens", result: "Browser is ready" }
      { action: "Navigate to /", data: "Page loads", result: "Dashboard visible" }
    ]
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "Test case summary"
        labels: ["TC_ETR_6_001", "ETR-6", "ETR-14", "smoke"]
      }
    }
  ) {
    test {
      issueId
      jira(fields: ["key", "summary"])
    }
  }
}
```

**PowerShell:**
```powershell
$mutationBody = @"
{
  "query": "mutation { createTest(testType: { name: \"Manual\" }, steps: [{ action: \"Open browser\", data: \"Browser opens\", result: \"Browser is ready\" }], jira: { fields: { project: { key: \"ETR\" }, summary: \"Test case summary\", labels: [\"TC_ETR_6_001\", \"ETR-6\", \"smoke\"] } }) { test { issueId jira(fields: [\"key\"]) } } }"
}
"@
$response = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $mutationBody
$testKey = $response.data.createTest.test.jira.key
```

---

### 3. Link Precondition to Test

**⚠️ Important:** Use **numeric `issueId`** (not Jira keys)!

```graphql
mutation {
  addPreconditionsToTest(
    issueId: "10016"
    preconditionIssueIds: ["10028", "10029"]
  ) {
    addedPreconditions
    warning
  }
}
```

**Get numeric IDs first:**
```graphql
{
  getTests(jql: "key IN (ETR-16, ETR-17)", limit: 10) {
    results {
      issueId
      jira(fields: ["key"])
    }
  }
  getPreconditions(jql: "key IN (ETR-28, ETR-29)", limit: 10) {
    results {
      issueId
      jira(fields: ["key"])
    }
  }
}
```

**PowerShell:**
```powershell
# Get numeric IDs
$query = @"
{ getTests(jql: "key = ETR-16", limit: 1) { results { issueId jira(fields: ["key"]) } } }
"@
$response = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } `
    -Body (@{ query = $query } | ConvertTo-Json)
$testId = $response.data.getTests.results[0].issueId

# Link preconditions
$mutationBody = @"
{
  "query": "mutation { addPreconditionsToTest(issueId: \"$testId\", preconditionIssueIds: [\"10028\"]) { addedPreconditions warning } }"
}
"@
Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $mutationBody
```

---

### 4. Create Test Set
```graphql
mutation {
  createTestSet(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "ETR-6 Landing Page Tests"
        description: "Test Set for Public Landing Page"
        labels: ["ETR-6", "test-set", "smoke"]
      }
    }
  ) {
    testSet {
      issueId
      jira(fields: ["key", "summary"])
    }
  }
}
```

---

### 5. Add Tests to Test Set
```graphql
mutation {
  addTestsToTestSet(
    issueId: "ETR-32"
    testIssueIds: ["ETR-16", "ETR-17", "ETR-19"]
  ) {
    addedTests
    warning
  }
}
```

---

### 6. Create Test Plan
```graphql
mutation {
  createTestPlan(
    jira: {
      fields: {
        project: { key: "ETR" }
        summary: "Sprint 1 - Dashboard Tests"
        description: "Test Plan for Sprint 1"
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
```

---

### 7. Add Tests to Test Plan
```graphql
mutation {
  addTestsToTestPlan(
    issueId: "ETR-33"
    testIssueIds: ["ETR-16", "ETR-17", "ETR-19", "ETR-20", "ETR-21", "ETR-26", "ETR-27"]
  ) {
    addedTests
    warning
  }
}
```

---

### 8. Query Tests by JQL
```graphql
query {
  getTests(jql: "project = ETR AND labels = TC_ETR_6_001", limit: 10) {
    total
    results {
      issueId
      jira(fields: ["key", "summary", "labels"])
    }
  }
}
```

**PowerShell:**
```powershell
$queryBody = @"
{
  "query": "{ getTests(jql: \"project = ETR AND labels = TC_ETR_6_001\", limit: 10) { total results { jira(fields: [\"key\", \"summary\"]) } } }"
}
"@
$response = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $queryBody
```

---

## Jira REST API

### 1. Create Issue Link (Tests Relationship)
```powershell
$linkBody = @{
    type = @{ name = "Test" }
    inwardIssue = @{ key = "ETR-6" }
    outwardIssue = @{ key = "ETR-16" }
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri "https://ai-autopilot.atlassian.net/rest/api/2/issueLink" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = $jiraAuth } -Body $linkBody
```

**Result:** ETR-16 **tests** ETR-6

---

### 2. Get Issue Link Types
```powershell
$response = Invoke-RestMethod -Uri "https://ai-autopilot.atlassian.net/rest/api/2/issueLinkType" `
    -Headers @{ Authorization = $jiraAuth }
$response.issueLinkTypes | Select-Object id, name, inward, outward
```

**Common Link Types:**
- `Blocks` — is blocked by / blocks
- `Relates` — relates to / relates to
- `Test` — is tested by / tests

---

### 3. Create Jira Issue (Story/Task)
```powershell
$issueBody = @{
    fields = @{
        project = @{ key = "ETR" }
        issuetype = @{ name = "Story" }
        summary = "Story summary"
        description = "Story description"
        labels = @("frontend", "dashboard")
    }
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "https://ai-autopilot.atlassian.net/rest/api/2/issue" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = $jiraAuth } -Body $issueBody
$issueKey = $response.key
```

---

### 4. Query Issues by JQL
```powershell
$jql = "project = ETR AND issuetype = Story"
$encodedJql = [System.Web.HttpUtility]::UrlEncode($jql)
$response = Invoke-RestMethod -Uri "https://ai-autopilot.atlassian.net/rest/api/2/search?jql=$encodedJql" `
    -Headers @{ Authorization = $jiraAuth }
$response.issues | Select-Object key, @{Name='summary';Expression={$_.fields.summary}}
```

---

## Complete Workflow Example

### Create Test Case with All Relationships
```powershell
# 1. Authenticate
$token = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/authenticate" `
    -Method Post -ContentType "application/json" `
    -Body (@{ client_id = $clientId; client_secret = $clientSecret } | ConvertTo-Json)

# 2. Create Precondition
$pc = @"
{ "query": "mutation { createPrecondition(jira: { fields: { project: { key: \"ETR\" }, summary: \"User logged in\", labels: [\"precondition\"] } }) { precondition { jira(fields: [\"key\"]) } } }" }
"@
$pcResponse = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $pc
$pcKey = $pcResponse.data.createPrecondition.precondition.jira.key

# 3. Create Test
$test = @"
{ "query": "mutation { createTest(testType: { name: \"Manual\" }, steps: [{ action: \"Do something\", data: \"Data\", result: \"Expected\" }], jira: { fields: { project: { key: \"ETR\" }, summary: \"Test name\", labels: [\"TC_ETR_6_999\", \"ETR-6\"] } }) { test { jira(fields: [\"key\"]) } } }" }
"@
$testResponse = Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $test
$testKey = $testResponse.data.createTest.test.jira.key
$testId = $testResponse.data.createTest.test.issueId
$pcId = $pcResponse.data.createPrecondition.precondition.issueId

# 4. Link Precondition to Test (use numeric IDs, not Jira keys!)
$link = @"
{ "query": "mutation { addPreconditionsToTest(issueId: \"$testId\", preconditionIssueIds: [\"$pcId\"]) { addedPreconditions } }" }
"@
Invoke-RestMethod -Uri "https://xray.cloud.getxray.app/api/v2/graphql" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $token" } -Body $link

# 5. Link Test to Story (Jira REST API)
$jiraAuth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("email:token"))
$linkBody = @{ type = @{ name = "Test" }; inwardIssue = @{ key = "ETR-6" }; outwardIssue = @{ key = $testKey } } | ConvertTo-Json -Depth 5
Invoke-RestMethod -Uri "https://ai-autopilot.atlassian.net/rest/api/2/issueLink" `
    -Method Post -ContentType "application/json" `
    -Headers @{ Authorization = $jiraAuth } -Body $linkBody
```

---

## Notes

- **Token Expiration:** Xray tokens expire after ~60 minutes — re-authenticate as needed
- **Rate Limiting:** Xray Cloud API: 600 requests/hour per API key
- **JSON Escaping:** When embedding GraphQL in JSON, escape quotes: `\"key\"`
- **Field Names:** Use GraphQL schema introspection to verify field names:
  ```graphql
  query {
    __type(name: "CreateTestInput") {
      fields {
        name
        type { name }
      }
    }
  }
  ```
