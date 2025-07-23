import React, { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  ToggleButton,
  ToggleButtonGroup,
  Skeleton,
  useTheme,
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
} from 'recharts';

interface LibraryStatisticsChartProps {
  isLoading?: boolean;
}

// Mock data for demonstration - in production, this would come from props or API
const monthlyData = [
  { month: 'Jan', borrowed: 65, returned: 62, new: 8 },
  { month: 'Feb', borrowed: 78, returned: 70, new: 12 },
  { month: 'Mar', borrowed: 82, returned: 75, new: 15 },
  { month: 'Apr', borrowed: 94, returned: 88, new: 10 },
  { month: 'May', borrowed: 89, returned: 85, new: 18 },
  { month: 'Jun', borrowed: 76, returned: 78, new: 14 },
];

const categoryData = [
  { name: 'Fiction', value: 45, color: '#4285F4' },
  { name: 'Science', value: 30, color: '#34A853' },
  { name: 'History', value: 25, color: '#FBBC05' },
  { name: 'Romance', value: 20, color: '#9C27B0' },
  { name: 'Mystery', value: 15, color: '#EA4335' },
];

const weeklyTrends = [
  { day: 'Mon', checkouts: 23, returns: 18 },
  { day: 'Tue', checkouts: 35, returns: 28 },
  { day: 'Wed', checkouts: 42, returns: 35 },
  { day: 'Thu', checkouts: 38, returns: 32 },
  { day: 'Fri', checkouts: 51, returns: 45 },
  { day: 'Sat', checkouts: 67, returns: 52 },
  { day: 'Sun', checkouts: 43, returns: 38 },
];

type ChartType = 'monthly' | 'categories' | 'weekly';

export const LibraryStatisticsChart: React.FC<LibraryStatisticsChartProps> = ({ 
  isLoading = false 
}) => {
  const [chartType, setChartType] = useState<ChartType>('monthly');
  const theme = useTheme();

  const handleChartTypeChange = (
    event: React.MouseEvent<HTMLElement>,
    newChartType: ChartType | null,
  ) => {
    if (newChartType !== null) {
      setChartType(newChartType);
    }
  };

  if (isLoading) {
    return (
      <Card sx={{ height: 400 }}>
        <CardContent>
          <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 600 }}>
            Library Statistics
          </Typography>
          <Skeleton variant="rectangular" height={60} sx={{ mb: 2 }} />
          <Skeleton variant="rectangular" height={280} />
        </CardContent>
      </Card>
    );
  }

  const renderChart = () => {
    switch (chartType) {
      case 'monthly':
        return (
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={monthlyData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis 
                dataKey="month" 
                tick={{ fontSize: 12 }}
                stroke={theme.palette.text.secondary}
              />
              <YAxis 
                tick={{ fontSize: 12 }}
                stroke={theme.palette.text.secondary}
              />
              <Tooltip 
                contentStyle={{
                  backgroundColor: theme.palette.background.paper,
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: 8,
                }}
              />
              <Bar dataKey="borrowed" fill="#4285F4" name="Books Borrowed" radius={[2, 2, 0, 0]} />
              <Bar dataKey="returned" fill="#34A853" name="Books Returned" radius={[2, 2, 0, 0]} />
              <Bar dataKey="new" fill="#9C27B0" name="New Books" radius={[2, 2, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        );

      case 'categories':
        return (
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie
                data={categoryData}
                cx="50%"
                cy="50%"
                innerRadius={60}
                outerRadius={100}
                paddingAngle={5}
                dataKey="value"
              >
                {categoryData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip 
                contentStyle={{
                  backgroundColor: theme.palette.background.paper,
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: 8,
                }}
              />
            </PieChart>
          </ResponsiveContainer>
        );

      case 'weekly':
        return (
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={weeklyTrends} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis 
                dataKey="day" 
                tick={{ fontSize: 12 }}
                stroke={theme.palette.text.secondary}
              />
              <YAxis 
                tick={{ fontSize: 12 }}
                stroke={theme.palette.text.secondary}
              />
              <Tooltip 
                contentStyle={{
                  backgroundColor: theme.palette.background.paper,
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: 8,
                }}
              />
              <Line 
                type="monotone" 
                dataKey="checkouts" 
                stroke="#4285F4" 
                strokeWidth={3}
                name="Checkouts"
                dot={{ fill: '#4285F4', strokeWidth: 2, r: 4 }}
              />
              <Line 
                type="monotone" 
                dataKey="returns" 
                stroke="#34A853" 
                strokeWidth={3}
                name="Returns"
                dot={{ fill: '#34A853', strokeWidth: 2, r: 4 }}
              />
            </LineChart>
          </ResponsiveContainer>
        );

      default:
        return null;
    }
  };

  return (
    <Card sx={{ height: 400 }}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
            Library Statistics
          </Typography>
          
          <ToggleButtonGroup
            value={chartType}
            exclusive
            onChange={handleChartTypeChange}
            size="small"
            sx={{ height: 32 }}
          >
            <ToggleButton value="monthly" sx={{ px: 2, fontSize: '0.75rem' }}>
              Monthly
            </ToggleButton>
            <ToggleButton value="weekly" sx={{ px: 2, fontSize: '0.75rem' }}>
              Weekly
            </ToggleButton>
            <ToggleButton value="categories" sx={{ px: 2, fontSize: '0.75rem' }}>
              Categories
            </ToggleButton>
          </ToggleButtonGroup>
        </Box>

        {renderChart()}

        {/* Chart Legend for Categories */}
        {chartType === 'categories' && (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mt: 2, justifyContent: 'center' }}>
            {categoryData.map((category) => (
              <Box key={category.name} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <Box
                  sx={{
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    backgroundColor: category.color,
                  }}
                />
                <Typography variant="caption" color="text.secondary">
                  {category.name} ({category.value})
                </Typography>
              </Box>
            ))}
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default LibraryStatisticsChart;