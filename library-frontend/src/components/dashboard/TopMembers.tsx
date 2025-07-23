import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Avatar,
  Box,
  Skeleton,
  Chip,
} from '@mui/material';
import { Person } from '@mui/icons-material';
import { TopMember } from '../../types';

interface TopMembersProps {
  members: TopMember[];
  isLoading: boolean;
}

export const TopMembers: React.FC<TopMembersProps> = ({ members, isLoading }) => {
  if (isLoading) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
            Top Active Members
          </Typography>
          <List>
            {Array.from({ length: 5 }).map((_, index) => (
              <ListItem key={index} sx={{ px: 0 }}>
                <ListItemAvatar>
                  <Skeleton variant="circular" width={40} height={40} />
                </ListItemAvatar>
                <ListItemText
                  primary={<Skeleton variant="text" width="60%" />}
                  secondary={<Skeleton variant="text" width="40%" />}
                />
                <Skeleton variant="rectangular" width={60} height={24} sx={{ borderRadius: 1 }} />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
          Top Active Members
        </Typography>
        
        <List sx={{ pt: 1 }}>
          {members.length === 0 ? (
            <Box sx={{ py: 3, textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                No member data available
              </Typography>
            </Box>
          ) : (
            members.map((member, index) => (
              <ListItem key={member.memberId} sx={{ px: 0, py: 1 }}>
                <ListItemAvatar>
                  <Avatar
                    sx={{
                      bgcolor: '#9C27B0',
                      width: 40,
                      height: 40,
                    }}
                  >
                    {member.memberName.split(' ').map(n => n[0]).join('').toUpperCase()}
                  </Avatar>
                </ListItemAvatar>
                <ListItemText
                  primary={
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>
                      {member.memberName}
                    </Typography>
                  }
                  secondary={
                    <Typography variant="caption" color="text.secondary">
                      {member.email}
                    </Typography>
                  }
                />
                <Box sx={{ textAlign: 'right' }}>
                  <Chip
                    label={`${member.totalBorrowings} books`}
                    size="small"
                    color="primary"
                    variant="outlined"
                  />
                  {member.currentBorrowings > 0 && (
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                      {member.currentBorrowings} current
                    </Typography>
                  )}
                </Box>
              </ListItem>
            ))
          )}
        </List>
      </CardContent>
    </Card>
  );
};

export default TopMembers;