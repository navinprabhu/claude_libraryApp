import React, { useState, useRef } from 'react';
import {
  Paper,
  InputBase,
  IconButton,
  Popper,
  ClickAwayListener,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Typography,
  Chip,
  Box,
  Divider,
  CircularProgress,
  Fade,
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  MenuBook as BookIcon,
  Person as PersonIcon,
  SwapHoriz as TransactionIcon,
  TrendingUp as TrendingIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useSearch, useSearchSuggestions, SearchResult } from '../../hooks/useSearch';

interface GlobalSearchProps {
  placeholder?: string;
  width?: number | string;
}

const getResultIcon = (type: SearchResult['type']) => {
  switch (type) {
    case 'book':
      return <BookIcon fontSize="small" />;
    case 'member':
      return <PersonIcon fontSize="small" />;
    case 'transaction':
      return <TransactionIcon fontSize="small" />;
    default:
      return <SearchIcon fontSize="small" />;
  }
};

const getResultColor = (type: SearchResult['type']) => {
  switch (type) {
    case 'book':
      return '#4285F4';
    case 'member':
      return '#9C27B0';
    case 'transaction':
      return '#34A853';
    default:
      return '#666';
  }
};

export const GlobalSearch: React.FC<GlobalSearchProps> = ({ 
  placeholder = "Search books, members...",
  width = 400 
}) => {
  const [open, setOpen] = useState(false);
  const anchorRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  const { query, setQuery, results, isLoading, clearSearch } = useSearch();
  const suggestions = useSearchSuggestions(query);

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setQuery(value);
    setOpen(value.length > 0);
  };

  const handleResultClick = (result: SearchResult) => {
    navigate(result.url);
    setOpen(false);
    clearSearch();
  };

  const handleSuggestionClick = (suggestion: string) => {
    setQuery(suggestion);
    setOpen(true);
  };

  const handleClear = () => {
    clearSearch();
    setOpen(false);
  };

  const handleClickAway = () => {
    setOpen(false);
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      setOpen(false);
    }
  };

  const showResults = query.length >= 2;
  const showSuggestions = query.length > 0 && query.length < 2;

  return (
    <ClickAwayListener onClickAway={handleClickAway}>
      <Box ref={anchorRef} sx={{ position: 'relative' }}>
        <Paper
          component="form"
          sx={{
            p: '2px 4px',
            display: 'flex',
            alignItems: 'center',
            width,
            bgcolor: '#f5f5f5',
            boxShadow: 'none',
            border: '1px solid #e0e0e0',
            '&:focus-within': {
              boxShadow: '0 0 0 2px rgba(66, 133, 244, 0.2)',
              borderColor: '#4285F4',
            },
          }}
          onSubmit={(e) => e.preventDefault()}
        >
          <IconButton sx={{ p: '10px' }} aria-label="search">
            <SearchIcon />
          </IconButton>
          
          <InputBase
            sx={{ ml: 1, flex: 1 }}
            placeholder={placeholder}
            value={query}
            onChange={handleInputChange}
            onKeyDown={handleKeyDown}
            inputProps={{ 'aria-label': 'search', 'data-cy': 'search-input' }}
          />

          {isLoading && (
            <CircularProgress size={20} sx={{ mr: 1 }} />
          )}

          {query && (
            <IconButton
              size="small"
              onClick={handleClear}
              sx={{ p: '4px' }}
              aria-label="clear search"
            >
              <ClearIcon fontSize="small" />
            </IconButton>
          )}
        </Paper>

        <Popper
          open={open}
          anchorEl={anchorRef.current}
          placement="bottom-start"
          style={{ width: anchorRef.current?.offsetWidth, zIndex: 1300 }}
          transition
        >
          {({ TransitionProps }) => (
            <Fade {...TransitionProps} timeout={200}>
              <Paper
                sx={{
                  mt: 0.5,
                  maxHeight: 400,
                  overflow: 'auto',
                  boxShadow: 3,
                  border: '1px solid #e0e0e0',
                }}
              >
                {/* Search Suggestions */}
                {showSuggestions && suggestions.length > 0 && (
                  <>
                    <Box sx={{ p: 2, pb: 1 }}>
                      <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                        <TrendingIcon fontSize="small" sx={{ mr: 0.5, verticalAlign: 'middle' }} />
                        Popular Searches
                      </Typography>
                    </Box>
                    <List dense>
                      {suggestions.map((suggestion, index) => (
                        <ListItem
                          key={index}
                          button
                          onClick={() => handleSuggestionClick(suggestion)}
                          sx={{ py: 0.5 }}
                        >
                          <ListItemText 
                            primary={suggestion}
                            primaryTypographyProps={{ variant: 'body2' }}
                          />
                        </ListItem>
                      ))}
                    </List>
                  </>
                )}

                {/* Search Results */}
                {showResults && (
                  <>
                    {results.length > 0 ? (
                      <>
                        <Box sx={{ p: 2, pb: 1 }}>
                          <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                            {results.length} result{results.length !== 1 ? 's' : ''} found
                          </Typography>
                        </Box>
                        <List dense>
                          {results.map((result, index) => (
                            <React.Fragment key={result.id}>
                              <ListItem
                                button
                                onClick={() => handleResultClick(result)}
                                sx={{ 
                                  py: 1,
                                  '&:hover': { bgcolor: 'action.hover' }
                                }}
                              >
                                <ListItemIcon sx={{ minWidth: 36 }}>
                                  <Box sx={{ color: getResultColor(result.type) }}>
                                    {getResultIcon(result.type)}
                                  </Box>
                                </ListItemIcon>
                                
                                <ListItemText
                                  primary={
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                        {result.title}
                                      </Typography>
                                      <Chip
                                        label={result.type}
                                        size="small"
                                        sx={{
                                          height: 18,
                                          fontSize: '0.7rem',
                                          bgcolor: getResultColor(result.type),
                                          color: 'white',
                                        }}
                                      />
                                    </Box>
                                  }
                                  secondary={
                                    <Box>
                                      {result.subtitle && (
                                        <Typography variant="caption" color="text.secondary">
                                          {result.subtitle}
                                        </Typography>
                                      )}
                                      {result.description && (
                                        <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                                          {result.description}
                                        </Typography>
                                      )}
                                    </Box>
                                  }
                                />
                              </ListItem>
                              {index < results.length - 1 && <Divider />}
                            </React.Fragment>
                          ))}
                        </List>
                      </>
                    ) : !isLoading ? (
                      <Box sx={{ p: 3, textAlign: 'center' }}>
                        <Typography variant="body2" color="text.secondary">
                          No results found for "{query}"
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Try different keywords or check spelling
                        </Typography>
                      </Box>
                    ) : null}
                  </>
                )}

                {/* Quick Actions */}
                {!query && (
                  <Box sx={{ p: 2 }}>
                    <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, mb: 1, display: 'block' }}>
                      Quick Actions
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      <Chip
                        label="New Books"
                        size="small"
                        variant="outlined"
                        onClick={() => handleSuggestionClick('new books')}
                        sx={{ cursor: 'pointer' }}
                      />
                      <Chip
                        label="Popular"
                        size="small"
                        variant="outlined"
                        onClick={() => handleSuggestionClick('popular')}
                        sx={{ cursor: 'pointer' }}
                      />
                      <Chip
                        label="Available"
                        size="small"
                        variant="outlined"
                        onClick={() => handleSuggestionClick('available')}
                        sx={{ cursor: 'pointer' }}
                      />
                    </Box>
                  </Box>
                )}
              </Paper>
            </Fade>
          )}
        </Popper>
      </Box>
    </ClickAwayListener>
  );
};

export default GlobalSearch;