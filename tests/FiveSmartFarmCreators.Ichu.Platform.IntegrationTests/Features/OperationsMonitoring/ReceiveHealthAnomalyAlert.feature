@US-16
Feature: Receive an alert for a health anomaly
  As a livestock administrator, I want to be alerted when the temperature or activity of cattle deviates,
  so that I can review the cattle and request professional care when needed.

  Background:
    Given a ranch manager with monitored cattle

  Scenario: The alert carries the cattle, the value, the threshold, the reading and the capture time
    Given the ranch has the fever threshold configured
    And the edge gateway holds a reading "fever" from the collar "monitored collar" with body temperature 40.2
    And the edge gateway holds a fever alert "fever alert" citing the reading "fever"
    When the edge gateway synchronizes its pending records
    Then the entry "fever alert" is "Accepted"
    When the ranch manager lists the alerts of the ranch
    Then the alert "fever alert" has:
      | field             | value              |
      | animalId          | <monitored cattle> |
      | type              | Temperature        |
      | status            | Pending            |
      | detectedValue     | 40.2               |
      | thresholdMinValue | 37.5               |
      | thresholdMaxValue | 39.5               |
      | unit              | celsius            |
      | readingIds        | <fever>            |
      | raisedAt          | <time of fever>    |

  Scenario: A later reading of the same condition joins the open alert instead of raising another one
    Given the ranch has the fever threshold configured
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 40.2
    And the edge gateway holds a fever alert "first alert" citing the reading "first"
    When the edge gateway synchronizes its pending records
    Then the entry "first alert" is "Accepted"
    Given the edge gateway holds a reading "second" from the collar "monitored collar" with body temperature 40.4
    And the edge gateway holds a fever alert "second alert" citing the reading "second"
    When the edge gateway synchronizes its pending records
    Then the entry "second alert" is "Duplicate" and joins the alert "first alert"
    When the ranch manager lists the alerts of the ranch
    Then the response lists 1 items
    And the alert "first alert" has:
      | field      | value             |
      | readingIds | <first>, <second> |

  Scenario: An invalid measurement never supports an alert
    Given the ranch has the fever threshold configured
    And the edge gateway holds a reading "faulty" from the collar "monitored collar" with an invalid body temperature because "sensor fault"
    And the edge gateway holds a fever alert "faulty alert" citing the reading "faulty"
    When the edge gateway synchronizes its pending records
    Then the entry "faulty" is "Accepted"
    And the entry "faulty alert" is "Rejected" with a reason containing "does not support"
    When the ranch manager lists the alerts of the ranch
    Then the response lists 0 items

  Scenario: Evidence captured while the collar was not assigned cannot raise an alert
    Given the ranch has the fever threshold configured
    And a collar "unassigned collar" is registered without an assignment
    And the edge gateway holds a reading "loose" from the collar "unassigned collar" with body temperature 40.2
    And the edge gateway holds a fever alert "loose alert" citing the reading "loose"
    When the edge gateway synchronizes its pending records
    Then the entry "loose" is "Accepted"
    And the entry "loose alert" is "Rejected" with a reason containing "attributed to an animal"
    When the ranch manager lists the alerts of the ranch
    Then the response lists 0 items

  Scenario: No alert is raised before a threshold has been published
    Given the edge gateway holds a reading "untreated" from the collar "monitored collar" with body temperature 40.2
    And the edge gateway holds a fever alert "untreated alert" citing the reading "untreated"
    When the edge gateway synchronizes its pending records
    Then the entry "untreated alert" is "Rejected" with a reason containing "never published"
    When the ranch manager lists the alerts of the ranch
    Then the response lists 0 items
