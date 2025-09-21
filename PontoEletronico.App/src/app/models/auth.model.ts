export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  employeeCode: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  token: string;
  expiration: string;
  user: UserProfile;
}

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  employeeCode: string;
  createdAt: string;
  isActive: boolean;
}

export interface RefreshTokenRequest {
  token: string;
}