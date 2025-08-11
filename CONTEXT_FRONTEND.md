# Frontend Development Context (~400 tokens)

## Stack
React 19.1.0 + TypeScript + Material-UI v7 + React Query v5

## Key Features
- ✅ Dark/Light mode toggle with cookie persistence
- Global search, responsive design, lazy loading

## Theme System
```typescript
// Use theme context
const { isDarkMode, toggleTheme } = useThemeMode();
<Box sx={{ bgcolor: isDarkMode ? 'background.paper' : 'white' }}>
```

## Structure
```
src/
├── contexts/ThemeContext.tsx      # Theme management
├── components/common/ThemeToggle.tsx  # Toggle component
├── components/layout/             # Header, Sidebar, AppLayout  
├── pages/                        # Dashboard, Login, Settings
└── services/api.ts               # API client
```

## Development Commands
```bash
cd library-frontend
npm start                 # Port 3000
npm run build            # Production build
npm test                 # Run tests
```

## API Integration
- Base URL: http://localhost:5000 (API Gateway)
- JWT tokens in localStorage
- React Query for caching