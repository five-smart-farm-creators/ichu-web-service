# Subscription Plans

This module owns the plan catalog, cost estimates, subscription requests, payments, validity and the entitlements other modules depend on. It implements the backend side of US-34, US-35 and TS-10 from the Report and the Subscription Entitlement relationship of the context map.

A subscription request is not a payment and does not enable a plan. A request is Pending until a payment result verified by the provider adapter is accepted. No payment is simulated: without a provider adapter, subscriptions stay Pending.

## Conventions

- JSON uses camelCase; enumerations are PascalCase strings. Identifiers are Guid UUIDs and timestamps ISO 8601 `DateTimeOffset` values in UTC.
- Amounts are `{ amount, currency }` with two decimals and an ISO 4217 currency code.
- Errors are ProblemDetails: 400 invalid input, 401 missing identity or unverified provider notification, 403 denied access, 404 unknown plan, subscription or payment, 409 stale or conflicting data, 503 configuration not available.

## Public API

| Method | Route | Caller and permission | Behavior |
| --- | --- | --- | --- |
| GET | `/api/v1/plans` | Anonymous | Plans published with a valid price: code, name, billing period, price per head, hardware unit price, head-count coverage, device limit, features and price validity. |
| GET | `/api/v1/plans/{planId}` | Anonymous | One published plan. |
| GET | `/api/v1/plans/{planId}/quote?headCount` | Anonymous | US-35. `quoteId`, recurring amount, separate hardware amount, billing period, `validUntil` and `isEstimate=true`. A count outside the coverage returns 400 with the restriction and `suggestedPlanId` only when another offered plan covers it. |
| POST | `/api/v1/subscriptions` | ManageSubscription | US-34/TS-10. `ranchId`, `planId`, `headCount`, `quoteId`, `termsVersion`, `operationId`, optional `acquisitionSource`. 201 with a Pending subscription and its pending activation payment; 200 when the same operation is repeated. |
| GET | `/api/v1/subscriptions?ranchId` | ViewSubscription | The pending or active subscription, or the latest one: status, `isEntitled`, paid period, device limit and features. |
| GET | `/api/v1/subscriptions/{subscriptionId}` | ViewSubscription | One subscription. |
| POST | `/api/v1/subscriptions/{subscriptionId}/cancellations` | ManageSubscription | `reason`. Cancels immediately and keeps the payment history. |
| GET | `/api/v1/subscriptions/{subscriptionId}/payments` | ViewSubscription | Payment history. |
| POST | `/api/v1/subscriptions/{subscriptionId}/payments` | ManageSubscription | Requests the next activation or renewal payment; 409 while another payment is pending. |
| POST | `/api/v1/payment-confirmations` | Payment provider | Raw notification up to 64 KB, verified by `IPaymentNotificationVerifier`. 200 with `Applied` or `AlreadyApplied`; 401 when unverified; 409 for a different result or amount. |

## Domain rules

- A plan covers a head count from its minimum to its maximum; a null maximum has no upper limit. It is offered only while published and before `pricesValidUntil`.
- The recurring amount is the price per head multiplied by the head count for each billing period (Monthly or Annual). The hardware estimate is reported separately and is not charged by this module. It assumes one collar per head; that assumption is pending commercial confirmation.
- The quote identifier is derived from the plan, its pricing version and the head count, so anonymous quotes are not stored. Catalog maintainers must increase `PricingVersion` whenever prices change; earlier quotes then no longer match and requests return 409.
- A request requires a verified account that can manage the ranch, the configured current terms version (`SubscriptionPlans:CurrentTermsVersion`), an offered plan, a covered head count and the current quote. A ranch has at most one Pending or Active subscription, enforced by a filtered unique index.
- The same `operationId` with the same request returns the existing subscription; with different content it is a conflict.
- An accepted payment must match the requested amount and currency. It activates a Pending subscription, or extends an Active one from the later of its paid-until date and the processing time. A rejected payment leaves the subscription unchanged, and a new payment can be requested. Repeated provider results are idempotent.
- Expired is derived when an Active subscription reaches its paid-until date. Cancellation is immediate, ends entitlements and preserves payments; a payment accepted after cancellation is recorded without reactivation. Refunds are outside this module.

## Cross-context contract

`ISubscriptionPlansContextFacade` (Interfaces/Acl):

| Method | Consumer | Result |
| --- | --- | --- |
| `GetEntitlementAsync(ranchId, ct)` | IoTAssets (US-10 device limit) and DashboardAnalytics | `RanchEntitlement`: `IsActive`, subscription and plan identifiers, plan code, `MaximumDevices`, `Features` and `PaidUntil`. Without an active, paid subscription it is inactive with 0 devices and no features. |
| `HasFeatureAsync(ranchId, feature, ct)` | DashboardAnalytics | Whether the current entitlement includes a feature: AdvancedReports, SmsAlerts, HerdHealthAnalytics or MultiRanchPortfolio. |

The facade performs no user authorization and never accepts a plan name from a client. Limits and features come from the current plan definition. The proposed mapping for the unmerged DashboardAnalytics port is `IAnalyticsEntitlements.CanExportAsync` to `HasFeatureAsync(ranchId, FeatureCode.AdvancedReports)`; it requires review with Diego. IoTAssets compares its own count of assigned devices with `MaximumDevices`.

## Ports required from the host

`AddSubscriptionPlans` registers no simulated implementations.

| Port | Owner | Required behavior |
| --- | --- | --- |
| `ISubscriptionAccessControl` | Iam (Diego) | Verified current account; ViewSubscription for ranch members and ManageSubscription only for the account that controls the ranch. |
| `IPaymentNotificationVerifier` | Payment provider adapter (not selected) | Verifies the provider signature and returns the payment identifier, result, external reference, amount, currency and processing time, or null. |

Initiating a charge with a provider is not implemented, because no provider has been selected; the module records the pending payment that a provider integration must charge.

## Host integration

This branch does not change `Program.cs`, `Shared`, project files, global configuration, the shared DbContext or migrations.

1. Register the shared PostgreSQL DbContext and expose the same scoped instance as `DbContext`.
2. Apply `SubscriptionPlansEntityConfigurations` through `ApplyConfigurationsFromAssembly`. Tables use the `subscription` schema: Plans, PlanFeatures, Subscriptions and Payments.
3. Load the catalog through a reviewed data migration. The Report does not fix prices and the frontend values are provisional, so no plan data is included. There is no catalog-administration endpoint because no platform-administrator role exists yet.
4. Configure `SubscriptionPlans:CurrentTermsVersion` with the published terms version; requests return 503 until it is set.
5. Register real implementations of both ports, then call `builder.Services.AddSubscriptionPlans(builder.Configuration)`.

## Pending scope

- Payment-provider selection, charge initiation, refunds and provider webhooks.
- Catalog administration, plan changes for an existing subscription and expiry notifications.
- Confirmation of the hardware quantity rule and of the feature required by each consumer.

## Verification

The temporary xUnit project described in the OperationsMonitoring README also covered:

- Money and quote rules, coverage limits, plan suggestions and price expiry.
- Pending requests, replays, stale quotes and terms, and the single-open-subscription rule.
- Unverified, mismatched, accepted and repeated payment results, renewal, expiry and cancellation.
- Entitlements through EF Core on in-memory SQLite, the PostgreSQL DDL, and the public HTTP routes.

Real PostgreSQL behavior and provider integration remain integration checks.
