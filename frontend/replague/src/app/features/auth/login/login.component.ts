import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center">
      <div class="w-full max-w-md p-8 bg-gray-900 rounded-2xl shadow-xl">
        <h1 class="text-3xl font-bold text-center mb-8 text-primary-400">RepLeague</h1>
        <p class="text-center text-gray-400">Login form — coming soon</p>
        <p class="text-center mt-4">
          <a routerLink="/auth/register" class="text-primary-400 hover:underline">Create account</a>
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {}
