import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  Box,
  Skeleton,
} from '@mui/material';
import { format } from 'date-fns';
import { Transaction } from '../../types';

interface RecentTransactionsProps {
  transactions: Transaction[];
  isLoading: boolean;
  onViewAll?: () => void;
}

const getStatusColor = (action: string): 'success' | 'primary' | 'warning' | 'error' => {
  switch (action.toLowerCase()) {
    case 'returned':
      return 'success';
    case 'borrowed':
      return 'primary';
    case 'reserved':
      return 'warning';
    case 'overdue':
      return 'error';
    default:
      return 'primary';
  }
};

const getStatusLabel = (action: string): string => {
  switch (action.toLowerCase()) {
    case 'borrowed':
      return 'Active';
    case 'returned':
      return 'Completed';
    case 'reserved':
      return 'Pending';
    case 'overdue':
      return 'Overdue';
    default:
      return action;
  }
};

export const RecentTransactions: React.FC<RecentTransactionsProps> = ({
  transactions,
  isLoading,
  onViewAll,
}) => {
  if (isLoading) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
            Recent Transactions
          </Typography>
          <Box sx={{ mt: 2 }}>
            {Array.from({ length: 5 }).map((_, index) => (
              <Box key={index} sx={{ display: 'flex', alignItems: 'center', py: 1 }}>
                <Skeleton variant="text" width="30%" />
                <Skeleton variant="text" width="25%" sx={{ mx: 2 }} />
                <Skeleton variant="rectangular" width={80} height={24} sx={{ borderRadius: 1 }} />
                <Skeleton variant="text" width="15%" sx={{ ml: 'auto' }} />
              </Box>
            ))}
          </Box>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card sx={{ mb: 4 }}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
            Recent Transactions
          </Typography>
          {onViewAll && (
            <Button variant="text" color="primary" onClick={onViewAll}>
              View All
            </Button>
          )}
        </Box>

        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Member</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Book</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Action</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {transactions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                    <Typography variant="body2" color="text.secondary">
                      No recent transactions found
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                transactions.map((transaction) => (
                  <TableRow 
                    key={transaction.id}
                    sx={{ 
                      '&:hover': { 
                        bgcolor: 'rgba(0,0,0,0.02)' 
                      } 
                    }}
                  >
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        {transaction.memberName}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {transaction.bookTitle}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ textTransform: 'capitalize' }}>
                        {transaction.action}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={getStatusLabel(transaction.action)}
                        color={getStatusColor(transaction.action)}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {format(new Date(transaction.timestamp), 'MMM dd, yyyy')}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </CardContent>
    </Card>
  );
};

export default RecentTransactions;