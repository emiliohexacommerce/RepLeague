import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex flex-col items-center justify-center gap-6">
      <h1 class="text-4xl font-bold text-primary-400">RepLeague</h1>
      <p class="text-gray-400">Dashboard coming soon</p>
      <div class="flex gap-4">
        <a routerLink="/workouts/new" class="btn-primary">Log Workout</a>
        <a routerLink="/leagues/new" class="btn-secondary">Create League</a>
      </div>
    </div>
  `
})
export class HomeComponent {}
