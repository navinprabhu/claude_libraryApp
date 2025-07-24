import axios, { AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import { 
  DashboardStats, 
  Transaction, 
  BookCategory, 
  TopMember, 
  Alert,
  LoginRequest,
  LoginResponse,
  ApiResponse,
  User
} from '../types';

// Temporarily bypass gateway - connect directly to services
const AUTH_SERVICE_URL = 'http://localhost:5001/api';
const BOOK_SERVICE_URL = 'http://localhost:5002/api';
const MEMBER_SERVICE_URL = 'http://localhost:5003/api';

class ApiClient {
  private authAxios: AxiosInstance;
  private bookAxios: AxiosInstance;
  private memberAxios: AxiosInstance;

  constructor() {
    // Create separate axios instances for each service
    this.authAxios = axios.create({
      baseURL: AUTH_SERVICE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.bookAxios = axios.create({
      baseURL: BOOK_SERVICE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.memberAxios = axios.create({
      baseURL: MEMBER_SERVICE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    const requestInterceptor = (config: InternalAxiosRequestConfig) => {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    };

    const responseInterceptor = (response: AxiosResponse) => response;
    const errorInterceptor = (error: any) => {
      if (error.response?.status === 401) {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
      return Promise.reject(error);
    };

    // Apply interceptors to all axios instances
    [this.authAxios, this.bookAxios, this.memberAxios].forEach(instance => {
      instance.interceptors.request.use(requestInterceptor, (error) => Promise.reject(error));
      instance.interceptors.response.use(responseInterceptor, errorInterceptor);
    });
  }

  // Authentication endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await this.authAxios.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  }

  async getProfile(): Promise<ApiResponse<User>> {
    const response = await this.authAxios.get<ApiResponse<User>>('/auth/profile');
    return response.data;
  }

  // Dashboard endpoints - temporarily return mock data since gateway is down
  async getDashboardStats(): Promise<DashboardStats> {
    // Mock data for dashboard stats
    return {
      totalBooks: 24,
      availableBooks: 18,
      booksBorrowed: 6,
      totalMembers: 15,
      activeMembers: 12,
      overdueBooks: 3,
      lastUpdated: new Date().toISOString()
    };
  }

  async getRecentTransactions(limit: number = 10): Promise<Transaction[]> {
    // Mock data for transactions
    return [
      {
        id: 1,
        memberId: 1,
        memberName: 'John Doe',
        bookId: 3,
        bookTitle: 'Sample Book',
        action: 'borrowed',
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString()
      },
      {
        id: 2,
        memberId: 2,
        memberName: 'Jane Smith',
        bookId: 1,
        bookTitle: 'Another Book',
        action: 'returned',
        timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString()
      }
    ].slice(0, limit);
  }

  async getBookCategories(): Promise<BookCategory[]> {
    // Mock data for book categories
    return [
      {
        category: 'Fiction',
        totalBooks: 10,
        availableBooks: 7,
        borrowedBooks: 3
      },
      {
        category: 'Non-Fiction',
        totalBooks: 8,
        availableBooks: 5,
        borrowedBooks: 3
      },
      {
        category: 'Science',
        totalBooks: 6,
        availableBooks: 4,
        borrowedBooks: 2
      }
    ];
  }

  async getTopMembers(limit: number = 5): Promise<TopMember[]> {
    // Mock data for top members
    return [
      {
        memberId: 1,
        memberName: 'Jane Smith',
        email: 'jane@example.com',
        totalBorrowings: 15,
        currentBorrowings: 2
      },
      {
        memberId: 2,
        memberName: 'John Doe',
        email: 'john@example.com',
        totalBorrowings: 12,
        currentBorrowings: 1
      }
    ].slice(0, limit);
  }

  async getAlerts(): Promise<Alert[]> {
    // Mock data for alerts
    return [
      {
        id: 1,
        type: 'overdue',
        message: '3 books are overdue',
        severity: 'warning',
        timestamp: new Date().toISOString()
      },
      {
        id: 2,
        type: 'system',
        message: 'Auth service is operational (bypassing gateway)',
        severity: 'info',
        timestamp: new Date().toISOString()
      }
    ];
  }
}

export const apiClient = new ApiClient();
export default apiClient;