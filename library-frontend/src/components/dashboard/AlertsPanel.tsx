import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Badge,
  Box,
  Skeleton,
  Chip,
} from '@mui/material';
import {
  Warning,
  Info,
  Error,
  CheckCircle,
} from '@mui/icons-material';
import { format } from 'date-fns';
import { Alert } from '../../types';

interface AlertsPanelProps {
  alerts: Alert[];
  isLoading: boolean;
}

const getAlertIcon = (severity: string) => {
  switch (severity) {
    case 'error':
      return <Error color="error" />;
    case 'warning':
      return <Warning color="warning" />;
    case 'success':
      return <CheckCircle color="success" />;
    default:
      return <Info color="info" />;
  }
};

const getAlertColor = (severity: string): "error" | "warning" | "success" | "info" => {
  switch (severity) {
    case 'error':
      return 'error';
    case 'warning':
      return 'warning';
    case 'success':
      return 'success';
    default:
      return 'info';
  }
};

export const AlertsPanel: React.FC<AlertsPanelProps> = ({ alerts, isLoading }) => {
  if (isLoading) {
    return (
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
            <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
              Alerts & Notifications
            </Typography>
            <Skeleton variant="circular" width={24} height={24} />
          </Box>
          <List>
            {Array.from({ length: 3 }).map((_, index) => (
              <ListItem key={index} sx={{ px: 0 }}>
                <ListItemIcon>
                  <Skeleton variant="circular" width={24} height={24} />
                </ListItemIcon>
                <ListItemText
                  primary={<Skeleton variant="text" width="80%" />}
                  secondary={<Skeleton variant="text" width="50%" />}
                />
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
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
          <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
            Alerts & Notifications
          </Typography>
          {alerts.length > 0 && (
            <Badge badgeContent={alerts.length} color="error">
              <Warning color="action" />
            </Badge>
          )}
        </Box>
        
        <List sx={{ pt: 0 }}>
          {alerts.length === 0 ? (
            <Box sx={{ py: 3, textAlign: 'center' }}>
              <CheckCircle color="success" sx={{ fontSize: 48, mb: 2 }} />
              <Typography variant="body2" color="text.secondary">
                No alerts at this time
              </Typography>
              <Typography variant="caption" color="text.secondary">
                All systems running smoothly
              </Typography>
            </Box>
          ) : (
            alerts.map((alert) => (
              <ListItem key={alert.id} sx={{ px: 0, py: 1, alignItems: 'flex-start' }}>
                <ListItemIcon sx={{ minWidth: 36 }}>
                  {getAlertIcon(alert.severity)}
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        {alert.message}
                      </Typography>
                      <Chip
                        label={alert.type}
                        size="small"
                        color={getAlertColor(alert.severity)}
                        variant="outlined"
                        sx={{ fontSize: '0.75rem', height: 20 }}
                      />
                    </Box>
                  }
                  secondary={
                    <Typography variant="caption" color="text.secondary">
                      {format(new Date(alert.timestamp), 'MMM dd, yyyy HH:mm')}
                    </Typography>
                  }
                />
              </ListItem>
            ))
          )}
        </List>
      </CardContent>
    </Card>
  );
};

export default AlertsPanel;