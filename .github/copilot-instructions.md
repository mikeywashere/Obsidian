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
