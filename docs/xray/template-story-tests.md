# Test Cases — {STORY-KEY}: {Story Name}

**Epic:** {EPIC-KEY} — {Epic Name}  
**Story:** {STORY-KEY} — {Story Summary} ← Parent requirement for all test cases  
**Xray Create Subtask:** {SUBTASK-KEY} — [QA-XRAY-CREATE] Manual test cases in Xray  
**Xray Execute Subtask:** {SUBTASK-KEY} — [QA-XRAY-EXEC] Execute manual test cases in Xray

> **Note:** All test cases are linked to **{STORY-KEY}** via "Tests" relationship in Jira.  
> Test cases are created as type **Manual** with step-by-step actions and expected results.  
> Gherkin/Cucumber syntax is used only in `.feature` files in `ETR.ToDo.Tests.Playwright`.

---

## Xray Artifacts

| Artifact | Key | Description |
|---|---|---|
| Test Set | {TEST-SET-KEY} | Logical grouping of all test cases for this story |
| Test Plan | {TEST-PLAN-KEY} | Sprint/release test plan containing this Test Set |
| Preconditions | {PRECONDITION-KEYS} | Shared setup requirements |

---

## {TC_ID} — {Test Case Name}

**Tags:** `@{STORY-KEY} @{SUBTASK-KEY} @{TC_ID} @{suite-tags}`  
**Xray Test Issue:** {TEST-KEY}  
**Suite:** {smoke | regression | integration}  
**Preconditions:** {PRECONDITION-KEYS (if applicable)}

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | {Action step 1} | {Expected result 1} |
| 2 | {Action step 2} | {Expected result 2} |
| 3 | {Action step 3} | {Expected result 3} |

### Feature File Scenario (Playwright — created separately)

```gherkin
@{STORY-KEY} @{SUBTASK-KEY} @{TC_ID} @{suite-tag}
Scenario: {Scenario description in natural language}
  Given {precondition / setup step}
  When {action taken by user}
  Then {expected outcome}
  And {additional verification}
```

---

## Summary

| Test Case | Description | Suite | Preconditions |
|---|---|---|---|
| {TC_ID_001} | {Brief description} | smoke, regression | {PC if any} |
| {TC_ID_002} | {Brief description} | regression | {PC if any} |
| {TC_ID_003} | {Brief description} | smoke | {PC if any} |

---

## Xray Structure

```
{EPIC-KEY} (Epic)
└── {STORY-KEY} (Story) ← Parent requirement
    ├── {SUBTASK-CREATE} (Subtask: Create test cases)
    ├── {SUBTASK-EXECUTE} (Subtask: Execute test cases)
    ├── {TEST-SET-KEY} (Test Set)
    │   ├── {TEST-KEY-001} (Test) → Precondition: {PC-KEY}
    │   ├── {TEST-KEY-002} (Test) → Precondition: {PC-KEY}
    │   └── {TEST-KEY-00N} (Test)
    └── {TEST-PLAN-KEY} (Test Plan)
        └── Contains: {TEST-SET-KEY}
```

---

## Preconditions Defined

### {PC-KEY-1} — {Precondition Name}
**Description:** {What setup is required}  
**Used By:** {TEST-KEY-001, TEST-KEY-002}

### {PC-KEY-2} — {Precondition Name}
**Description:** {What setup is required}  
**Used By:** {TEST-KEY-003, TEST-KEY-004}

---

## Workflow to Create This Structure

### 1. Create Preconditions
```powershell
# See docs/xray/api-reference.md for full examples
# Create shared setup requirements first
```

### 2. Create Test Cases
```powershell
# Create Manual test with steps
# Link to parent story via "Tests" relationship
```

### 3. Link Preconditions to Tests
```powershell
# Use addPreconditionsToTest mutation
```

### 4. Create Test Set
```powershell
# Group all tests for this story
```

### 5. Create Test Plan
```powershell
# Sprint/release level planning
```

### 6. Add Tests to Test Set and Test Plan
```powershell
# Organize tests into logical groupings
```

---

## Gherkin Feature File Template

Save as: `ETR.ToDo.Tests.Playwright/tests/{feature-name}.feature`

```gherkin
@{STORY-KEY} @{SUBTASK-CREATE-KEY}
Feature: {Feature Name}
  As a {user type}
  I want to {action}
  So that {benefit}

  Background:
    Given {common setup for all scenarios}

  @{TC_ID_001} @smoke @regression
  Scenario: {Scenario 1 description}
    Given {precondition}
    When {action}
    Then {expected result}
    And {additional assertion}

  @{TC_ID_002} @regression
  Scenario: {Scenario 2 description}
    Given {precondition}
    When {action}
    Then {expected result}

  @{TC_ID_003} @smoke
  Scenario Outline: {Parameterized scenario description}
    Given {precondition with <parameter>}
    When {action with <parameter>}
    Then {expected result}

    Examples:
      | parameter |
      | value1    |
      | value2    |
```

---

## Checklist for New Story Test Cases

- [ ] Create Preconditions (if shared setup exists)
- [ ] Create Test Cases (Manual type with steps)
- [ ] Link Preconditions to Tests
- [ ] Link Tests to Story (via "Tests" relationship)
- [ ] Create Test Set for the story
- [ ] Add all tests to Test Set
- [ ] Add tests to Test Plan
- [ ] Create `.feature` file with matching `@TC_` tags
- [ ] Create step definitions in Playwright
- [ ] Update this MD file with actual keys
- [ ] Transition {SUBTASK-CREATE} to Done
