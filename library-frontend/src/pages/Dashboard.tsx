import React from 'react';
import { Box, Typography, Alert, Button, Fab } from '@mui/material';
import { Refresh as RefreshIcon } from '@mui/icons-material';
import { useDashboardData } from '../hooks/useDashboardData';
import DashboardStats from '../components/dashboard/DashboardStats';
import RecentTransactions from '../components/dashboard/RecentTransactions';
import BookCategories from '../components/dashboard/BookCategories';
import TopMembers from '../components/dashboard/TopMembers';
import AlertsPanel from '../components/dashboard/AlertsPanel';
import LibraryStatisticsChart from '../components/dashboard/LibraryStatisticsChart';

export const Dashboard: React.FC = () => {
  const {
    stats,
    transactions,
    categories,
    members,
    alerts,
    isLoading,
    hasError,
    refetchAll,
  } = useDashboardData();

  // Show error if critical data fails to load
  if (hasError && !isLoading) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert 
          severity="error" 
          sx={{ mb: 3 }}
          action={
            <Button color="inherit" size="small" onClick={refetchAll}>
              Retry
            </Button>
          }
        >
          Failed to load dashboard data. Please check your connection and try again.
        </Alert>
        <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
          Dashboard
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Some dashboard data may be unavailable. Click retry to reload.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
          Dashboard
        </Typography>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={refetchAll}
          size="small"
        >
          Refresh
        </Button>
      </Box>

      {/* Statistics Cards */}
      <DashboardStats stats={stats.data || null} isLoading={stats.isLoading} />

      {/* Middle Section - Transactions and Categories */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', lg: '2fr 1fr' },
          gap: 3,
          mb: 4
        }}
      >
        <RecentTransactions 
          transactions={transactions.data} 
          isLoading={transactions.isLoading}
          onViewAll={() => console.log('View all transactions')}
        />
        <BookCategories categories={categories.data} isLoading={categories.isLoading} />
      </Box>

      {/* Bottom Section - Members, Statistics, Alerts */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            md: 'repeat(2, 1fr)',
            lg: 'repeat(3, 1fr)'
          },
          gap: 3
        }}
      >
        <TopMembers members={members.data} isLoading={members.isLoading} />
        <LibraryStatisticsChart isLoading={isLoading} />
        <Box sx={{ gridColumn: { xs: '1', md: '1 / -1', lg: 'auto' } }}>
          <AlertsPanel alerts={alerts.data} isLoading={alerts.isLoading} />
        </Box>
      </Box>

      {/* Floating Action Button for Quick Refresh */}
      <Fab
        color="primary"
        aria-label="refresh"
        onClick={refetchAll}
        sx={{
          position: 'fixed',
          bottom: 16,
          right: 16,
          display: { xs: 'flex', sm: 'none' }, // Only show on mobile
        }}
      >
        <RefreshIcon />
      </Fab>
    </Box>
  );
};

export default Dashboard;