export const environment = {
  production: false,
  appVersion: 'v1.0.0',
  USERDATA_KEY: 'replague_auth',
  isMockEnabled: false,
  apiUrl: 'http://localhost:5000/api',
  appThemeName: 'RepLeague',
  appPurchaseUrl: '',
  appPreviewUrl: '',
  appPreviewChangelogUrl: '',
  appDemos: {} as Record<string, { title: string; thumbnail: string; published: boolean }>,
  // PWA / Push (dev: SW disabled, keys unused)
  vapidPublicKey: '',
};
