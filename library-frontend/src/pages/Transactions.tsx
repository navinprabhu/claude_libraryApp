import React from 'react';
import { Box, Typography } from '@mui/material';

const Transactions: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
        Transactions
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Transaction management page is under development.
      </Typography>
    </Box>
  );
};

export default Transactions;