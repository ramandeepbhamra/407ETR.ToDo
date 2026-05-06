Feature: User Authentication - Login
    As a user of the ETR.ToDo application
    I want to authenticate with my credentials
    So that I can access protected resources

Background:
    Given the API is running
    And the following users exist in the system:
        | Email                 | Password     | Role  |
        | testuser@example.com  | Password123! | Basic |
        | admin@example.com     | Admin123!    | Admin |

Scenario: Successful login with valid credentials
    Given I am not authenticated
    When I attempt to login with the following credentials:
        | Email                | Password     |
        | testuser@example.com | Password123! |
    Then the response status code should be 200
    And the response should contain an access token
    And the response should contain a refresh token
    And the access token should be valid
    And the token should contain the user email "testuser@example.com"

Scenario: Failed login with invalid password
    Given I am not authenticated
    When I attempt to login with the following credentials:
        | Email                | Password        |
        | testuser@example.com | WrongPassword!! |
    Then the response status code should be 401
    And the response should contain error message "Invalid email or password"

Scenario: Failed login with non-existent user
    Given I am not authenticated
    When I attempt to login with the following credentials:
        | Email                  | Password     |
        | nonexistent@email.com  | Password123! |
    Then the response status code should be 401
    And the response should contain error message "Invalid email or password"

Scenario: Failed login with empty email
    Given I am not authenticated
    When I attempt to login with the following credentials:
        | Email | Password     |
        |       | Password123! |
    Then the response status code should be 400
    And the response should contain validation errors

Scenario: Failed login with empty password
    Given I am not authenticated
    When I attempt to login with the following credentials:
        | Email                | Password |
        | testuser@example.com |          |
    Then the response status code should be 400
    And the response should contain validation errors

Scenario: Admin user successful login
    Given I am not authenticated
    When I attempt to login with the following credentials:
        | Email             | Password  |
        | admin@example.com | Admin123! |
    Then the response status code should be 200
    And the response should contain an access token
    And the token should contain the user email "admin@example.com"
    And the token should contain role "Admin"
