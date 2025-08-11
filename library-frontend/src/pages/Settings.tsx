import React from 'react';
import { 
  Box, 
  Typography, 
  Card, 
  CardContent, 
  Divider,
  Grid,
} from '@mui/material';
import ThemeToggle from '../components/common/ThemeToggle';

const Settings: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
        Settings
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Customize your application preferences
      </Typography>

      <Grid container spacing={3}>
        {/* Appearance Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" component="h2" gutterBottom sx={{ fontWeight: 600 }}>
                Appearance
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Customize the look and feel of the application
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              <Box sx={{ py: 1 }}>
                <ThemeToggle />
                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                  Choose between light and dark theme. Your preference will be saved and persist across sessions.
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Additional Settings Placeholder */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" component="h2" gutterBottom sx={{ fontWeight: 600 }}>
                General
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                General application settings
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              <Typography variant="body2" color="text.secondary">
                Additional settings will be available here in future updates.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Settings;