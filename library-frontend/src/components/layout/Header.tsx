import React, { useState } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Box,
  IconButton,
  Badge,
  Menu,
  MenuItem,
  Avatar,
  InputBase,
  Paper,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Search as SearchIcon,
  Notifications,
  Message,
  AccountCircle,
  Logout,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';

interface HeaderProps {
  onMenuClick: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onMenuClick }) => {
  const { user, logout } = useAuth();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleProfileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    handleMenuClose();
  };

  return (
    <AppBar
      position="fixed"
      sx={{
        width: { md: `calc(100% - 280px)` },
        ml: { md: '280px' },
        bgcolor: 'white',
        color: 'text.primary',
        boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
        borderBottom: '1px solid #e0e0e0',
      }}
    >
      <Toolbar sx={{ justifyContent: 'space-between' }}>
        {/* Left Section - Menu & Search */}
        <Box sx={{ display: 'flex', alignItems: 'center', flex: 1 }}>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={onMenuClick}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>

          <Paper
            component="form"
            sx={{
              p: '2px 4px',
              display: 'flex',
              alignItems: 'center',
              width: { xs: 200, sm: 400 },
              bgcolor: '#f5f5f5',
              boxShadow: 'none',
              border: '1px solid #e0e0e0',
            }}
          >
            <IconButton sx={{ p: '10px' }} aria-label="search">
              <SearchIcon />
            </IconButton>
            <InputBase
              sx={{ ml: 1, flex: 1 }}
              placeholder="Search books, members..."
              inputProps={{ 'aria-label': 'search' }}
            />
          </Paper>
        </Box>

        {/* Right Section - Notifications & Profile */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {/* Message Icon */}
          <IconButton color="inherit">
            <Badge badgeContent={2} color="primary">
              <Message />
            </Badge>
          </IconButton>

          {/* Notifications Icon */}
          <IconButton color="inherit">
            <Badge badgeContent={5} color="error">
              <Notifications />
            </Badge>
          </IconButton>

          {/* User Profile */}
          <Box sx={{ display: 'flex', alignItems: 'center', ml: 2 }}>
            <Typography variant="body2" sx={{ mr: 1, display: { xs: 'none', sm: 'block' } }}>
              Hi, {user?.username || 'Librarian'}
            </Typography>
            <IconButton
              size="large"
              edge="end"
              aria-label="account of current user"
              aria-controls="primary-search-account-menu"
              aria-haspopup="true"
              onClick={handleProfileMenuOpen}
              color="inherit"
            >
              <Avatar sx={{ width: 32, height: 32, bgcolor: '#4285F4' }}>
                {user?.username?.charAt(0).toUpperCase() || 'L'}
              </Avatar>
            </IconButton>
          </Box>
        </Box>

        {/* Profile Menu */}
        <Menu
          anchorEl={anchorEl}
          anchorOrigin={{
            vertical: 'bottom',
            horizontal: 'right',
          }}
          keepMounted
          transformOrigin={{
            vertical: 'top',
            horizontal: 'right',
          }}
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
        >
          <MenuItem onClick={handleMenuClose}>
            <AccountCircle sx={{ mr: 2 }} />
            Profile
          </MenuItem>
          <MenuItem onClick={handleLogout}>
            <Logout sx={{ mr: 2 }} />
            Logout
          </MenuItem>
        </Menu>
      </Toolbar>
    </AppBar>
  );
};

export default Header;