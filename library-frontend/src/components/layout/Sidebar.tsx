import React from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Box,
  Typography,
  Divider,
} from '@mui/material';
import {
  Dashboard,
  MenuBook,
  People,
  SwapHoriz,
  Assessment,
  Settings,
  Search,
  Notifications,
} from '@mui/icons-material';
import { useLocation, useNavigate } from 'react-router-dom';

const DRAWER_WIDTH = 280;

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

const menuItems = [
  { id: 'dashboard', label: 'Dashboard', icon: Dashboard, path: '/' },
  { id: 'books', label: 'Books', icon: MenuBook, path: '/books' },
  { id: 'members', label: 'Members', icon: People, path: '/members' },
  { id: 'transactions', label: 'Transactions', icon: SwapHoriz, path: '/transactions' },
  { id: 'reports', label: 'Reports', icon: Assessment, path: '/reports' },
  { id: 'settings', label: 'Settings', icon: Settings, path: '/settings' },
];

export const Sidebar: React.FC<SidebarProps> = ({ open, onClose }) => {
  const location = useLocation();
  const navigate = useNavigate();

  const handleNavigation = (path: string) => {
    navigate(path);
    onClose();
  };

  const drawerContent = (
    <Box sx={{ width: DRAWER_WIDTH, height: '100%', bgcolor: '#4285F4' }}>
      {/* Logo/Brand Section */}
      <Box sx={{ p: 3, color: 'white' }}>
        <Typography variant="h5" component="h1" fontWeight="bold">
          Library App
        </Typography>
        <Typography variant="body2" sx={{ opacity: 0.8, mt: 1 }}>
          Management System
        </Typography>
      </Box>

      <Divider sx={{ bgcolor: 'rgba(255,255,255,0.2)' }} />

      {/* Navigation Menu */}
      <List sx={{ px: 2, pt: 2 }}>
        {menuItems.map((item) => {
          const Icon = item.icon;
          const isActive = location.pathname === item.path;
          
          return (
            <ListItem key={item.id} disablePadding sx={{ mb: 1 }}>
              <ListItemButton
                onClick={() => handleNavigation(item.path)}
                sx={{
                  borderRadius: 2,
                  color: 'white',
                  bgcolor: isActive ? 'rgba(255,255,255,0.15)' : 'transparent',
                  '&:hover': {
                    bgcolor: 'rgba(255,255,255,0.1)',
                  },
                  py: 1.5,
                }}
                data-cy={`nav-${item.id}`}
              >
                <ListItemIcon sx={{ color: 'white', minWidth: 40 }}>
                  <Icon />
                </ListItemIcon>
                <ListItemText 
                  primary={item.label}
                  primaryTypographyProps={{
                    fontWeight: isActive ? 600 : 400,
                  }}
                />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>

      {/* Search Section */}
      <Box sx={{ px: 3, mt: 4 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            bgcolor: 'rgba(255,255,255,0.1)',
            color: 'white',
            borderRadius: 2,
            px: 2,
            py: 1,
            cursor: 'pointer',
            '&:hover': {
              bgcolor: 'rgba(255,255,255,0.15)',
            },
          }}
        >
          <Search sx={{ mr: 1, fontSize: 20 }} />
          <Typography variant="body2">Search...</Typography>
        </Box>
      </Box>
    </Box>
  );

  return (
    <>
      {/* Desktop Drawer */}
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', md: 'block' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: DRAWER_WIDTH,
            border: 'none',
          },
        }}
        data-cy="sidebar"
      >
        {drawerContent}
      </Drawer>

      {/* Mobile Drawer */}
      <Drawer
        variant="temporary"
        open={open}
        onClose={onClose}
        ModalProps={{
          keepMounted: true,
        }}
        sx={{
          display: { xs: 'block', md: 'none' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: DRAWER_WIDTH,
            border: 'none',
          },
        }}
      >
        {drawerContent}
      </Drawer>
    </>
  );
};

export default Sidebar;