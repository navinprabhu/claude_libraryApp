import React, { useState, useEffect } from 'react';
import { Grid, Box, Typography, Alert } from '@mui/material';
import {
  DashboardStats as DashboardStatsType,
  Transaction,
  BookCategory,
  TopMember,
  Alert as AlertType,
} from '../types';
import apiClient from '../services/api';
import DashboardStats from '../components/dashboard/DashboardStats';
import RecentTransactions from '../components/dashboard/RecentTransactions';
import BookCategories from '../components/dashboard/BookCategories';
import TopMembers from '../components/dashboard/TopMembers';
import AlertsPanel from '../components/dashboard/AlertsPanel';

export const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStatsType | null>(null);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [categories, setCategories] = useState<BookCategory[]>([]);
  const [topMembers, setTopMembers] = useState<TopMember[]>([]);
  const [alerts, setAlerts] = useState<AlertType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        setError(null);

        // Fetch all dashboard data in parallel
        const [
          statsResponse,
          transactionsResponse,
          categoriesResponse,
          membersResponse,
          alertsResponse,
        ] = await Promise.allSettled([
          apiClient.getDashboardStats(),
          apiClient.getRecentTransactions(10),
          apiClient.getBookCategories(),
          apiClient.getTopMembers(5),
          apiClient.getAlerts(),
        ]);

        // Handle stats
        if (statsResponse.status === 'fulfilled') {
          setStats(statsResponse.value);
        } else {
          console.error('Failed to fetch stats:', statsResponse.reason);
        }

        // Handle transactions
        if (transactionsResponse.status === 'fulfilled') {
          setTransactions(transactionsResponse.value);
        } else {
          console.error('Failed to fetch transactions:', transactionsResponse.reason);
        }

        // Handle categories
        if (categoriesResponse.status === 'fulfilled') {
          setCategories(categoriesResponse.value);
        } else {
          console.error('Failed to fetch categories:', categoriesResponse.reason);
        }

        // Handle top members
        if (membersResponse.status === 'fulfilled') {
          setTopMembers(membersResponse.value);
        } else {
          console.error('Failed to fetch top members:', membersResponse.reason);
        }

        // Handle alerts
        if (alertsResponse.status === 'fulfilled') {
          setAlerts(alertsResponse.value);
        } else {
          console.error('Failed to fetch alerts:', alertsResponse.reason);
        }

      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError('Failed to load dashboard data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  // Show error if all API calls failed
  if (error && !loading) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
        <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
          Dashboard
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Unable to load dashboard data. Please check your connection and try again.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600, mb: 4 }}>
        Dashboard
      </Typography>

      {/* Statistics Cards */}
      <DashboardStats stats={stats} isLoading={loading} />

      {/* Middle Section - Transactions and Categories */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} lg={8}>
          <RecentTransactions 
            transactions={transactions} 
            isLoading={loading}
            onViewAll={() => console.log('View all transactions')}
          />
        </Grid>
        <Grid item xs={12} lg={4}>
          <BookCategories categories={categories} isLoading={loading} />
        </Grid>
      </Grid>

      {/* Bottom Section - Members, Statistics, Alerts */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6} lg={4}>
          <TopMembers members={topMembers} isLoading={loading} />
        </Grid>
        
        <Grid item xs={12} md={6} lg={4}>
          {/* Placeholder for Library Statistics Chart */}
          <Box 
            sx={{ 
              height: 300, 
              border: '2px dashed #e0e0e0', 
              borderRadius: 2,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: '#fafafa'
            }}
          >
            <Typography variant="body2" color="text.secondary">
              Library Statistics Chart
              <br />
              (Coming Soon)
            </Typography>
          </Box>
        </Grid>
        
        <Grid item xs={12} md={12} lg={4}>
          <AlertsPanel alerts={alerts} isLoading={loading} />
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;