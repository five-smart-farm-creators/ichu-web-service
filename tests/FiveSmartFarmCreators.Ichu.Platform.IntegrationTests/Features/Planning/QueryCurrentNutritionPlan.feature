@US-46
Feature: Query the current nutrition plan of a lot
  As a veterinarian, I want to read the nutrition plan in force for a lot on a given date,
  so that I can compare its ration with the evolution of the lot.

  Background:
    Given a ranch manager with monitored cattle
    And a lot "North pasture" exists in the ranch
    And a veterinarian with a valid authorization over the ranch

  Scenario: A ration change keeps the earlier plan in force until the day before the change
    When the ranch manager registers the nutrition plan "Dry season ration" for the lot "North pasture" valid from "2026-11-01" with the feed "Alfalfa hay" of kind "Forage" at 6 kilograms per head per day
    And the ranch manager registers the nutrition plan "Rainy season ration" for the lot "North pasture" valid from "2026-11-15" with the feed "Maize silage" of kind "Forage" at 8 kilograms per head per day
    And the veterinarian requests the nutrition plan of the lot "North pasture" in force on "2026-11-14"
    Then the request returns status 200
    And the response has:
      | field   | value             |
      | name    | Dry season ration |
      | validTo | 2026-11-14        |
    When the veterinarian requests the nutrition plan of the lot "North pasture" in force on "2026-11-15"
    Then the response has:
      | field   | value               |
      | name    | Rainy season ration |
      | validTo | null                |
