@US-45
Feature: Query the reproductive history of a female
  As a veterinarian, I want to review the breeding services, pregnancies and calvings of a female,
  so that my reproductive decisions rest on its recorded history.

  Background:
    Given a ranch manager with monitored cattle
    And a veterinarian with a valid authorization over the ranch

  Scenario: A veterinarian reads a confirmed pregnancy after a service and a calving
    When the ranch manager records the breeding service "first service" for the cattle "monitored cattle" 200 days ago with sire "Bull 07"
    And the ranch manager records the pregnancy diagnosis "pregnancy check" for the cattle "monitored cattle" 180 days ago with result "Pregnant"
    And the ranch manager records the calving "calving" for the cattle "monitored cattle" 10 days ago
    And the veterinarian requests the reproductive history of the cattle "monitored cattle"
    Then the request returns status 200
    And the response has:
      | field                | value      |
      | sex                  | Female     |
      | lifeStage            | MilkingCow |
      | hasHistory           | true       |
      | pregnanciesConfirmed | 1          |
      | servicesPerPregnancy | 1          |
