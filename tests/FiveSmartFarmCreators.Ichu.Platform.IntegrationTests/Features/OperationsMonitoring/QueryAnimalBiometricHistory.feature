@US-14
Feature: Query the biometric history of cattle
  As a veterinarian, I want to review the temperature, activity and rumination of cattle over a period,
  so that my diagnosis rests on data instead of a single observation.

  Background:
    Given a ranch manager with monitored cattle

  Scenario: An authorized veterinarian reads the valid readings of the period with units and capture time
    Given a veterinarian with a valid authorization over the ranch
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    And the veterinarian requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the response has:
      | field      | value |
      | totalCount | 1     |
    And the telemetry item "first" has:
      | field                  | value           |
      | recordedAt             | <time of first> |
      | bodyTemperature.value  | 38.6            |
      | bodyTemperature.unit   | celsius         |
      | bodyTemperature.status | Valid           |

  Scenario: Rumination is reported when a validated measurement exists
    Given a veterinarian with a valid authorization over the ranch
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    And the reading "first" also reports rumination of 420 minutes
    When the edge gateway synchronizes its pending records
    And the veterinarian requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the telemetry item "first" has:
      | field                    | value   |
      | ruminationMinutes.value  | 420     |
      | ruminationMinutes.unit   | minutes |
      | ruminationMinutes.status | Valid   |

  Scenario: Rumination is marked absent without an invented value when no measurement exists
    Given a veterinarian with a valid authorization over the ranch
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    And the veterinarian requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the telemetry item "first" has:
      | field                    | value  |
      | ruminationMinutes.status | Absent |
      | ruminationMinutes.value  | null   |

  Scenario: The veterinarian reads the alerts and clinical events of the cattle next to its readings
    Given the ranch has the fever threshold configured
    And a veterinarian with a valid authorization over the ranch
    And the edge gateway holds a reading "fever" from the collar "monitored collar" with body temperature 40.2
    And the edge gateway holds a fever alert "fever alert" citing the reading "fever"
    When the edge gateway synchronizes its pending records
    And the ranch manager records the clinical intervention "vaccination" of type "Vaccination" for the cattle "monitored cattle" with product "Foot-and-mouth vaccine" and dose "2 ml"
    And the veterinarian lists the alerts of the ranch
    Then the alert "fever alert" has:
      | field  | value   |
      | status | Pending |
    When the veterinarian requests the livestock operations of the cattle "monitored cattle" for the last 1 days
    Then the response lists 1 items
    And the operation "vaccination" has:
      | field      | value                  |
      | isClinical | true                   |
      | product    | Foot-and-mouth vaccine |

  Scenario: A period longer than the retained data returns the oldest retained reading
    Given a veterinarian with a valid authorization over the ranch
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    And the veterinarian requests the telemetry of the cattle "monitored cattle" for the last 730 days
    Then the response has:
      | field                    | value           |
      | oldestRetainedRecordedAt | <time of first> |
      | totalCount               | 1               |

  Scenario: A revoked authorization rejects the history query
    Given a veterinarian with a valid authorization over the ranch
    When the ranch owner revokes the authorization of the veterinarian
    And the veterinarian requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the request is refused with status 403 and code "OperationsMonitoring.Forbidden"
