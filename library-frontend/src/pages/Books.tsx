import React from 'react';
import { Box, Typography, Card, CardContent, Button, Grid } from '@mui/material';
import { Add as AddIcon, MenuBook as BookIcon } from '@mui/icons-material';

const Books: React.FC = () => {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
          Books Management
        </Typography>
        <Button 
          variant="contained" 
          startIcon={<AddIcon />}
          data-cy="add-book-button"
        >
          Add Book
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6} lg={4}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <BookIcon sx={{ fontSize: 64, color: 'primary.main', mb: 2 }} />
              <Typography variant="h6" gutterBottom>
                Book Catalog
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Manage your library's book collection
              </Typography>
              <Button variant="outlined" size="small">
                View Catalog
              </Button>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6} lg={4}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <BookIcon sx={{ fontSize: 64, color: 'secondary.main', mb: 2 }} />
              <Typography variant="h6" gutterBottom>
                Categories
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Organize books by genre and topic
              </Typography>
              <Button variant="outlined" size="small">
                Manage Categories
              </Button>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6} lg={4}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <BookIcon sx={{ fontSize: 64, color: 'warning.main', mb: 2 }} />
              <Typography variant="h6" gutterBottom>
                Inventory
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Track book availability and condition
              </Typography>
              <Button variant="outlined" size="small">
                Check Inventory
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Books;