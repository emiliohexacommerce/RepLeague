import { Routes } from '@angular/router';

export const WORKOUTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./workout-list/workout-list.component').then(m => m.WorkoutListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./workout-form/workout-form.component').then(m => m.WorkoutFormComponent)
  }
];
