import React from 'react';
import {
  Box,
  FormControlLabel,
  Switch,
  Typography,
  useTheme,
} from '@mui/material';
import { Brightness4, Brightness7 } from '@mui/icons-material';
import { useThemeMode } from '../../contexts/ThemeContext';

interface ThemeToggleProps {
  showLabel?: boolean;
  size?: 'small' | 'medium';
}

export const ThemeToggle: React.FC<ThemeToggleProps> = ({ 
  showLabel = true, 
  size = 'medium' 
}) => {
  const { isDarkMode, toggleTheme } = useThemeMode();
  const theme = useTheme();

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      {showLabel && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Brightness7 
            sx={{ 
              color: !isDarkMode ? theme.palette.primary.main : theme.palette.text.disabled,
              fontSize: size === 'small' ? 20 : 24 
            }} 
          />
          <Typography 
            variant={size === 'small' ? 'body2' : 'body1'} 
            color="text.secondary"
          >
            Theme
          </Typography>
        </Box>
      )}
      
      <FormControlLabel
        control={
          <Switch
            checked={isDarkMode}
            onChange={toggleTheme}
            size={size}
            sx={{
              '& .MuiSwitch-switchBase.Mui-checked': {
                color: theme.palette.primary.main,
              },
              '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                backgroundColor: theme.palette.primary.main,
                opacity: 0.5,
              },
            }}
          />
        }
        label={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Brightness4 
              sx={{ 
                color: isDarkMode ? theme.palette.primary.main : theme.palette.text.disabled,
                fontSize: size === 'small' ? 20 : 24 
              }} 
            />
            {showLabel && (
              <Typography 
                variant={size === 'small' ? 'body2' : 'body1'} 
                color="text.secondary"
              >
                {isDarkMode ? 'Dark' : 'Light'}
              </Typography>
            )}
          </Box>
        }
        sx={{ ml: 0 }}
      />
    </Box>
  );
};

export default ThemeToggle;