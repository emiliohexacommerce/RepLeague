export class UserModel {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string;
  country: string;
  bio: string;
  // Metronic layout compat fields
  get pic(): string { return this.avatarUrl || './assets/media/avatars/blank.png'; }
  get fullname(): string { return this.displayName; }

  setUser(user: Partial<UserModel>) {
    this.id = user.id || '';
    this.email = user.email || '';
    this.displayName = user.displayName || '';
    this.avatarUrl = user.avatarUrl || '';
    this.country = user.country || '';
    this.bio = user.bio || '';
  }
}
