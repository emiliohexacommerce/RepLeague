export const environment = {
  production: true,
  appVersion: 'v1.0.0',
  USERDATA_KEY: 'replague_auth',
  isMockEnabled: false,
  apiUrl: 'http://api-repleague.azurewebsites.net/api',
  appThemeName: 'RepLeague',
  appPurchaseUrl: '',
  appPreviewUrl: '',
  appPreviewChangelogUrl: '',
  appDemos: {} as Record<string, { title: string; thumbnail: string; published: boolean }>,
  // PWA / Push — Sustituir con claves reales generadas con: npx web-push generate-vapid-keys
  vapidPublicKey: 'REPLACE_WITH_YOUR_VAPID_PUBLIC_KEY',
};
