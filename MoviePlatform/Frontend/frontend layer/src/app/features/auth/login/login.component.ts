import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { AuthLayoutComponent } from '../auth-layout/auth-layout.component';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, AuthLayoutComponent],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberMe: [false],
  });

  readonly showPassword = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly infoMessage = signal<string | null>(null);

  togglePassword(): void {
    this.showPassword.update((visible) => !visible);
  }

  fieldError(name: 'email' | 'password'): string | null {
    const control = this.form.controls[name];
    if (!control.touched || !control.errors) return null;
    if (control.hasError('required')) {
      return name === 'email' ? 'Please enter your email address.' : 'Please enter your password.';
    }
    if (control.hasError('email')) return 'Please enter a valid email address.';
    if (control.hasError('minlength')) return 'Your password must contain at least 6 characters.';
    return null;
  }

  socialLogin(provider: 'Google' | 'Apple'): void {
    // The UserManagementApi has no OAuth endpoints yet; wire these up once it does.
    this.errorMessage.set(null);
    this.infoMessage.set(`Sign in with ${provider} is coming soon.`);
  }

  submit(): void {
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigateByUrl(this.safeReturnUrl());
      },
      error: (err: Error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }

  /** Only allow in-app paths, so ?returnUrl=https://evil.example can't redirect off-site. */
  private safeReturnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//')
      ? returnUrl
      : '/browse';
  }
}
