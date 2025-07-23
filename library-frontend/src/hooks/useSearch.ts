import { useState, useCallback, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../services/api';

export interface SearchResult {
  id: string;
  type: 'book' | 'member' | 'transaction';
  title: string;
  subtitle?: string;
  description?: string;
  url: string;
  metadata?: Record<string, any>;
}

interface SearchHookResult {
  query: string;
  setQuery: (query: string) => void;
  results: SearchResult[];
  isLoading: boolean;
  isError: boolean;
  error: any;
  clearSearch: () => void;
}

// Mock search function - in production, this would be a real API call
const performSearch = async (query: string): Promise<SearchResult[]> => {
  if (!query.trim()) return [];
  
  // Simulate API delay
  await new Promise(resolve => setTimeout(resolve, 300));
  
  // Mock search results
  const mockResults: SearchResult[] = [
    {
      id: '1',
      type: 'book',
      title: 'The Great Gatsby',
      subtitle: 'by F. Scott Fitzgerald',
      description: 'Classic American literature',
      url: '/books/1',
      metadata: { isbn: '978-0-7432-7356-5', status: 'available' }
    },
    {
      id: '2',
      type: 'book',
      title: 'To Kill a Mockingbird',
      subtitle: 'by Harper Lee',
      description: 'Pulitzer Prize winner',
      url: '/books/2',
      metadata: { isbn: '978-0-06-112008-4', status: 'borrowed' }
    },
    {
      id: '3',
      type: 'member',
      title: 'John Smith',
      subtitle: 'Member since 2022',
      description: '15 books borrowed this year',
      url: '/members/3',
      metadata: { email: 'john@example.com', status: 'active' }
    },
    {
      id: '4',
      type: 'transaction',
      title: 'Book Return',
      subtitle: 'The Great Gatsby returned by John Smith',
      description: 'Completed 2 hours ago',
      url: '/transactions/4',
      metadata: { status: 'completed', date: new Date().toISOString() }
    }
  ];

  // Filter results based on query
  return mockResults.filter(result => 
    result.title.toLowerCase().includes(query.toLowerCase()) ||
    result.subtitle?.toLowerCase().includes(query.toLowerCase()) ||
    result.description?.toLowerCase().includes(query.toLowerCase())
  );
};

export const useSearch = (): SearchHookResult => {
  const [query, setQuery] = useState('');

  const {
    data: results = [],
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['search', query],
    queryFn: () => performSearch(query),
    enabled: query.trim().length >= 2, // Only search if query is 2+ characters
    staleTime: 30 * 1000, // 30 seconds
    cacheTime: 5 * 60 * 1000, // 5 minutes
  });

  const clearSearch = useCallback(() => {
    setQuery('');
  }, []);

  return {
    query,
    setQuery,
    results,
    isLoading,
    isError,
    error,
    clearSearch,
  };
};

// Hook for quick search suggestions
export const useSearchSuggestions = (query: string) => {
  return useMemo(() => {
    if (!query.trim()) return [];
    
    const suggestions = [
      'Harry Potter',
      'Jane Austen',
      'Science Fiction',
      'History Books',
      'Recently Added',
      'Popular Authors',
    ];

    return suggestions.filter(suggestion =>
      suggestion.toLowerCase().includes(query.toLowerCase())
    ).slice(0, 5);
  }, [query]);
};