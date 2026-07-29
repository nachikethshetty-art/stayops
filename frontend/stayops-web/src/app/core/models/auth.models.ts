export interface LoginRequest {
  userNameOrEmail: string;
  password: string;
}

export interface CurrentUser {
  id: string;
  userName: string;
  email: string;
  fullName: string;
  roles: string[];
  accessibleHotelIds: string[];
  isSuperAdmin: boolean;
}

export interface TokenResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  user: CurrentUser;
}

export const ROLES = {
  SuperAdmin: 'SuperAdmin',
  HotelManager: 'HotelManager',
  Receptionist: 'Receptionist',
  FinanceUser: 'FinanceUser',
  Housekeeper: 'Housekeeper',
  POSSystem: 'POSSystem'
} as const;
