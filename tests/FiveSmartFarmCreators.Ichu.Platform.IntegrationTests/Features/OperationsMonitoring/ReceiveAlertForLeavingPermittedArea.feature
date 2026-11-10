@US-17
Feature: Receive an alert when cattle leave the permitted area
  As a livestock administrator, I want to be alerted when cattle leave their assigned geofence,
  so that I reduce the risk of cattle rustling or loss.

  Background:
    Given a ranch manager with monitored cattle
    And a lot "North pasture" exists in the ranch
    And the cattle "monitored cattle" belongs to the lot "North pasture"
    And the lot "North pasture" has the geofence "North boundary"

  Scenario: Cattle outside its geofence raises a security alert with its position and distance to the boundary
    Given the edge gateway holds a reading "outside" from the collar "monitored collar" at latitude -13.52 and longitude -72.12
    And the edge gateway holds a geofence alert "exit" citing the reading "outside" at 12.5 meters from the boundary of the geofence "North boundary"
    When the edge gateway synchronizes its pending records
    Then the entry "exit" is "Accepted"
    When the ranch manager lists the alerts of the ranch
    Then the alert "exit" has:
      | field                    | value              |
      | type                     | Geofence           |
      | status                   | Pending            |
      | cattleId                 | <monitored cattle> |
      | geofenceId               | <North boundary>   |
      | latitude                 | -13.52             |
      | longitude                | -72.12             |
      | distanceToBoundaryMeters | 12.5               |
      | raisedAt                 | <time of outside>  |

  Scenario: The return to the geofence closes the alert and keeps the total time outside
    Given the edge gateway holds a reading "outside" from the collar "monitored collar" at latitude -13.52 and longitude -72.12
    And the edge gateway holds a geofence alert "exit" citing the reading "outside" at 12.5 meters from the boundary of the geofence "North boundary"
    When the edge gateway synchronizes its pending records
    And the edge gateway records the return to the geofence of the alert "exit" as "return"
    And the edge gateway synchronizes its pending records
    Then the entry "return" is "Accepted"
    When the ranch manager lists the alerts of the ranch
    Then the alert "exit" has:
      | field     | value            |
      | clearedAt | <time of return> |
    And the exit duration of the alert "exit" equals the time from its raising to its return
