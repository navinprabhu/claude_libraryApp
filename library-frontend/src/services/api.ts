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

const API_BASE_URL = 'http://localhost:5000/api';

class ApiClient {
  private axiosInstance: AxiosInstance;

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: API_BASE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    // Request interceptor - add auth token
    this.axiosInstance.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem('auth_token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - handle errors
    this.axiosInstance.interceptors.response.use(
      (response: AxiosResponse) => response,
      (error) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('auth_token');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Authentication endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await this.axiosInstance.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  }

  async getProfile(): Promise<ApiResponse<User>> {
    const response = await this.axiosInstance.get<ApiResponse<User>>('/auth/profile');
    return response.data;
  }

  // Dashboard endpoints
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.axiosInstance.get<DashboardStats>('/dashboard/stats');
    return response.data;
  }

  async getRecentTransactions(limit: number = 10): Promise<Transaction[]> {
    const response = await this.axiosInstance.get<Transaction[]>(`/dashboard/recent-transactions?limit=${limit}`);
    return response.data;
  }

  async getBookCategories(): Promise<BookCategory[]> {
    const response = await this.axiosInstance.get<BookCategory[]>('/dashboard/book-categories');
    return response.data;
  }

  async getTopMembers(limit: number = 5): Promise<TopMember[]> {
    const response = await this.axiosInstance.get<TopMember[]>(`/dashboard/top-members?limit=${limit}`);
    return response.data;
  }

  async getAlerts(): Promise<Alert[]> {
    const response = await this.axiosInstance.get<Alert[]>('/dashboard/alerts');
    return response.data;
  }
}

export const apiClient = new ApiClient();
export default apiClient;