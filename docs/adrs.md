# Architecture Decision Records

Architecture Decision Records (ADRs) capture significant architectural decisions, the context that led to them, and their consequences. They provide a concise, durable record for current and future contributors; they complement the code and other documentation rather than replacing them.

## When to write an ADR

Write an ADR when a decision has a meaningful, lasting effect on the architecture or is difficult to reverse. Examples include choosing a major technology, defining a domain or bounded-context boundary, setting integration or persistence patterns, or changing a system-wide quality attribute.

Do not use ADRs for routine implementation details, temporary tasks, or decisions that are easy to reverse and have little impact. Keep each ADR focused on one decision. If a decision affects domain language or rules, align it with the relevant domain documentation.

## Process

1. Discuss the problem and relevant constraints with the people affected. Record the decision drivers and reasonable alternatives, including the status quo where appropriate.
2. Create the next sequential ADR using the template below. Use a zero-padded number and a short kebab-case title, for example `0001-use-postgresql.md`. Keep the number stable; do not reuse numbers.
3. Open the ADR as **Proposed** with the change that introduces or depends on the decision. Review it with the related code and documentation.
4. Once the decision is made, set its status to **Accepted** or **Rejected** and record the date. If it is not yet decided, leave it **Proposed**. Do not present a proposal as an established system fact.
5. Keep accepted decisions consistent with the implementation. If a later decision replaces one, create a new ADR and mark the earlier record **Superseded by ADR-NNNN**. Do not silently rewrite the history of an accepted decision.

An ADR can also be marked **Deprecated** when it no longer applies and has not been replaced by a single new decision. Explain the reason and any migration or follow-up needed.

## Finding ADRs

Store individual records in `docs/adr/` and name them `<number>-<short-title>.md`. List them here in numerical order, with a brief description and current status:

| ADR | Title | Status |
| --- | --- | --- |
| _No ADRs recorded yet._ |  |  |

When adding the first ADR, replace the placeholder row. Keep this index in sync when adding, rejecting, deprecating, or superseding records.

## ADR template

Copy this template into a new numbered file under `docs/adr/`. Remove instructional prompts when filling it in; mark sections not applicable where useful.

```markdown
# ADR-NNNN: <Short decision title>

- Status: Proposed
- Date: YYYY-MM-DD
- Decision makers: <People or roles involved>
- Related: <Links to issues, pull requests, or other ADRs, if applicable>

## Context

Describe the problem, background, constraints, and forces that make a decision necessary. Include relevant domain terminology and quality attributes.

## Decision drivers

- <The requirements or constraints used to evaluate options>

## Considered options

### <Option 1>

- Benefits: <Relevant advantages>
- Drawbacks: <Relevant costs or risks>

### <Option 2>

- Benefits: <Relevant advantages>
- Drawbacks: <Relevant costs or risks>

## Decision

State the chosen option clearly and explain why it best addresses the decision drivers.

## Consequences

Record the significant positive, negative, and neutral consequences, including trade-offs, risks, and follow-up work. Do not omit costs introduced by the decision.
```

## Writing guidance

- Keep the record concise, factual, and understandable without requiring the reader to know the discussion that produced it.
- Describe the problem before advocating a solution. Compare viable alternatives fairly and explain why the chosen option fits the constraints.
- Distinguish facts, assumptions, and unresolved questions. Avoid vague statements such as “best,” “standard,” or “future-proof” without evidence.
- Record consequences and operational or migration responsibilities, not just benefits.
- Use stable links where possible. Update implementation and user-facing documentation separately when a decision changes them.
