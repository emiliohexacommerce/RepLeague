export class AuthModel {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  country?: string;
  bio?: string;
}
