# Session 2: React/TypeScript Frontend Development

## Mission
Build a modern, responsive React/TypeScript frontend for the LibraryApp dashboard. Work on branch `feature/react-dashboard-ui`.

## Context
You are building the frontend for a .NET 8 microservices library management system. The CLAUDE.md file contains full project context. Backend APIs are being enhanced simultaneously by another session.

## UI Requirements (Based on Dashboard Mockup)
Build an exact replica of the provided dashboard with these components:

### Layout Structure
```
├── Header (user info, search, notifications)
├── Sidebar Navigation (8 menu items)
└── Main Content Area
    ├── Stats Cards Grid (6 cards)
    ├── Recent Transactions Table
    ├── Book Categories Progress Bars
    └── Bottom Grid (Top Members, Statistics, Alerts)
```

### Color Scheme & Branding
- **Primary Blue:** #4285F4 (sidebar, primary buttons)
- **Success Green:** #34A853 (available books, completed)
- **Warning Orange:** #FBBC05 (borrowed books, pending)
- **Error Red:** #EA4335 (overdue, alerts)
- **Purple:** #9C27B0 (members, romance category)
- **White/Light Gray:** Background and cards

## Phase 1: Project Setup & Architecture
1. **Create React TypeScript project:**
   ```bash
   git checkout -b feature/react-dashboard-ui
   cd frontend  # or create new directory
   npx create-react-app library-dashboard --template typescript
   cd library-dashboard
   ```

2. **Install essential dependencies:**
   ```json
   {
     "dependencies": {
       "@mui/material": "^5.15.0",
       "@mui/icons-material": "^5.15.0",
       "@emotion/react": "^11.11.0",
       "@emotion/styled": "^11.11.0",
       "axios": "^1.6.0",
       "react-router-dom": "^6.20.0",
       "recharts": "^2.9.0",
       "@tanstack/react-query": "^5.0.0",
       "date-fns": "^2.30.0",
       "react-hook-form": "^7.48.0",
       "zod": "^3.22.0",
       "@hookform/resolvers": "^3.3.0"
     },
     "devDependencies": {
       "@types/node": "^20.0.0",
       "tailwindcss": "^3.3.0"
     }
   }
   ```

3. **Setup project structure:**
   ```
   src/
   ├── components/
   │   ├── common/
   │   ├── dashboard/
   │   ├── layout/
   │   └── ui/
   ├── pages/
   ├── services/
   ├── hooks/
   ├── types/
   ├── utils/
   └── contexts/
   ```

## Phase 2: Type Definitions & API Client
1. **Create TypeScript interfaces:**
   ```typescript
   // types/dashboard.ts
   interface DashboardStats {
     totalBooks: number;
     availableBooks: number;
     booksBorrowed: number;
     totalMembers: number;
     activeMembers: number;
     overdueBooks: number;
   }

   interface Transaction {
     id: string;
     memberName: string;
     bookTitle: string;
     type: 'borrow' | 'return' | 'reserve';
     status: 'completed' | 'active' | 'pending' | 'overdue';
     date: string;
   }

   interface BookCategory {
     name: string;
     count: number;
     color: string;
   }
   ```

2. **Setup API client with Axios:**
   ```typescript
   // services/api.ts
   const API_BASE_URL = 'http://localhost:5000/api';
   
   export const dashboardApi = {
     getStats: () => axios.get<DashboardStats>('/dashboard/stats'),
     getRecentTransactions: () => axios.get<Transaction[]>('/dashboard/recent-transactions'),
     getBookCategories: () => axios.get<BookCategory[]>('/dashboard/book-categories'),
     // ... other endpoints
   };
   ```

## Phase 3: Layout Components
1. **App Layout with Sidebar:**
   ```typescript
   // components/layout/AppLayout.tsx
   // Fixed sidebar with navigation items
   // Header with user info and search
   // Main content area with routing
   ```

2. **Sidebar Navigation:**
   - Dashboard (active state)
   - Books, Members, Transactions, Reports, Settings
   - Search functionality
   - Responsive collapse on mobile

3. **Header Component:**
   - Search bar with icon
   - User profile dropdown ("Hi, Librarian")
   - Notification bell with count
   - Message icon

## Phase 4: Dashboard Statistics Cards
1. **StatCard Component:**
   ```typescript
   interface StatCardProps {
     title: string;
     value: number;
     icon: ReactNode;
     color: string;
     trend?: { value: number; direction: 'up' | 'down' };
   }
   ```

2. **Six Statistics Cards:**
   - Total Books (blue, books icon)
   - Available Books (green, checkmark icon)
   - Books Borrowed (orange, bookmark icon)
   - Total Members (purple, people icon)
   - Active Members (blue, person icon)
   - Overdue Books (red, warning icon)

3. **Grid Layout:**
   - 3 columns on desktop
   - 2 columns on tablet
   - 1 column on mobile

## Phase 5: Recent Transactions Component
1. **Transaction Table:**
   ```typescript
   // components/dashboard/RecentTransactions.tsx
   // Table with member name, book title, action type, status, date
   // Status badges with appropriate colors
   // "View All" button
   ```

2. **Status Badges:**
   - Completed (green)
   - Active (blue)
   - Pending (orange)
   - Overdue (red)

## Phase 6: Book Categories Visualization
1. **Progress Bar Component:**
   ```typescript
   // components/dashboard/BookCategories.tsx
   // Horizontal progress bars for each category
   // Category names: Fiction, Science, History, Romance, Mystery, Biography
   // Counts and color-coded bars
   ```

2. **Category Data:**
   - Fiction: 45 (blue)
   - Science: 30 (green)
   - History: 25 (orange)
   - Romance: 20 (purple)
   - Mystery: 15 (red)
   - Biography: 10 (indigo)

## Phase 7: Bottom Dashboard Sections
1. **Top Active Members:**
   ```typescript
   // List of most active library members
   // Avatar, name, books borrowed count
   ```

2. **Library Statistics:**
   ```typescript
   // Monthly circulation data
   // Could include small charts using Recharts
   ```

3. **Alerts & Notifications:**
   ```typescript
   // Alert list with count indicator (3)
   // Different alert types with icons
   ```

## Phase 8: Authentication & Routing
1. **Authentication Context:**
   ```typescript
   // contexts/AuthContext.tsx
   // Login/logout state management
   // JWT token handling
   // User profile data
   ```

2. **Protected Routes:**
   ```typescript
   // Route protection based on user roles
   // Redirect to login if not authenticated
   ```

3. **Login Page:**
   ```typescript
   // Simple login form
   // Username/password fields
   // Remember me option
   // Library branding
   ```

## Phase 9: Data Fetching & State Management
1. **React Query Setup:**
   ```typescript
   // hooks/useDashboardData.ts
   // Fetch dashboard statistics
   // Cache management and auto-refresh
   // Error handling
   ```

2. **Custom Hooks:**
   ```typescript
   const useDashboardStats = () => useQuery({
     queryKey: ['dashboard', 'stats'],
     queryFn: dashboardApi.getStats,
     refetchInterval: 30000, // 30 seconds
   });
   ```

## Phase 10: Responsive Design & Polish
1. **Mobile Responsiveness:**
   - Collapsible sidebar
   - Responsive grid layouts
   - Touch-friendly interactions

2. **Loading States:**
   - Skeleton components for cards
   - Loading spinners for data
   - Error boundaries

3. **Accessibility:**
   - ARIA labels and roles
   - Keyboard navigation
   - Screen reader support

## Phase 11: Performance Optimization
1. **Code Splitting:**
   ```typescript
   // Lazy load non-critical components
   const Reports = lazy(() => import('./pages/Reports'));
   ```

2. **Memoization:**
   - React.memo for expensive components
   - useMemo for calculations
   - useCallback for event handlers

## Technical Requirements
- **Framework:** React 18+ with TypeScript
- **Styling:** Material-UI + Tailwind CSS (or styled-components)
- **State Management:** React Query + Context API
- **Routing:** React Router v6
- **API Integration:** Axios with interceptors
- **Build Tool:** Vite (recommended) or Create React App
- **Testing:** Jest + React Testing Library

## Development Environment
```json
// package.json scripts
{
  "scripts": {
    "dev": "vite --port 3000",
    "build": "tsc && vite build",
    "test": "jest",
    "lint": "eslint src --ext ts,tsx",
    "format": "prettier --write src/**/*.{ts,tsx}"
  }
}
```

## API Integration Points
Configure API client to work with:
- **Base URL:** `http://localhost:5000/api`
- **Authentication:** JWT Bearer tokens
- **CORS:** Ensure backend allows localhost:3000
- **Error Handling:** Consistent error response format

## Acceptance Criteria
- [ ] Pixel-perfect match to provided dashboard mockup
- [ ] Fully responsive design (mobile, tablet, desktop)
- [ ] TypeScript strict mode with no any types
- [ ] All dashboard data loads from backend APIs
- [ ] Authentication flow works end-to-end
- [ ] Navigation between all sections functional
- [ ] Performance: First contentful paint < 2s
- [ ] Accessibility score > 90 in Lighthouse
- [ ] Unit tests for critical components
- [ ] Ready for production deployment

## Iterative Development Approach
Work in 3-4 hour iterations:
1. **Iteration 1:** Project setup, layout, basic routing
2. **Iteration 2:** Dashboard stats cards, authentication
3. **Iteration 3:** Tables, charts, data integration
4. **Iteration 4:** Polish, responsive design, testing

## Commands to Get Started
```bash
# Create feature branch
git checkout -b feature/react-dashboard-ui

# Setup React project
npx create-react-app library-frontend --template typescript
cd library-frontend
npm install @mui/material @mui/icons-material axios react-router-dom

# Start development server
npm start
```

## Handoff Requirements
When complete, provide:
1. **Build artifacts** ready for deployment
2. **Environment configuration** documentation
3. **API integration** status and any issues
4. **Component documentation** with Storybook (optional)
5. **Deployment guide** for production

Remember: Backend APIs are being developed simultaneously, so be prepared to adapt to API changes and provide feedback on API requirements.