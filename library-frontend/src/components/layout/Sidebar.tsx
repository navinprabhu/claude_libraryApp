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
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { useThemeMode } from '../../contexts/ThemeContext';
import {
  Dashboard,
  MenuBook,
  People,
  SwapHoriz,
  Assessment,
  Settings,
  Search,
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
  const theme = useTheme();
  const { isDarkMode } = useThemeMode();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const handleNavigation = (path: string) => {
    navigate(path);
    // Only close sidebar on mobile devices
    if (isMobile) {
      onClose();
    }
  };

  // Dynamic sidebar styling based on theme mode
  const sidebarStyles = {
    width: DRAWER_WIDTH,
    height: '100%',
    bgcolor: isDarkMode ? 'background.paper' : theme.palette.primary.main,
    borderRight: isDarkMode ? `1px solid ${theme.palette.divider}` : 'none',
  };

  const textColor = isDarkMode ? 'text.primary' : 'white';
  const secondaryTextColor = isDarkMode ? 'text.secondary' : 'rgba(255,255,255,0.8)';
  const dividerColor = isDarkMode ? 'divider' : 'rgba(255,255,255,0.2)';
  const hoverBgColor = isDarkMode ? 'action.hover' : 'rgba(255,255,255,0.1)';
  const activeBgColor = isDarkMode ? 'action.selected' : 'rgba(255,255,255,0.15)';

  const drawerContent = (
    <Box sx={sidebarStyles}>
      {/* Logo/Brand Section */}
      <Box sx={{ p: 3, color: textColor }}>
        <Typography variant="h5" component="h1" fontWeight="bold">
          Library App
        </Typography>
        <Typography variant="body2" sx={{ color: secondaryTextColor, mt: 1 }}>
          Management System
        </Typography>
      </Box>

      <Divider sx={{ bgcolor: dividerColor }} />

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
                  color: textColor,
                  bgcolor: isActive ? activeBgColor : 'transparent',
                  '&:hover': {
                    bgcolor: hoverBgColor,
                  },
                  py: 1.5,
                }}
                data-cy={`nav-${item.id}`}
              >
                <ListItemIcon sx={{ color: textColor, minWidth: 40 }}>
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
            bgcolor: isDarkMode ? 'action.hover' : 'rgba(255,255,255,0.1)',
            color: textColor,
            borderRadius: 2,
            px: 2,
            py: 1,
            cursor: 'pointer',
            '&:hover': {
              bgcolor: isDarkMode ? 'action.selected' : 'rgba(255,255,255,0.15)',
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