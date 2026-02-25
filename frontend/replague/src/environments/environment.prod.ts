export const environment = {
  production: true,
  appVersion: 'v1.0.0',
  USERDATA_KEY: 'replague_auth',
  isMockEnabled: false,
  apiUrl: 'https://YOUR_AZURE_APP_SERVICE.azurewebsites.net/api',
  appThemeName: 'RepLeague',
  appPurchaseUrl: '',
  appPreviewUrl: '',
  appPreviewChangelogUrl: '',
  appDemos: {} as Record<string, { title: string; thumbnail: string; published: boolean }>,
};
