@US-15
Feature: Define the permitted grazing zones
  As a livestock administrator, I want to delimit the permitted geofences of my farm,
  so that the system can determine when cattle are outside them.

  Background:
    Given a ranch manager with monitored cattle
    And a lot "North pasture" exists in the ranch

  Scenario: A closed area of at least three vertices is saved for a lot and reaches the edge gateway
    When the ranch manager defines the geofence "North boundary" for the lot "North pasture" with the boundary:
      | latitude | longitude |
      | -13.50   | -72.10    |
      | -13.50   | -72.09    |
      | -13.49   | -72.09    |
      | -13.49   | -72.10    |
    Then the request returns status 201
    And the response has:
      | field | value           |
      | name  | North boundary  |
      | lotId | <North pasture> |
    When the edge gateway reads its monitoring rules
    Then the monitoring rules include the geofence "North boundary"

  Scenario: A boundary whose sides cross is refused and nothing is stored
    When the ranch manager defines the geofence "Crossing boundary" for the lot "North pasture" with the boundary:
      | latitude | longitude |
      | -13.50   | -72.10    |
      | -13.46   | -72.06    |
      | -13.50   | -72.06    |
      | -13.48   | -72.09    |
    Then the request is refused with status 400 and code "OperationsMonitoring.GeofenceBoundaryCrossing"
    When the ranch manager lists the geofences of the ranch
    Then the response lists 0 items

  Scenario: A boundary with fewer than three vertices is refused
    When the ranch manager defines the geofence "Segment" for the lot "North pasture" with the boundary:
      | latitude | longitude |
      | -13.50   | -72.10    |
      | -13.49   | -72.09    |
    Then the request is refused with status 400 and code "OperationsMonitoring.GeofenceVertexCount"
