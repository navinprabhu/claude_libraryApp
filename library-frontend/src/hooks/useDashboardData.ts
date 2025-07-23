import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import apiClient from '../services/api';
import {
  DashboardStats,
  Transaction,
  BookCategory,
  TopMember,
  Alert,
} from '../types';

// Query keys for caching
export const dashboardKeys = {
  all: ['dashboard'] as const,
  stats: () => [...dashboardKeys.all, 'stats'] as const,
  transactions: (limit?: number) => [...dashboardKeys.all, 'transactions', limit] as const,
  categories: () => [...dashboardKeys.all, 'categories'] as const,
  topMembers: (limit?: number) => [...dashboardKeys.all, 'topMembers', limit] as const,
  alerts: () => [...dashboardKeys.all, 'alerts'] as const,
};

// Dashboard statistics hook
export const useDashboardStats = (options?: UseQueryOptions<DashboardStats>) => {
  return useQuery({
    queryKey: dashboardKeys.stats(),
    queryFn: () => apiClient.getDashboardStats(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
    refetchOnWindowFocus: false,
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
    ...options,
  });
};

// Recent transactions hook
export const useRecentTransactions = (
  limit: number = 10,
  options?: UseQueryOptions<Transaction[]>
) => {
  return useQuery({
    queryKey: dashboardKeys.transactions(limit),
    queryFn: () => apiClient.getRecentTransactions(limit),
    staleTime: 2 * 60 * 1000, // 2 minutes
    cacheTime: 5 * 60 * 1000, // 5 minutes
    refetchOnWindowFocus: false,
    retry: 3,
    ...options,
  });
};

// Book categories hook
export const useBookCategories = (options?: UseQueryOptions<BookCategory[]>) => {
  return useQuery({
    queryKey: dashboardKeys.categories(),
    queryFn: () => apiClient.getBookCategories(),
    staleTime: 10 * 60 * 1000, // 10 minutes
    cacheTime: 15 * 60 * 1000, // 15 minutes
    refetchOnWindowFocus: false,
    retry: 3,
    ...options,
  });
};

// Top members hook
export const useTopMembers = (
  limit: number = 5,
  options?: UseQueryOptions<TopMember[]>
) => {
  return useQuery({
    queryKey: dashboardKeys.topMembers(limit),
    queryFn: () => apiClient.getTopMembers(limit),
    staleTime: 15 * 60 * 1000, // 15 minutes
    cacheTime: 30 * 60 * 1000, // 30 minutes
    refetchOnWindowFocus: false,
    retry: 3,
    ...options,
  });
};

// Alerts hook
export const useAlerts = (options?: UseQueryOptions<Alert[]>) => {
  return useQuery({
    queryKey: dashboardKeys.alerts(),
    queryFn: () => apiClient.getAlerts(),
    staleTime: 1 * 60 * 1000, // 1 minute
    cacheTime: 2 * 60 * 1000, // 2 minutes
    refetchOnWindowFocus: true,
    refetchInterval: 2 * 60 * 1000, // Auto-refetch every 2 minutes
    retry: 3,
    ...options,
  });
};

// Combined dashboard data hook
export const useDashboardData = () => {
  const statsQuery = useDashboardStats();
  const transactionsQuery = useRecentTransactions(10);
  const categoriesQuery = useBookCategories();
  const membersQuery = useTopMembers(5);
  const alertsQuery = useAlerts();

  return {
    stats: {
      data: statsQuery.data,
      isLoading: statsQuery.isLoading,
      error: statsQuery.error,
      refetch: statsQuery.refetch,
    },
    transactions: {
      data: transactionsQuery.data || [],
      isLoading: transactionsQuery.isLoading,
      error: transactionsQuery.error,
      refetch: transactionsQuery.refetch,
    },
    categories: {
      data: categoriesQuery.data || [],
      isLoading: categoriesQuery.isLoading,
      error: categoriesQuery.error,
      refetch: categoriesQuery.refetch,
    },
    members: {
      data: membersQuery.data || [],
      isLoading: membersQuery.isLoading,
      error: membersQuery.error,
      refetch: membersQuery.refetch,
    },
    alerts: {
      data: alertsQuery.data || [],
      isLoading: alertsQuery.isLoading,
      error: alertsQuery.error,
      refetch: alertsQuery.refetch,
    },
    isLoading: 
      statsQuery.isLoading ||
      transactionsQuery.isLoading ||
      categoriesQuery.isLoading ||
      membersQuery.isLoading ||
      alertsQuery.isLoading,
    hasError: !!(
      statsQuery.error ||
      transactionsQuery.error ||
      categoriesQuery.error ||
      membersQuery.error ||
      alertsQuery.error
    ),
    refetchAll: () => {
      statsQuery.refetch();
      transactionsQuery.refetch();
      categoriesQuery.refetch();
      membersQuery.refetch();
      alertsQuery.refetch();
    },
  };
};