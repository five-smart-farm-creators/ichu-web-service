@TS-06
Feature: Synchronize the portable edge node with the central API
  As a portable edge node, I want to send my readings, alerts and pending changes to the central API,
  so that the local records reach the applications when connectivity exists.

  Background:
    Given a ranch manager with monitored cattle

  Scenario: Every entry of a batch receives its own result and a rejected entry does not block the others
    Given the ranch has the fever threshold configured
    And a collar "foreign collar" is registered in another ranch
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 40.2
    And the edge gateway holds a fever alert "fever alert" citing the reading "first"
    And the edge gateway holds a reading "stray" from the collar "foreign collar" with body temperature 38.6
    When the edge gateway synchronizes its pending records
    Then the entry "first" is "Accepted"
    And the entry "fever alert" is "Accepted"
    And the entry "stray" is "Rejected"

  Scenario: Resending the same batch confirms the stored records without duplicating them
    Given the ranch has the fever threshold configured
    And the edge gateway holds a reading "first" from the collar "monitored collar" with body temperature 40.2
    And the edge gateway holds a fever alert "fever alert" citing the reading "first"
    When the edge gateway synchronizes its pending records
    And the edge gateway resends the last batch
    Then the entry "first" is "Duplicate"
    And the entry "fever alert" is "Duplicate"
    When the ranch manager lists the alerts of the ranch
    Then the response lists 1 items
