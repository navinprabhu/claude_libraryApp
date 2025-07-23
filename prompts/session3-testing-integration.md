# Session 3: Testing & Integration Validation

## Mission
Comprehensive testing of the LibraryApp system including backend APIs, frontend components, and end-to-end integration. Work on branch `feature/comprehensive-testing`.

## Context
You are testing a complete library management system with .NET 8 microservices backend and React/TypeScript frontend. Two other sessions are simultaneously developing backend APIs and frontend UI. The CLAUDE.md file contains full project context.

## Testing Architecture Overview
Your role is to ensure:
1. **Backend APIs** work correctly and meet performance requirements
2. **Frontend components** render properly and handle all states
3. **End-to-end integration** between frontend and backend works seamlessly
4. **User experience** matches requirements and is bug-free
5. **Performance benchmarks** are met across the system

## Phase 1: Environment Setup & Test Planning
1. **Create testing branch:**
   ```bash
   git checkout -b feature/comprehensive-testing
   ```

2. **Setup testing tools:**
   ```json
   {
     "devDependencies": {
       "cypress": "^13.6.0",
       "playwright": "^1.40.0",
       "@testing-library/react": "^13.4.0",
       "@testing-library/jest-dom": "^6.1.0",
       "@testing-library/user-event": "^14.5.0",
       "jest": "^29.7.0",
       "supertest": "^6.3.0",
       "k6": "latest",
       "newman": "latest"
     }
   }
   ```

3. **Test environment configuration:**
   ```bash
   # Ensure all services are running
   .\scripts\start-dev.ps1 -Detached
   
   # Verify health checks
   curl http://localhost:5000/health/services
   ```

## Phase 2: Backend API Testing
1. **Unit Test Validation:**
   ```bash
   # Run existing backend unit tests
   dotnet test
   
   # Generate coverage report
   dotnet test --collect:"XPlat Code Coverage"
   ```

2. **API Contract Testing:**
   ```javascript
   // tests/api/dashboard.spec.js
   describe('Dashboard API', () => {
     test('GET /api/dashboard/stats returns correct schema', async () => {
       const response = await request(app).get('/api/dashboard/stats');
       expect(response.status).toBe(200);
       expect(response.body).toHaveProperty('totalBooks');
       expect(response.body).toHaveProperty('availableBooks');
       expect(typeof response.body.totalBooks).toBe('number');
     });
   });
   ```

3. **Performance Testing with k6:**
   ```javascript
   // tests/performance/dashboard-load.js
   import http from 'k6/http';
   import { check } from 'k6';

   export let options = {
     stages: [
       { duration: '2m', target: 100 },
       { duration: '5m', target: 100 },
       { duration: '2m', target: 0 },
     ],
   };

   export default function() {
     let response = http.get('http://localhost:5000/api/dashboard/stats');
     check(response, {
       'status is 200': (r) => r.status === 200,
       'response time < 500ms': (r) => r.timings.duration < 500,
     });
   }
   ```

4. **API Security Testing:**
   ```javascript
   // Verify JWT token validation
   // Test rate limiting
   // Check CORS configuration
   // Validate input sanitization
   ```

## Phase 3: Frontend Component Testing
1. **Unit Tests for React Components:**
   ```typescript
   // tests/components/StatCard.test.tsx
   import { render, screen } from '@testing-library/react';
   import StatCard from '../components/dashboard/StatCard';

   describe('StatCard', () => {
     test('renders stat card with correct data', () => {
       render(
         <StatCard 
           title="Total Books" 
           value={5} 
           color="blue" 
           icon={<BookIcon />} 
         />
       );
       expect(screen.getByText('Total Books')).toBeInTheDocument();
       expect(screen.getByText('5')).toBeInTheDocument();
     });
   });
   ```

2. **Integration Tests for Data Fetching:**
   ```typescript
   // tests/hooks/useDashboardData.test.tsx
   import { renderHook, waitFor } from '@testing-library/react';
   import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
   import { useDashboardStats } from '../hooks/useDashboardData';

   test('useDashboardStats fetches and returns data', async () => {
     const { result } = renderHook(() => useDashboardStats());
     
     await waitFor(() => {
       expect(result.current.isSuccess).toBe(true);
     });

     expect(result.current.data).toHaveProperty('totalBooks');
   });
   ```

3. **Visual Regression Testing:**
   ```javascript
   // tests/visual/dashboard.spec.js
   const { test, expect } = require('@playwright/test');

   test('dashboard visual regression', async ({ page }) => {
     await page.goto('http://localhost:3000/dashboard');
     await page.waitForLoadState('networkidle');
     await expect(page).toHaveScreenshot('dashboard-full.png');
   });
   ```

## Phase 4: End-to-End Integration Testing
1. **User Journey Tests with Cypress:**
   ```javascript
   // cypress/e2e/library-workflow.cy.js
   describe('Library Management Workflow', () => {
     it('should complete full library workflow', () => {
       // Login as librarian
       cy.visit('http://localhost:3000/login');
       cy.get('[data-cy=username]').type('admin');
       cy.get('[data-cy=password]').type('password');
       cy.get('[data-cy=login-button]').click();

       // Verify dashboard loads
       cy.url().should('include', '/dashboard');
       cy.get('[data-cy=total-books]').should('contain', '5');
       cy.get('[data-cy=available-books]').should('contain', '2');

       // Navigate to books section
       cy.get('[data-cy=nav-books]').click();
       cy.url().should('include', '/books');

       // Add a new book
       cy.get('[data-cy=add-book-button]').click();
       cy.get('[data-cy=book-title]').type('Test Book');
       cy.get('[data-cy=book-author]').type('Test Author');
       cy.get('[data-cy=save-book]').click();

       // Verify book appears in list
       cy.get('[data-cy=books-table]').should('contain', 'Test Book');
     });
   });
   ```

2. **Cross-Browser Testing:**
   ```javascript
   // playwright.config.js
   module.exports = {
     projects: [
       { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
       { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
       { name: 'webkit', use: { ...devices['Desktop Safari'] } },
       { name: 'mobile', use: { ...devices['iPhone 13'] } },
     ],
   };
   ```

3. **API Integration Validation:**
   ```javascript
   // Verify frontend-backend communication
   // Test error handling scenarios
   // Validate data flow accuracy
   // Check authentication persistence
   ```

## Phase 5: Performance & Load Testing
1. **Frontend Performance Metrics:**
   ```javascript
   // tests/performance/frontend-metrics.spec.js
   test('dashboard performance metrics', async ({ page }) => {
     await page.goto('http://localhost:3000/dashboard');
     
     const metrics = await page.evaluate(() => {
       const navigation = performance.getEntriesByType('navigation')[0];
       return {
         fcp: performance.getEntriesByName('first-contentful-paint')[0]?.startTime,
         lcp: performance.getEntriesByName('largest-contentful-paint')[0]?.startTime,
         domContentLoaded: navigation.domContentLoadedEventEnd - navigation.navigationStart,
       };
     });

     expect(metrics.fcp).toBeLessThan(2000); // First Contentful Paint < 2s
     expect(metrics.domContentLoaded).toBeLessThan(3000); // DOM ready < 3s
   });
   ```

2. **Backend Load Testing:**
   ```bash
   # Run k6 performance tests
   k6 run tests/performance/dashboard-load.js
   k6 run tests/performance/api-stress-test.js
   ```

3. **Database Performance:**
   ```sql
   -- Test query performance
   EXPLAIN ANALYZE SELECT * FROM books WHERE status = 'available';
   EXPLAIN ANALYZE SELECT COUNT(*) FROM borrowing_records WHERE return_date IS NULL;
   ```

## Phase 6: Security Testing
1. **Authentication & Authorization:**
   ```javascript
   describe('Security Tests', () => {
     test('should reject invalid JWT tokens', async () => {
       const response = await request(app)
         .get('/api/books')
         .set('Authorization', 'Bearer invalid-token');
       expect(response.status).toBe(401);
     });

     test('should enforce role-based access', async () => {
       const memberToken = await getMemberToken();
       const response = await request(app)
         .post('/api/books')
         .set('Authorization', `Bearer ${memberToken}`);
       expect(response.status).toBe(403);
     });
   });
   ```

2. **Input Validation Testing:**
   ```javascript
   // Test SQL injection prevention
   // Test XSS protection
   // Validate CSRF protection
   // Check rate limiting effectiveness
   ```

## Phase 7: Accessibility Testing
1. **Automated Accessibility Testing:**
   ```javascript
   // tests/accessibility/dashboard.spec.js
   const { injectAxe, checkA11y } = require('axe-playwright');

   test('dashboard accessibility', async ({ page }) => {
     await page.goto('http://localhost:3000/dashboard');
     await injectAxe(page);
     await checkA11y(page, null, {
       detailedReport: true,
       detailedReportOptions: { html: true },
     });
   });
   ```

2. **Keyboard Navigation Testing:**
   ```javascript
   test('keyboard navigation works', async ({ page }) => {
     await page.goto('http://localhost:3000/dashboard');
     
     // Test tab navigation
     await page.keyboard.press('Tab');
     await expect(page.locator(':focus')).toHaveAttribute('data-cy', 'nav-dashboard');
     
     // Test Enter key activation
     await page.keyboard.press('Enter');
     await expect(page).toHaveURL(/.*dashboard/);
   });
   ```

## Phase 8: Mobile & Responsive Testing
1. **Mobile Device Testing:**
   ```javascript
   // tests/mobile/responsive.spec.js
   test('mobile dashboard layout', async ({ page }) => {
     await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE
     await page.goto('http://localhost:3000/dashboard');
     
     // Verify sidebar collapses
     await expect(page.locator('[data-cy=sidebar]')).not.toBeVisible();
     
     // Verify stats cards stack vertically
     const statsCards = page.locator('[data-cy=stat-card]');
     const firstCard = statsCards.first();
     const secondCard = statsCards.nth(1);
     
     const firstCardBox = await firstCard.boundingBox();
     const secondCardBox = await secondCard.boundingBox();
     
     expect(secondCardBox.y).toBeGreaterThan(firstCardBox.y + firstCardBox.height);
   });
   ```

## Phase 9: Monitoring & Reporting
1. **Test Result Aggregation:**
   ```javascript
   // Create comprehensive test reports
   // Generate coverage reports
   // Performance benchmark reports
   // Security scan reports
   ```

2. **Continuous Integration Setup:**
   ```yaml
   # .github/workflows/testing.yml
   name: Comprehensive Testing
   on: [push, pull_request]
   jobs:
     test:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - name: Setup Backend
           run: |
             docker-compose up -d
             sleep 30
         - name: Setup Frontend
           run: |
             cd frontend
             npm install
             npm start &
             sleep 10
         - name: Run Tests
           run: |
             npm run test:unit
             npm run test:integration
             npm run test:e2e
   ```

## Phase 10: Bug Reporting & Documentation
1. **Bug Tracking Template:**
   ```markdown
   ## Bug Report
   **Title:** [Brief description]
   **Severity:** Critical/High/Medium/Low
   **Component:** Backend API/Frontend UI/Integration
   **Steps to Reproduce:**
   1. 
   2. 
   3. 
   **Expected Result:**
   **Actual Result:**
   **Screenshots/Videos:**
   **Environment:** Browser/OS/Version
   **Additional Context:**
   ```

2. **Test Documentation:**
   ```markdown
   # Test Coverage Report
   ## Backend API Tests
   - Unit Tests: 95% coverage
   - Integration Tests: 87% coverage
   - Performance Tests: All endpoints < 500ms
   
   ## Frontend Tests
   - Component Tests: 92% coverage
   - E2E Tests: 15 critical user journeys
   - Accessibility: WCAG AA compliance
   
   ## Known Issues
   1. [Issue description with priority]
   2. [Issue description with priority]
   ```

## Success Criteria
- [ ] Backend API test coverage > 90%
- [ ] Frontend component test coverage > 85%
- [ ] All critical user journeys pass E2E tests
- [ ] Performance benchmarks met (< 2s load time, < 500ms API response)
- [ ] Security tests pass (authentication, authorization, input validation)
- [ ] Accessibility score > 90 (WCAG AA compliance)
- [ ] Cross-browser compatibility verified (Chrome, Firefox, Safari)
- [ ] Mobile responsiveness validated on 3 device sizes
- [ ] Zero critical bugs, < 5 high-priority bugs
- [ ] Comprehensive test documentation delivered

## Test Execution Schedule
**Week 1:** Backend API testing, performance benchmarking
**Week 2:** Frontend component testing, visual regression
**Week 3:** End-to-end integration, security testing
**Week 4:** Mobile testing, accessibility, final validation

## Tools & Commands Reference
```bash
# Backend testing
dotnet test --collect:"XPlat Code Coverage"
k6 run tests/performance/load-test.js

# Frontend testing
npm test -- --coverage
npm run test:e2e
npx playwright test

# Integration testing
cypress run
npm run test:integration

# Security testing
npm audit
docker run --rm -v $(pwd):/app securecodewarrior/docker-bench-security

# Performance monitoring
npm run lighthouse
npm run bundle-analyzer
```

## Deliverables
1. **Test Suite:** Complete automated test coverage
2. **Performance Report:** Benchmarks and optimization recommendations
3. **Security Audit:** Vulnerability assessment and fixes
4. **Bug Report:** Prioritized list of issues found
5. **Test Documentation:** Comprehensive testing guide
6. **CI/CD Integration:** Automated testing pipeline
7. **User Acceptance Criteria:** Validation checklist