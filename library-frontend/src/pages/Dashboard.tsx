import React from 'react';
import { Grid, Box, Typography, Alert, Button, Fab } from '@mui/material';
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
      <DashboardStats stats={stats.data} isLoading={stats.isLoading} />

      {/* Middle Section - Transactions and Categories */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} lg={8}>
          <RecentTransactions 
            transactions={transactions.data} 
            isLoading={transactions.isLoading}
            onViewAll={() => console.log('View all transactions')}
          />
        </Grid>
        <Grid item xs={12} lg={4}>
          <BookCategories categories={categories.data} isLoading={categories.isLoading} />
        </Grid>
      </Grid>

      {/* Bottom Section - Members, Statistics, Alerts */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6} lg={4}>
          <TopMembers members={members.data} isLoading={members.isLoading} />
        </Grid>
        
        <Grid item xs={12} md={6} lg={4}>
          <LibraryStatisticsChart isLoading={isLoading} />
        </Grid>
        
        <Grid item xs={12} md={12} lg={4}>
          <AlertsPanel alerts={alerts.data} isLoading={alerts.isLoading} />
        </Grid>
      </Grid>

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