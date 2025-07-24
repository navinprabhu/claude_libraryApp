import React from 'react';
import { Box, Typography, Card, CardContent, Button } from '@mui/material';
import { PersonAdd as PersonAddIcon, People as PeopleIcon } from '@mui/icons-material';

const Members: React.FC = () => {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
          Members Management
        </Typography>
        <Button variant="contained" startIcon={<PersonAddIcon />}>
          Add Member
        </Button>
      </Box>

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            md: 'repeat(2, 1fr)',
            lg: 'repeat(3, 1fr)'
          },
          gap: 3
        }}
      >
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <PeopleIcon sx={{ fontSize: 64, color: 'primary.main', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              Member Directory
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              View and manage library members
            </Typography>
            <Button variant="outlined" size="small">
              View Directory
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <PeopleIcon sx={{ fontSize: 64, color: 'secondary.main', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              Membership Plans
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Manage different membership types
            </Typography>
            <Button variant="outlined" size="small">
              View Plans
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <PeopleIcon sx={{ fontSize: 64, color: 'warning.main', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              Member Activity
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Track member borrowing history
            </Typography>
            <Button variant="outlined" size="small">
              View Activity
            </Button>
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
};

export default Members;