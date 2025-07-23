export interface LoginRequest {
  username: string;
  password: string;
}

export interface User {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface LoginResponse {
  success: boolean;
  data: {
    token: string;
    user: User;
    expiresIn: number;
  };
  message?: string;
  errors?: string[];
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}