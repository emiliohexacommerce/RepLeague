import { Routes } from '@angular/router';

const Routing: Routes = [
  {
    path: 'dashboard',
    loadChildren: () => import('./dashboard/dashboard.module').then((m) => m.DashboardModule),
  },
  {
    path: 'workouts',
    loadChildren: () => import('./workouts/workouts.module').then((m) => m.WorkoutsModule),
  },
  {
    path: 'leagues',
    loadChildren: () => import('./leagues/leagues.module').then((m) => m.LeaguesModule),
  },
  {
    path: 'profile',
    loadChildren: () => import('./profile/profile-page.module').then((m) => m.ProfilePageModule),
  },
  {
    path: 'wod',
    loadChildren: () => import('./wod/wod.module').then((m) => m.WodModule),
  },
  {
    path: 'strength',
    loadChildren: () => import('./strength/strength.module').then((m) => m.StrengthModule),
  },
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },
  {
    path: '**',
    redirectTo: 'error/404',
  },
];

export { Routing };
