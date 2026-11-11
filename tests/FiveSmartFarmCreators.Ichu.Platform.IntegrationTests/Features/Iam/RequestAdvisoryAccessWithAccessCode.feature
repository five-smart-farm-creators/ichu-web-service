@US-39
Feature: Request advisory access to a ranch with its access code
  As a veterinarian, I want to find a ranch by the access code that its manager shares,
  so that I can request advisory access without knowing the ranch identifier.

  Background:
    Given a verified ranch manager with a ranch

  Scenario: A veterinarian with a complete profile finds the ranch by its code and requests advisory access
    Given a veterinarian with a complete profile
    When the veterinarian looks up the ranch with the access code of the ranch manager
    Then the request returns status 200
    And the response has:
      | field    | value     |
      | ranchId  | <ranch>   |
      | name     | Los Andes |
      | region   | Cusco     |
      | district | Urubamba  |
    When the veterinarian requests advisory access to the ranch found by its code
    Then the request returns status 201
    And the response has:
      | field  | value   |
      | status | Pending |
    When the ranch manager lists the herd advisories of the ranch
    Then the response lists 1 items
    And the herd advisory of the veterinarian has:
      | field  | value   |
      | status | Pending |
