import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { first } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const pw = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pw === confirm ? null : { passwordsMismatch: true };
}

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
  form: FormGroup;
  isLoading$: Observable<boolean>;
  token: string | null = null;
  state: 'form' | 'success' | 'error' | 'invalidToken' = 'form';
  errorMessage = '';

  private subs: Subscription[] = [];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.isLoading$ = this.authService.isLoading$;
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token');
    if (!this.token) {
      this.state = 'invalidToken';
      return;
    }
    this.form = this.fb.group(
      {
        newPassword: ['', Validators.compose([Validators.required, Validators.minLength(6)])],
        confirmPassword: ['', Validators.required],
      },
      { validators: passwordsMatch }
    );
  }

  get f() {
    return this.form?.controls ?? {};
  }

  submit() {
    if (!this.form || this.form.invalid || !this.token) return;
    const sub = this.authService
      .resetPassword(this.token, this.f['newPassword'].value)
      .pipe(first())
      .subscribe({
        next: () => (this.state = 'success'),
        error: (err) => {
          this.errorMessage =
            err?.error?.message ?? 'El enlace no es válido o ha expirado.';
          this.state = 'error';
        },
      });
    this.subs.push(sub);
  }

  ngOnDestroy(): void {
    this.subs.forEach((s) => s.unsubscribe());
  }
}
