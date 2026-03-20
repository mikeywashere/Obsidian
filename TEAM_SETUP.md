# Team Setup

## Team Size and Roles (5)

- **Lead Engineer**
  - Owns architecture and cross-project integration.
  - Reviews architecture-impacting pull requests.
  - Maintains engineering standards and technical risk tracking.

- **Backend Engineer**
  - Owns core server/runtime behavior in `source/Obsidian`.
  - Implements networking, configuration, and reliability/performance fixes.
  - Coordinates data contracts with DataAccess and Web.

- **Web Engineer**
  - Owns UI behavior and auth UX in `source/Obsidian.Web`.
  - Implements pages, service wiring, and user-facing state/error handling.
  - Ensures authorization policies are correctly enforced in UI flows.

- **QA Engineer**
  - Owns test strategy and regression safety nets.
  - Maintains xUnit test coverage in `source/Obsidian.UnitTests`.
  - Defines pre-merge verification checks and release readiness testing.

- **Product + Ops/Security (combined role)**
  - Owns backlog priority, acceptance criteria, and sprint outcomes.
  - Coordinates release notes, runbooks, and incident readiness.
  - Runs a lightweight auth/security checklist each sprint.

## Ownership Map

- **`source/Obsidian`**
  - Primary: Backend Engineer
  - Secondary: Lead Engineer

- **`source/Obsidian.Web`**
  - Primary: Web Engineer
  - Secondary: Lead Engineer

- **`source/Obsidian.DataAccess`**
  - Primary: Backend Engineer
  - Secondary: QA Engineer (testability and validation)

- **`source/Obsidian.UnitTests`**
  - Primary: QA Engineer
  - Secondary: All engineers (tests required with feature work)

## Team Cadence

- **Daily standup (15 min)**
  - Yesterday, today, blockers, and one risk callout.

- **Twice-weekly backlog + risk review (30 min)**
  - Re-prioritize, verify dependencies, and surface delivery risks.

- **Weekly demo + retro (45 min)**
  - Demo completed work, capture improvements, and lock next commitments.

- **Async collaboration rules**
  - First PR review response within 1 business day.
  - Blocker pings answered within 2 hours during core working hours.

## First 2 Weeks (Kickoff Plan)

### Week 1 outcomes

- Confirm architecture boundaries across core/web/data/test projects.
- Slice backlog into 6–10 vertical tasks with clear acceptance criteria.
- Ensure local + CI builds are green for all projects.
- Validate auth and core server happy-path flow end to end.

### Week 2 outcomes

- Ship one thin vertical slice across core server, data access, and web UI.
- Add/adjust tests for all changed behavior.
- Fix and stabilize top 3 defects discovered during integration.
- Publish release checklist and runbook v1.

## Definition of Done (DoD)

A work item is done when:

- Acceptance criteria are fully met.
- Tests are added/updated and passing.
- No critical auth/security regressions are introduced.
- Documentation is updated when behavior/config changed.
- Build and run steps remain functional.

## PR Checklist

Before merge, verify:

- PR scope is focused and linked to a tracked task.
- Behavioral changes include test coverage.
- CI is green.
- Risk/rollback notes are included for non-trivial changes.
- At least one non-author approval is present.
- No unresolved high-severity review comments remain.
