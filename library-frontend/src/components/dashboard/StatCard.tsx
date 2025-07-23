import React from 'react';
import { Card, CardContent, Typography, Box, Skeleton } from '@mui/material';
import { 
  MenuBook, 
  CheckCircle, 
  Bookmark, 
  People, 
  Person, 
  Warning 
} from '@mui/icons-material';

interface StatCardProps {
  title: string;
  value: number;
  icon: 'books' | 'available' | 'borrowed' | 'members' | 'active' | 'overdue';
  color: string;
  isLoading?: boolean;
}

const iconMap = {
  books: MenuBook,
  available: CheckCircle,
  borrowed: Bookmark,
  members: People,
  active: Person,
  overdue: Warning,
};

const colorMap = {
  blue: '#4285F4',
  green: '#34A853',
  orange: '#FBBC05',
  purple: '#9C27B0',
  red: '#EA4335',
};

export const StatCard: React.FC<StatCardProps> = ({ 
  title, 
  value, 
  icon, 
  color, 
  isLoading = false 
}) => {
  const IconComponent = iconMap[icon];
  const iconColor = colorMap[color as keyof typeof colorMap] || color;

  if (isLoading) {
    return (
      <Card 
        sx={{ 
          height: 140,
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
        }}
      >
        <CardContent>
          <Skeleton variant="circular" width={48} height={48} />
          <Skeleton variant="text" width="60%" sx={{ mt: 2 }} />
          <Skeleton variant="text" width="40%" />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card 
      sx={{ 
        height: 140,
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'space-between',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: '0 4px 20px rgba(0,0,0,0.1)',
        },
      }}
      data-cy={`stat-card-${icon}`}
    >
      <CardContent sx={{ pb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box sx={{ flex: 1 }}>
            <Typography 
              variant="body2" 
              color="text.secondary" 
              gutterBottom
              sx={{ fontSize: '0.875rem', fontWeight: 500 }}
            >
              {title}
            </Typography>
            <Typography 
              variant="h4" 
              component="div" 
              sx={{ 
                fontWeight: 'bold',
                color: 'text.primary',
                lineHeight: 1.2,
              }}
              data-cy={`${icon.replace(/([A-Z])/g, '-$1').toLowerCase()}`}
            >
              {value.toLocaleString()}
            </Typography>
          </Box>
          
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              bgcolor: `${iconColor}15`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              ml: 2,
            }}
          >
            <IconComponent 
              sx={{ 
                color: iconColor,
                fontSize: 24,
              }} 
            />
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};

export default StatCard;