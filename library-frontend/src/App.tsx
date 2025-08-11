import React, { Suspense, lazy } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { CssBaseline, GlobalStyles } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { AuthProvider } from './contexts/AuthContext';
import { NotificationProvider } from './contexts/NotificationContext';
import { CustomThemeProvider } from './contexts/ThemeContext';
import ErrorBoundary from './components/common/ErrorBoundary';
import ProtectedRoute from './components/common/ProtectedRoute';
import LoadingFallback from './components/common/LoadingFallback';
import AppLayout from './components/layout/AppLayout';

// Lazy load components for code splitting
const Dashboard = lazy(() => import('./pages/Dashboard'));
const Login = lazy(() => import('./pages/Login'));
const Books = lazy(() => import('./pages/Books'));
const Members = lazy(() => import('./pages/Members'));
const Transactions = lazy(() => import('./pages/Transactions'));
const Reports = lazy(() => import('./pages/Reports'));
const Settings = lazy(() => import('./pages/Settings'));

// Create React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
      refetchOnWindowFocus: false,
    },
    mutations: {
      retry: 1,
    },
  },
});

// Global styles
const globalStyles = (
  <GlobalStyles
    styles={{
      '*': {
        boxSizing: 'border-box',
      },
      html: {
        WebkitFontSmoothing: 'antialiased',
        MozOsxFontSmoothing: 'grayscale',
      },
      body: {
        margin: 0,
        fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
      },
    }}
  />
);

// Page loading component
const PageSuspense: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <Suspense fallback={<LoadingFallback message="Loading page..." />}>
    {children}
  </Suspense>
);

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <CustomThemeProvider>
          <CssBaseline />
          {globalStyles}
          <NotificationProvider>
            <AuthProvider>
              <Router>
                <Routes>
                  {/* Public Routes */}
                  <Route 
                    path="/login" 
                    element={
                      <PageSuspense>
                        <Login />
                      </PageSuspense>
                    } 
                  />
                  
                  {/* Protected Routes */}
                  <Route
                    path="/"
                    element={
                      <ProtectedRoute>
                        <AppLayout />
                      </ProtectedRoute>
                    }
                  >
                    <Route 
                      index 
                      element={
                        <PageSuspense>
                          <Dashboard />
                        </PageSuspense>
                      } 
                    />
                    <Route 
                      path="books" 
                      element={
                        <PageSuspense>
                          <Books />
                        </PageSuspense>
                      } 
                    />
                    <Route 
                      path="members" 
                      element={
                        <PageSuspense>
                          <Members />
                        </PageSuspense>
                      } 
                    />
                    <Route 
                      path="transactions" 
                      element={
                        <PageSuspense>
                          <Transactions />
                        </PageSuspense>
                      } 
                    />
                    <Route 
                      path="reports" 
                      element={
                        <PageSuspense>
                          <Reports />
                        </PageSuspense>
                      } 
                    />
                    <Route 
                      path="settings" 
                      element={
                        <PageSuspense>
                          <Settings />
                        </PageSuspense>
                      } 
                    />
                  </Route>

                  {/* Catch all route */}
                  <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
              </Router>
            </AuthProvider>
          </NotificationProvider>
        </CustomThemeProvider>
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;