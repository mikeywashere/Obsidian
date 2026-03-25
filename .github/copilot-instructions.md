## Squad Team

This project uses **Squad** — an AI team framework. See `.squad/` for full configuration.

### Team Roster

| Name | Role | Charter |
|------|------|---------|
| Morpheus | Tech Lead | `.squad/agents/morpheus/charter.md` |
| Neo | Backend Dev | `.squad/agents/neo/charter.md` |
| Trinity | Frontend Dev | `.squad/agents/trinity/charter.md` |
| Tank | Data Engineer | `.squad/agents/tank/charter.md` |
| Oracle | Tester | `.squad/agents/oracle/charter.md` |
| Keaton | DevOps Engineer | `.squad/agents/keaton/charter.md` |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` |

### Routing Summary

| Work Type | Agent |
|-----------|-------|
| Architecture & system design | Morpheus |
| Backend C# / API / server | Neo |
| Frontend Blazor / UI | Trinity |
| Data access / auth | Tank |
| CI/CD / infrastructure | Keaton |
| Code review | Morpheus |
| Testing | Oracle |
| Session logging | Scribe (silent) |

See `.squad/routing.md` for full routing rules and issue label conventions.

---

## Playwright test requirements

When writing Playwright tests, please follow these guidelines to ensure consistency and maintainability:

- **Use stable locators** - Prefer `getByRole()`, `getByText()`, and `getByTestId()` over CSS selectors or XPath
- **Write isolated tests** - Each test should be independent and not rely on other tests' state
- **Follow naming conventions** - Use descriptive test names and `*.spec.ts` file naming
- **Implement proper assertions** - Use Playwright's `expect()` with specific matchers like `toHaveText()`, `toBeVisible()`
- **Leverage auto-wait** - Avoid manual `setTimeout()` and rely on Playwright's built-in waiting mechanisms
- **Configure cross-browser testing** - Test across Chromium, Firefox, and WebKit browsers
- **Use Page Object Model** - Organize selectors and actions into reusable page classes for maintainability
- **Handle dynamic content** - Properly wait for elements to load and handle loading states
- **Set up proper test data** - Use beforeEach/afterEach hooks for test setup and cleanup
- **Configure CI/CD integration** - Set up headless mode, screenshots on failure, and parallel execution

## C# coding requirements

- always use xunit for unit tests.
- prefer nsubstitute for mocks.
