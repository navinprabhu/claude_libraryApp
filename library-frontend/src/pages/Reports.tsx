import React from 'react';
import { Box, Typography } from '@mui/material';

const Reports: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
        Reports
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Reports and analytics page is under development.
      </Typography>
    </Box>
  );
};

export default Reports;