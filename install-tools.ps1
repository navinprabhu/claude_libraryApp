 # Install Node.js (if not already installed)
  winget install OpenJS.NodeJS

  # Create React project
  cd C:\Users\User\projects
  npx create-react-app library-frontend --template typescript
  cd library-frontend

  # Install all dependencies
  npm install @mui/material @emotion/react @emotion/styled @mui/icons-material @mui/x-data-grid axios react-router-dom @tanstack/react-query react-hook-form @hookform/resolvers zod date-fns recharts
  npm install --save-dev cypress @playwright/test @testing-library/react @testing-library/jest-dom @testing-library/user-event jest-environment-jsdom @types/jest supertest axios-mock-adapter tailwindcss postcss autoprefixer

  # Install global tools
  npm install -g newman lighthouse

  # Install k6
  winget install k6.k6

  # Initialize Playwright
  npx playwright install

  Write-Host "All tools installed successfully!" -ForegroundColor Green
  
  # Verify all tools are installed correctly
  node --version
  npm --version
  npx tsc --version
  cypress --version
  npx playwright --version
  lighthouse --version
  newman --version
  
 
  Write-Host "Fix Audit package"
  npm audit fix --force
  Write-Host "Run 'npm start' in the library-frontend directory to begin development" -ForegroundColor Cyan
  
  
  dotnet add package Microsoft.AspNetCore.Mvc.Testing
  dotnet add package FluentAssertions
  dotnet add package Testcontainers