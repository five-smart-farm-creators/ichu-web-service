@US-35
Feature: Compare the plans and estimate the cost
  As a livestock administrator, I want to compare the plans according to my herd size,
  so that I know the coverage and the estimated cost before subscribing.

  Scenario Outline: Each plan shows its device limit, features, currency, recurring fee and device price separately
    When a visitor lists the subscription plans
    Then the plan "<plan>" has:
      | field                  | value         |
      | deviceLimit            | <deviceLimit> |
      | features               | <features>    |
      | annualFee.amount       | <annualFee>   |
      | annualFee.currency     | PEN           |
      | deviceUnitPrice.amount | 120           |

    Examples:
      | plan     | deviceLimit | annualFee | features                                              |
      | Basic    | 50          | 600       | Monitoring, Alerts                                    |
      | Standard | 200         | 1800      | Monitoring, Alerts, Planning, Analytics               |
      | Pro      | 500         | 3900      | Monitoring, Alerts, Planning, Analytics, ReportExport |

  Scenario: The estimate for a valid head count breaks the total into the annual fee and the devices cost
    When a visitor requests a quote for the "Standard" plan with the head count "10"
    Then the request returns status 200
    And the response has:
      | field                  | value      |
      | planId                 | <Standard> |
      | headCount              | 10         |
      | annualFee.amount       | 1800       |
      | devicesCost.amount     | 1200       |
      | deviceUnitPrice.amount | 120        |
      | total.amount           | 3000       |
      | total.currency         | PEN        |

  Scenario Outline: An estimate for a head count the plan does not admit yields no quote
    When a visitor requests a quote for the "<plan>" plan with the head count "<headCount>"
    Then the request is refused with status 400 and code "<code>"

    Examples:
      | plan     | headCount | code                                   |
      | Standard | 0         | SubscriptionPlans.InvalidHeadCount     |
      | Standard | -3        | SubscriptionPlans.InvalidHeadCount     |
      | Basic    | 51        | SubscriptionPlans.HeadCountExceedsPlan |

  Scenario: A head count that is not a whole number yields no quote
    When a visitor requests a quote for the "Standard" plan with the head count "2.5"
    Then the request is refused with status 400
