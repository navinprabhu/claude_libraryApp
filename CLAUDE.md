# LibraryApp - AI Agent Context Manager

⚠️ **OPTIMIZE TOKENS**: Use `CONTEXT_MINIMAL.md` for session start (300 tokens vs 2500+)

This file provides **context-aware documentation loading** for AI agents working with the LibraryApp microservices project.

## Context Loading Strategy

### Task Detection and Context Composition:

**For CI/CD Issues:**
```
Load: docs/context/cicd-context.md
```
- Pipeline failures, health checks, Docker issues
- Integration test problems, GitHub Actions debugging

**For Feature Development:**
```  
Load: docs/context/development-context.md
```
- Adding endpoints, modifying services, business logic
- Authentication, database operations, API design

**For System Architecture Questions:**
```
Load: docs/context/architecture-context.md  
```
- Service communication, system design decisions
- Understanding data flow and dependencies

**For Troubleshooting:**
```
Load: docs/context/troubleshooting-context.md
```
- Service communication failures, debugging
- Performance issues, configuration problems

## Quick Task-Specific Access:

### Recipes (Step-by-Step Guides):
- **Fix CI pipeline**: `docs/recipes/fix-ci-pipeline.md`
- **Add API endpoint**: `docs/recipes/add-api-endpoint.md`
- **Add new service**: `docs/recipes/add-new-service.md`
- **Debug service communication**: `docs/recipes/debug-service-comm.md`
- **Deploy changes**: `docs/recipes/deploy-changes.md`

### Reference (Quick Lookup):
- **Commands**: `docs/reference/commands.md`
- **Service topology**: `docs/reference/ports-and-services.md`  
- **File locations**: `docs/reference/file-locations.md`
- **API endpoints**: `docs/reference/endpoints.md`

### Atomic Knowledge (Building Blocks):
- **Service patterns**: `docs/atoms/service-patterns.md`
- **API conventions**: `docs/atoms/api-conventions.md`
- **CI pipeline**: `docs/atoms/ci-pipeline.md`
- **Health checks**: `docs/atoms/health-checks.md`
- **Authentication**: `docs/atoms/authentication-flow.md`
- **Testing patterns**: `docs/atoms/testing-patterns.md`
- **Docker patterns**: `docs/atoms/docker-patterns.md`
- **Database patterns**: `docs/atoms/database-patterns.md`

## Project Overview Summary

**LibraryApp** - .NET 8 microservices library management system:
- **4 Core Services**: Auth (5001), Books (5002), Members (5003), Gateway (5000)
- **Modern React Frontend**: React 19.1.0 with Material-UI v7 and dark/light mode toggle
- **Docker-first**: PostgreSQL databases + Redis cache
- **JWT authentication** with role-based access (`Admin`, `Member`)
- **Ocelot API Gateway** for routing and rate limiting
- **Comprehensive CI/CD** with GitHub Actions

## AI Agent Usage Instructions

### How to Use This Context System:

1. **Identify the task type** from user request
2. **Load the appropriate context** using the mappings above  
3. **Reference specific recipes** for step-by-step guidance
4. **Use reference docs** for quick lookups during implementation

### Benefits for AI Agents:
- ✅ **Surgical Context**: Load only what's needed for the specific task
- ✅ **Faster Processing**: Smaller, focused documentation chunks  
- ✅ **Better Accuracy**: Task-specific patterns and examples
- ✅ **Consistent Patterns**: Standardized approaches across the system
- ✅ **Quick Reference**: Instant access to commands and configurations

### Example Usage:
```
User: "The CI pipeline is failing with container health check issues"
→ Load: docs/context/cicd-context.md
→ Reference: docs/recipes/fix-ci-pipeline.md
→ Quick commands: docs/reference/commands.md

User: "Add a new endpoint to get books by author" 
→ Load: docs/context/development-context.md
→ Reference: docs/recipes/add-api-endpoint.md
→ Patterns: docs/atoms/api-conventions.md
```

## Recent Feature Additions

### Dark Mode Toggle (✅ Completed)
**Location**: `library-frontend/src/contexts/ThemeContext.tsx`, `library-frontend/src/components/common/ThemeToggle.tsx`

**Implementation**: 
- Cookie-based theme persistence (365 days)
- Available in Settings page and Login screen
- Material-UI theme switching with proper light/dark palettes
- All layout components (Sidebar, Header, AppLayout) support theme switching

**Usage**:
- **Settings Page**: Navigate to Settings → Appearance → Theme toggle
- **Login Screen**: Theme toggle in top-right corner
- **Persistence**: User preference automatically saved and restored

**Files Modified**:
- `src/contexts/ThemeContext.tsx` - Theme management context
- `src/components/common/ThemeToggle.tsx` - Reusable toggle component  
- `src/App.tsx` - Updated to use CustomThemeProvider
- `src/pages/Settings.tsx` - Enhanced with appearance section
- `src/components/layout/*` - All layout components updated for theme awareness
- `src/pages/Login.tsx` - Added theme toggle and dark mode support

This atomic documentation structure ensures AI agents get exactly the context they need for each specific task, resulting in faster, more accurate, and more focused assistance.