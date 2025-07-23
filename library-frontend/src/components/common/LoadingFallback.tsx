import React from 'react';
import { Box, CircularProgress, Typography, Fade } from '@mui/material';

interface LoadingFallbackProps {
  message?: string;
  size?: number;
  fullPage?: boolean;
}

export const LoadingFallback: React.FC<LoadingFallbackProps> = ({
  message = 'Loading...',
  size = 40,
  fullPage = false,
}) => {
  const containerStyles = fullPage
    ? {
        position: 'fixed' as const,
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        bgcolor: 'background.default',
        zIndex: 9999,
      }
    : {
        minHeight: '200px',
      };

  return (
    <Fade in timeout={300}>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 2,
          ...containerStyles,
        }}
      >
        <CircularProgress size={size} thickness={4} />
        <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
          {message}
        </Typography>
      </Box>
    </Fade>
  );
};

export default LoadingFallback;