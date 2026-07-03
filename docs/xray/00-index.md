# ETR.ToDo — Xray Test Case Index

## Purpose

This folder documents test cases written for Xray, linked to Jira stories. Each file maps to one Epic and contains all test cases for its stories.

---

## Test Case ID Convention

Format: `TC_{STORY_KEY}_{3-digit-sequence}`

| Example | Story | Meaning |
|---|---|---|
| `TC_ETR_6_001` | ETR-6 | 1st test case for Public Landing Page |
| `TC_ETR_6_002` | ETR-6 | 2nd test case for Public Landing Page |

When a test case is pushed to Xray, it receives a Jira issue number (e.g. `ETR-16`). Both IDs coexist — the `TC_` ID is the stable automation handle used in feature files.

**Parent Relationship:** All test cases are linked to their parent **Story** (e.g., ETR-6) via Jira's "Tests" link type. This creates the relationship: _ETR-16 **tests** ETR-6_.

---

## Xray Test Type

All test cases are created in Xray as type **Manual**:
- Steps have three columns: Action, Data, Expected Result
- Steps are written in plain English — no Gherkin syntax in Xray
- Gherkin/Cucumber format is used **only** in `.feature` files inside `ETR.ToDo.Tests.Playwright`
- The `@TC_ETR_6_001` tag in the feature file links execution results back to the Xray test case

---

## Xray Artifact Types

| Artifact | Purpose | When to Create | Required? |
|---|---|---|---|
| **Test** | The test case itself (steps, summary, labels) | For each test scenario | **Yes** |
| **Precondition** | Shared setup requirements (e.g., "User logged in") | Before tests if common setup exists | Optional but recommended |
| **Test Set** | Logical groupings of tests (like test suites) | To organize tests by feature/story | Recommended |
| **Test Plan** | High-level execution plans (sprint/release) | For sprint/release planning | Recommended |
| **Test Execution** | Actual execution records (pass/fail per run) | Automatically created when tests run | Auto-generated |

### Example Structure

```
Story (ETR-6)
├── Preconditions (ETR-28, ETR-29, ETR-30, ETR-31)
├── Tests (ETR-16, ETR-17, ETR-19, ETR-20, ETR-21, ETR-26, ETR-27)
├── Test Set (ETR-32) ← Groups all 7 tests
└── Test Plan (ETR-33) ← Contains Test Set for sprint planning
```

---

## Tagging Strategy

Every `.feature` scenario carries four tag types:

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_001 @smoke
Scenario: Unauthenticated user sees the public landing page
```

| Tag | Type | Purpose |
|---|---|---|
| `@ETR-6` | Story reference | Parent Jira story (the requirement being tested) |
| `@ETR-14` | Xray create task | Links test creation back to the [QA-XRAY-CREATE] work item |
| `@TC_ETR_6_001` | Test case ID | Maps 1:1 to an Xray Test issue — used for pass/fail tracking |
| `@smoke` / `@regression` | Suite tag | Controls which suite the scenario belongs to |

**Note:** ETR-14 is a **work tracking** subtask. Test cases are **linked to ETR-6** (the Story) in Jira via the "Tests" relationship, not to ETR-14.

---

## Suite Tags

| Tag | Meaning |
|---|---|
| `@smoke` | Critical path — run on every deploy |
| `@regression` | Full regression suite |

---

## Execution Flow

```
1. playwright test --grep @ETR-6        ← run by story
   playwright test --grep @smoke         ← run by suite
   playwright test --grep @TC_ETR_6_001  ← run single test case

2. JSON report generated automatically

3. Report pushed to Xray GraphQL API
   → Creates Test Execution under ETR-15

4. Xray marks each @TC_ tag pass / fail

5. ETR-14 and ETR-15 transition to Done
```

---

## Complete Workflow — Creating Test Cases for a Story

### Step 1: Create Preconditions (Optional but Recommended)
Identify shared setup requirements across test cases:
- User authentication states (logged in, logged out)
- Device/viewport configurations (desktop, mobile)
- Data prerequisites (specific test data loaded)

```powershell
# See docs/xray/api-reference.md for API examples
createPrecondition(jira: { 
  fields: { 
    project: { key: "ETR" }, 
    summary: "User is logged in as Basic user", 
    description: "Valid Basic user authenticated with active session",
    labels: ["ETR-6", "precondition"] 
  } 
})
```

### Step 2: Create Test Cases
For each test scenario, create a Manual test with steps:

```powershell
createTest(
  testType: { name: "Manual" }
  steps: [
    { action: "Navigate to /", data: "Page loads", result: "Dashboard visible" }
  ]
  jira: { 
    fields: { 
      project: { key: "ETR" }, 
      summary: "Test summary", 
      labels: ["TC_ETR_6_001", "ETR-6", "ETR-14", "smoke"] 
    } 
  }
)
```

### Step 3: Link Preconditions to Tests
```powershell
# ⚠️ Use numeric issueId (e.g., "10016"), not Jira keys!
addPreconditionsToTest(
  issueId: "10016",    # Numeric ID from getTests query
  preconditionIssueIds: ["10028"]  # Numeric IDs from getPreconditions query
)
```

### Step 4: Link Tests to Parent Story
Create "Tests" relationship in Jira:

```powershell
# Jira REST API
POST /rest/api/2/issueLink
{
  "type": { "name": "Test" },
  "inwardIssue": { "key": "ETR-6" },    # Story
  "outwardIssue": { "key": "ETR-16" }   # Test
}
# Result: ETR-16 tests ETR-6
```

### Step 5: Create Test Set
Group all tests for the story:

```powershell
createTestSet(jira: { 
  fields: { 
    project: { key: "ETR" }, 
    summary: "ETR-6 Landing Page Tests", 
    labels: ["ETR-6", "test-set"] 
  } 
})
```

### Step 6: Add Tests to Test Set
```powershell
addTestsToTestSet(
  issueId: "ETR-32",  # Test Set
  testIssueIds: ["ETR-16", "ETR-17", "ETR-19", ...]
)
```

### Step 7: Create Test Plan (Sprint/Release Level)
```powershell
createTestPlan(jira: { 
  fields: { 
    project: { key: "ETR" }, 
    summary: "Sprint 1 - Dashboard Tests", 
    labels: ["sprint-1", "test-plan"] 
  } 
})
```

### Step 8: Add Tests to Test Plan
```powershell
addTestsToTestPlan(
  issueId: "ETR-33",  # Test Plan
  testIssueIds: ["ETR-16", "ETR-17", ...]
)
```

### Step 9: Create Gherkin Feature Files
In `ETR.ToDo.Tests.Playwright/tests/`, create `.feature` files with matching `@TC_` tags:

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_001 @smoke
Scenario: Landing page loads for unauthenticated user
  Given I am not logged in
  When I navigate to "/"
  Then the dashboard home page is visible
```

### Step 10: Execute Tests & Push Results
```bash
# Run tests
playwright test --grep @smoke

# Push results to Xray (via GraphQL API)
# Creates Test Execution linked to ETR-15
```

### Step 11: Transition Subtasks to Done
- **ETR-14** (Create test cases) → Done after steps 1-8
- **ETR-15** (Execute test cases) → Done after step 10

---

## Files

| File | Epic | Jira |
|---|---|---|
| [ETR-6-public-landing-page.md](ETR-6-public-landing-page.md) | Dashboard | ETR-5 |

---

## Reference Documentation

| Document | Purpose |
|---|---|
| [jira-xray-standalone-setup.md](jira-xray-standalone-setup.md) | **GENERIC SETUP** — Add new project to Jira, install Xray, generate credentials (no app-specific code) |
| [setup-guide.md](setup-guide.md) | **ETR.ToDo PROJECT SETUP** — Complete workflow specific to this codebase |
| [api-reference.md](api-reference.md) | Complete API examples for Xray GraphQL & Jira REST API |
| [template-story-tests.md](template-story-tests.md) | Reusable template for creating test cases for new stories |
| `ETR-6-public-landing-page.md` | Example implementation for ETR-6 (7 test cases) |

---

## Quick Links

- **Jira Project:** https://ai-autopilot.atlassian.net/projects/ETR
- **Xray GraphQL Endpoint:** https://xray.cloud.getxray.app/api/v2/graphql
- **Xray Documentation:** https://docs.getxray.app/display/XRAYCLOUD/GraphQL+API
- **Playwright Tests:** `ETR.ToDo.Tests.Playwright/tests/`
