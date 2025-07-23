# Session 1: Backend API Enhancement & Dashboard Endpoints

## Mission
Complete and enhance the LibraryApp backend microservices to support the dashboard UI requirements. Work on branch `feature/dashboard-backend-apis`.

## Context
You are working on a .NET 8 microservices library management system. The CLAUDE.md file contains full project context. A React/TypeScript frontend is being built simultaneously that requires specific dashboard and statistics endpoints.

## Dashboard Requirements Analysis
From the UI mockup, the backend needs to provide:

### Dashboard Statistics APIs
```
GET /api/dashboard/stats - Overall statistics
{
  "totalBooks": 5,
  "availableBooks": 2, 
  "booksBorrowed": 2,
  "totalMembers": 4,
  "activeMembers": 3,
  "overdueBooks": 1
}

GET /api/dashboard/recent-transactions?limit=10 - Recent activity
GET /api/dashboard/book-categories - Category breakdown with counts
GET /api/dashboard/top-members?limit=5 - Most active members
GET /api/dashboard/library-statistics - Monthly/weekly stats
GET /api/dashboard/alerts - System alerts and notifications
```

## Phase 1: Branch Setup & Assessment
1. Create feature branch: `git checkout -b feature/dashboard-backend-apis`
2. Analyze existing APIs in all services (Auth, Book, Member)
3. Test current functionality with `.\scripts\start-dev.ps1 -Detached`
4. Document any issues found

## Phase 2: Dashboard Statistics Service
1. **Decide architecture approach:**
   - Option A: Add dashboard endpoints to API Gateway with aggregation
   - Option B: Create dedicated Dashboard Service
   - Option C: Add endpoints to existing services

2. **Implement core statistics endpoints:**
   - Book statistics (total, available, borrowed, overdue)
   - Member statistics (total, active, inactive)
   - Transaction statistics
   - Category breakdown

3. **Add caching layer using Redis for performance**

## Phase 3: Enhanced API Endpoints
1. **Books Service enhancements:**
   ```
   GET /api/books/categories - Category list with counts
   GET /api/books/statistics - Book-related stats
   GET /api/books/overdue - Overdue books list
   ```

2. **Members Service enhancements:**
   ```
   GET /api/members/statistics - Member stats
   GET /api/members/active - Active members list
   GET /api/members/top-borrowers - Most active borrowers
   ```

3. **Transactions/History APIs:**
   ```
   GET /api/transactions/recent - Recent transactions
   GET /api/transactions/statistics - Transaction stats
   POST /api/transactions/borrow - Enhanced borrowing
   POST /api/transactions/return - Enhanced returns
   ```

## Phase 4: Authentication & Authorization
1. **Enhance Auth Service for frontend:**
   ```
   POST /api/auth/login - Return user profile with role
   GET /api/auth/profile - Get current user profile
   POST /api/auth/refresh - Token refresh
   ```

2. **Add role-based permissions:**
   - Librarian: Full dashboard access
   - Member: Limited access to personal data

## Phase 5: CORS & API Gateway Configuration
1. **Update Ocelot configuration** for frontend integration
2. **Configure CORS** for React development server
3. **Add rate limiting** appropriate for dashboard usage
4. **Update health checks** for new endpoints

## Phase 6: Data Seeding & Sample Data
1. **Enhance database seeding** with realistic library data:
   - 20+ books across different categories
   - 10+ members with borrowing history
   - Transaction history spanning last 30 days
   - Overdue books scenarios

2. **Create seed data scripts** for consistent development

## Phase 7: API Documentation
1. **Update Swagger documentation** for all new endpoints
2. **Create API collection** for Postman/testing
3. **Document response schemas** for frontend team

## Phase 8: Performance & Monitoring
1. **Implement caching strategies** for dashboard data
2. **Add performance logging** for slow queries
3. **Optimize database queries** with proper indexing
4. **Add correlation IDs** for request tracing

## Technical Requirements
- **Response Times:** Dashboard APIs should respond < 500ms
- **Caching:** Cache dashboard stats for 1-5 minutes
- **Error Handling:** Graceful degradation when services are unavailable
- **Data Validation:** Validate all inputs with appropriate error responses
- **Security:** Ensure all endpoints are properly authenticated/authorized

## Testing Requirements
1. **Unit tests** for new services and endpoints
2. **Integration tests** for dashboard data aggregation
3. **Performance tests** for statistics endpoints under load
4. **API contract tests** to ensure frontend compatibility

## Acceptance Criteria
- [ ] All dashboard statistics endpoints are implemented and working
- [ ] Enhanced book/member/transaction APIs are complete
- [ ] Authentication works with frontend (CORS configured)
- [ ] Database has sufficient seed data for realistic dashboard
- [ ] All new endpoints are documented in Swagger
- [ ] Performance targets are met (< 500ms response times)
- [ ] Unit and integration tests pass
- [ ] Ready for frontend integration

## Handoff to Frontend Team
When complete, provide:
1. **API documentation** with all endpoint details
2. **Sample API responses** for each endpoint
3. **Authentication flow** documentation
4. **Error response** schemas
5. **Environment setup** instructions for frontend integration

## Iterative Development Approach
Work in 2-3 hour iterations:
1. **Iteration 1:** Branch setup, assessment, basic stats endpoints
2. **Iteration 2:** Enhanced APIs, authentication improvements
3. **Iteration 3:** CORS, seeding, performance optimization
4. **Iteration 4:** Testing, documentation, handoff preparation

## Commands to Get Started
```powershell
# Create branch and start development
git checkout -b feature/dashboard-backend-apis
.\scripts\start-dev.ps1 -Detached

# Test current APIs
curl http://localhost:5000/health/services

# View logs while developing
.\scripts\logs.ps1
```

Remember: The frontend team is building simultaneously, so prioritize API contracts and documentation early in development.