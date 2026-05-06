export interface AuthTokenInfo {
  accessToken: string;
  expiresAt: string;
  expiresInSeconds: number;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}
