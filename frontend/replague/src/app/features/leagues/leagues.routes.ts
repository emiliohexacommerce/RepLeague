import { Routes } from '@angular/router';

export const LEAGUES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./league-list/league-list.component').then(m => m.LeagueListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./league-form/league-form.component').then(m => m.LeagueFormComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./league-detail/league-detail.component').then(m => m.LeagueDetailComponent)
  },
  {
    path: 'join/:token',
    loadComponent: () => import('./join-league/join-league.component').then(m => m.JoinLeagueComponent)
  }
];
