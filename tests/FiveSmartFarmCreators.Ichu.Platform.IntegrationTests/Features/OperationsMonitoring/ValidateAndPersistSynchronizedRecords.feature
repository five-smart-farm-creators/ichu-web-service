@TS-07
Feature: Validate and persist the records synchronized by the edge node
  As a portable edge node, I need the central API to validate and confirm my synchronized records,
  so that the cloud keeps the information captured offline without duplicating it.

  Background:
    Given a ranch manager with monitored cattle

  Scenario: A reading is attributed to the cattle whose collar was assigned at its capture time
    Given the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    Then the entry "first" is "Accepted"
    When the ranch manager requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the response has:
      | field      | value |
      | totalCount | 1     |
    And the telemetry item "first" has:
      | field                 | value           |
      | recordedAt            | <time of first> |
      | receivedAt            | not null        |
      | bodyTemperature.value | 38.6            |

  Scenario: A repeated identifier with the same content is a duplicate
    Given the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    Given the edge gateway holds a copy "again" of the reading "first"
    When the edge gateway synchronizes its pending records
    Then the entry "again" is "Duplicate"

  Scenario: A repeated identifier with different content is a conflict that keeps the original reading
    Given the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 40.2
    When the edge gateway synchronizes its pending records
    Given the edge gateway holds a copy "altered" of the reading "first" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    Then the entry "altered" is "Conflict"
    When the ranch manager requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the telemetry item "first" has:
      | field                 | value |
      | bodyTemperature.value | 40.2  |

  Scenario: A reading captured without an effective assignment is kept as device data and left out of the cattle's history
    Given a collar "unassigned collar" is registered without an assignment
    And the edge gateway holds a reading "unassigned reading" from the collar "unassigned collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    Then the entry "unassigned reading" is "Accepted" with a reason containing "No device assignment"
    When the ranch manager requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the response has:
      | field      | value |
      | totalCount | 0     |

  Scenario: A later reassignment keeps the attribution that the reading had at its capture
    Given the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 38.6
    And cattle "second cattle" is registered in the ranch
    When the edge gateway synchronizes its pending records
    And the ranch manager releases the collar "monitored collar"
    And the ranch manager assigns the collar "monitored collar" to the cattle "second cattle"
    And the ranch manager requests the telemetry of the cattle "monitored cattle" for the last 1 days
    Then the response has:
      | field      | value |
      | totalCount | 1     |
    When the ranch manager requests the telemetry of the cattle "second cattle" for the last 1 days
    Then the response has:
      | field      | value |
      | totalCount | 0     |

  Scenario: A collar of another ranch is rejected while the valid entries of the batch are accepted
    Given a collar "foreign collar" is registered in another ranch
    And the edge gateway holds a reading "foreign reading" from the collar "foreign collar" with body temperature 38.6
    And the edge gateway holds a reading "valid reading" from the collar "monitored collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    Then the entry "foreign reading" is "Rejected" with a reason containing "no access"
    And the entry "valid reading" is "Accepted"

  Scenario: A capture time beyond the clock tolerance is rejected
    Given the edge gateway holds a reading "future reading" from the collar "monitored collar" with body temperature 38.6 captured 10 minutes from now
    When the edge gateway synchronizes its pending records
    Then the entry "future reading" is "Rejected" with a reason containing "ahead of the server clock"

  Scenario: An old capture is not rejected merely because it arrives late
    Given the edge gateway holds a reading "old reading" from the collar "monitored collar" with body temperature 38.6 captured -2880 minutes from now
    When the edge gateway synchronizes its pending records
    Then the entry "old reading" is "Accepted"
