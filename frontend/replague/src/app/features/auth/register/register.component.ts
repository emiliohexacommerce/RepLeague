import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center">
      <div class="w-full max-w-md p-8 bg-gray-900 rounded-2xl shadow-xl">
        <h1 class="text-3xl font-bold text-center mb-8 text-primary-400">Create Account</h1>
        <p class="text-center text-gray-400">Register form — coming soon</p>
        <p class="text-center mt-4">
          <a routerLink="/auth/login" class="text-primary-400 hover:underline">Already have an account?</a>
        </p>
      </div>
    </div>
  `
})
export class RegisterComponent {}
