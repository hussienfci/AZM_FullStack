/** Mirrors UserManagementApi.Models.DTOs.UserDto */
export interface AuthUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string | null;
  dateOfBirth?: string | null;
  isActive: boolean;
  role: string;
  createdAt: string;
}

/** Mirrors UserManagementApi.Models.DTOs.LoginResponseDto */
export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string | null;
  expiresAt?: string | null;
  user?: AuthUser | null;
}

export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterPayload {
  fullName: string;
  email: string;
  password: string;
}
