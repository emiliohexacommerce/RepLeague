import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-verify-email',
  templateUrl: './verify-email.component.html',
  styleUrls: ['./verify-email.component.scss'],
})
export class VerifyEmailComponent implements OnInit {
  state: 'loading' | 'success' | 'error' | 'noToken' = 'loading';
  errorMessage = '';

  constructor(private route: ActivatedRoute, private authService: AuthService) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.state = 'noToken';
      return;
    }
    this.authService.verifyEmail(token).subscribe({
      next: () => (this.state = 'success'),
      error: (err) => {
        this.errorMessage = err?.error?.message ?? 'El enlace no es válido o ha expirado.';
        this.state = 'error';
      },
    });
  }
}
