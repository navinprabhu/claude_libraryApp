import React from 'react';
import { Grid, Box, Typography } from '@mui/material';
import { DashboardStats as DashboardStatsType } from '../../types';
import StatCard from './StatCard';

interface DashboardStatsProps {
  stats: DashboardStatsType | null;
  isLoading: boolean;
}

export const DashboardStats: React.FC<DashboardStatsProps> = ({ stats, isLoading }) => {
  const statsCards = [
    {
      title: 'Total Books',
      value: stats?.totalBooks || 0,
      icon: 'books' as const,
      color: 'blue',
      testId: 'total-books',
    },
    {
      title: 'Available Books',
      value: stats?.availableBooks || 0,
      icon: 'available' as const,
      color: 'green',
      testId: 'available-books',
    },
    {
      title: 'Books Borrowed',
      value: stats?.booksBorrowed || 0,
      icon: 'borrowed' as const,
      color: 'orange',
      testId: 'books-borrowed',
    },
    {
      title: 'Total Members',
      value: stats?.totalMembers || 0,
      icon: 'members' as const,
      color: 'purple',
      testId: 'total-members',
    },
    {
      title: 'Active Members',
      value: stats?.activeMembers || 0,
      icon: 'active' as const,
      color: 'blue',
      testId: 'active-members',
    },
    {
      title: 'Overdue Books',
      value: stats?.overdueBooks || 0,
      icon: 'overdue' as const,
      color: 'red',
      testId: 'overdue-books',
    },
  ];

  return (
    <Box sx={{ mb: 4 }}>
      <Typography variant="h5" component="h2" gutterBottom sx={{ fontWeight: 600, mb: 3 }}>
        Library Statistics
      </Typography>
      
      <Grid container spacing={3}>
        {statsCards.map((card) => (
          <Grid item xs={12} sm={6} md={4} key={card.testId}>
            <div data-cy={card.testId}>
              <StatCard
                title={card.title}
                value={card.value}
                icon={card.icon}
                color={card.color}
                isLoading={isLoading}
              />
            </div>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

export default DashboardStats;