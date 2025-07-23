import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  LinearProgress,
  Skeleton,
} from '@mui/material';
import { BookCategory } from '../../types';

interface BookCategoriesProps {
  categories: BookCategory[];
  isLoading: boolean;
}

const categoryColors = {
  Fiction: '#4285F4',
  Science: '#34A853',
  History: '#FBBC05',
  Romance: '#9C27B0',
  Mystery: '#EA4335',
  Biography: '#6366F1',
};

export const BookCategories: React.FC<BookCategoriesProps> = ({ categories, isLoading }) => {
  if (isLoading) {
    return (
      <Card sx={{ mb: 4 }}>
        <CardContent>
          <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
            Book Categories
          </Typography>
          <Box sx={{ mt: 3 }}>
            {Array.from({ length: 6 }).map((_, index) => (
              <Box key={index} sx={{ mb: 3 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Skeleton variant="text" width="30%" />
                  <Skeleton variant="text" width="15%" />
                </Box>
                <Skeleton variant="rectangular" height={8} sx={{ borderRadius: 1 }} />
              </Box>
            ))}
          </Box>
        </CardContent>
      </Card>
    );
  }

  const maxBooks = Math.max(...categories.map(cat => cat.totalBooks));

  return (
    <Card sx={{ mb: 4 }}>
      <CardContent>
        <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
          Book Categories
        </Typography>
        
        <Box sx={{ mt: 3 }}>
          {categories.map((category) => {
            const percentage = (category.totalBooks / maxBooks) * 100;
            const color = categoryColors[category.category as keyof typeof categoryColors] || '#6366F1';
            
            return (
              <Box key={category.category} sx={{ mb: 3 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>
                    {category.category}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {category.totalBooks} books
                  </Typography>
                </Box>
                
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ width: '100%' }}>
                    <LinearProgress
                      variant="determinate"
                      value={percentage}
                      sx={{
                        height: 8,
                        borderRadius: 1,
                        bgcolor: 'rgba(0,0,0,0.08)',
                        '& .MuiLinearProgress-bar': {
                          bgcolor: color,
                          borderRadius: 1,
                        },
                      }}
                    />
                  </Box>
                </Box>
                
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
                  <Typography variant="caption" color="text.secondary">
                    Available: {category.availableBooks}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Borrowed: {category.borrowedBooks}
                  </Typography>
                </Box>
              </Box>
            );
          })}
        </Box>
        
        {categories.length === 0 && (
          <Box sx={{ py: 3, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              No category data available
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default BookCategories;