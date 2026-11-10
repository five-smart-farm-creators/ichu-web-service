@US-21
Feature: Record a health event during field work
  As a field operator, I want to record what I observe in cattle at the moment I observe it,
  so that the information is not lost at the end of the working day.

  Background:
    Given a ranch manager with monitored cattle

  Scenario: A field event is stored with its cattle, type, date and author and joins the cattle's history
    When the ranch manager records the field event "health check" of type "HealthEvent" for the cattle "monitored cattle" with observation "Mild lameness"
    Then the request returns status 201
    And the response has:
      | field       | value                  |
      | animalId    | <monitored cattle>     |
      | type        | HealthEvent            |
      | isClinical  | false                  |
      | authorId    | <manager account>      |
      | observation | Mild lameness          |
      | occurredAt  | <time of health check> |
    When the ranch manager requests the livestock operations of the cattle "monitored cattle" for the last 1 days
    Then the response lists 1 items
    And the operation "health check" has:
      | field | value       |
      | type  | HealthEvent |

  Scenario Outline: A field event without its mandatory <missing> is not recorded
    When the ranch manager records a field event without its <missing>
    Then the request is refused with status 400 and code "OperationsMonitoring.RequiredFieldsMissing"
    When the ranch manager requests the livestock operations of the cattle "monitored cattle" for the last 1 days
    Then the response lists 0 items

    Examples:
      | missing |
      | cattle  |
      | type    |
