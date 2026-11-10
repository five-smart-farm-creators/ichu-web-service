@US-34
Feature: Start the subscription of a plan
  As a visitor from the livestock segment, I want to start the subscription of the plan I chose,
  so that I register my production unit and move toward an ICHU subscription.

  Background:
    Given a verified ranch manager with a ranch

  Scenario: A verified manager's request is recorded as pending without activating the coverage
    When the ranch manager requests a subscription to the "Standard" plan for the head count "10" with terms "2026-11" and an expected total of 3000
    Then the request returns status 201
    And the response has:
      | field          | value      |
      | status         | Pending    |
      | planId         | <Standard> |
      | total.amount   | 3000       |
      | total.currency | PEN        |
      | requestedAt    | not null   |
      | activatedAt    | null       |
      | validUntil     | null       |
    When the ranch manager reads the subscription of the ranch
    Then the response has:
      | field  | value   |
      | status | Pending |

  Scenario: A request with outdated terms is refused and nothing is recorded
    When the ranch manager requests a subscription to the "Standard" plan for the head count "10" with terms "2020-01" and an expected total of 3000
    Then the request is refused with status 409 and code "SubscriptionPlans.TermsVersionOutdated"
    When the ranch manager reads the subscription of the ranch
    Then the request is refused with status 404 and code "SubscriptionPlans.SubscriptionNotFound"

  Scenario: A request with a stale estimate is refused and nothing is recorded
    When the ranch manager requests a subscription to the "Standard" plan for the head count "10" with terms "2026-11" and an expected total of 2999
    Then the request is refused with status 409 and code "SubscriptionPlans.QuoteChanged"
    When the ranch manager reads the subscription of the ranch
    Then the request is refused with status 404 and code "SubscriptionPlans.SubscriptionNotFound"

  Scenario Outline: A request with a head count the plan does not admit is refused and nothing is recorded
    When the ranch manager requests a subscription to the "<plan>" plan for the head count "<headCount>" with terms "2026-11" and an expected total of 0
    Then the request is refused with status 400 and code "<code>"
    When the ranch manager reads the subscription of the ranch
    Then the request is refused with status 404 and code "SubscriptionPlans.SubscriptionNotFound"

    Examples:
      | plan     | headCount | code                                   |
      | Standard | 0         | SubscriptionPlans.InvalidHeadCount     |
      | Basic    | 51        | SubscriptionPlans.HeadCountExceedsPlan |
