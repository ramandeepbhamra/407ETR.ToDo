import { AuthTokenInfo } from './auth-token-info.model';
import { AuthUserInfo } from './auth-user-info.model';

export interface AuthLoginResponse {
  token: AuthTokenInfo;
  user: AuthUserInfo;
}
