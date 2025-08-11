import React, { useState } from 'react';
import { Box } from '@mui/material';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';

const DRAWER_WIDTH = 280;

export const AppLayout: React.FC = () => {
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  return (
    <Box sx={{ display: 'flex' }}>
      {/* Header */}
      <Header onMenuClick={handleDrawerToggle} />
      
      {/* Sidebar */}
      <Sidebar open={mobileOpen} onClose={handleDrawerToggle} />

      {/* Main Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { 
            xs: '100%', 
            sm: '100%',
            md: `calc(100% - ${DRAWER_WIDTH}px)` 
          },
          ml: {
            xs: 0,
            sm: 0, 
            md: 0 // Margin handled by width calculation
          },
          mt: 8, // Account for AppBar height (64px)
          minHeight: 'calc(100vh - 64px)',
          bgcolor: '#f8f9fa',
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default AppLayout;