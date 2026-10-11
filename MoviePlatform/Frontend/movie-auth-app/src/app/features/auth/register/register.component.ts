import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { AuthLayoutComponent } from '../auth-layout/auth-layout.component';
import {
  matchFieldsValidator,
  PASSWORD_RULES,
  passwordStrengthValidator,
} from '../validators/password.validators';

const STRENGTH_LEVELS = [
  { label: 'Too weak', bar: 'bg-red-600', text: 'text-red-500' },
  { label: 'Weak', bar: 'bg-red-600', text: 'text-red-500' },
  { label: 'Fair', bar: 'bg-amber-500', text: 'text-amber-400' },
  { label: 'Good', bar: 'bg-lime-500', text: 'text-lime-400' },
  { label: 'Strong', bar: 'bg-emerald-500', text: 'text-emerald-400' },
] as const;

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, AuthLayoutComponent],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group(
    {
      fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, passwordStrengthValidator]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]],
    },
    { validators: matchFieldsValidator('password', 'confirmPassword') },
  );

  readonly showPassword = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // ── Real-time password feedback, derived from the form via signals ──
  private readonly password = toSignal(this.form.controls.password.valueChanges, {
    initialValue: '',
  });
  private readonly confirmPassword = toSignal(this.form.controls.confirmPassword.valueChanges, {
    initialValue: '',
  });

  readonly passwordRules = computed(() =>
    PASSWORD_RULES.map((rule) => ({ label: rule.label, met: rule.test(this.password()) })),
  );
  readonly strength = computed(() => {
    const score = this.passwordRules().filter((rule) => rule.met).length;
    return { score, ...STRENGTH_LEVELS[score] };
  });
  readonly passwordsMatch = computed(
    () => !!this.confirmPassword() && this.password() === this.confirmPassword(),
  );
  readonly passwordsMismatch = computed(
    () => !!this.confirmPassword() && this.password() !== this.confirmPassword(),
  );

  togglePassword(): void {
    this.showPassword.update((visible) => !visible);
  }

  fieldError(name: 'fullName' | 'email' | 'password' | 'confirmPassword' | 'acceptTerms'): string | null {
    const control = this.form.controls[name];
    if (!control.touched || !control.errors) return null;

    switch (name) {
      case 'fullName':
        if (control.hasError('required')) return 'Please enter your full name.';
        if (control.hasError('minlength')) return 'Your name must be at least 2 characters.';
        return 'Your name must be at most 100 characters.';
      case 'email':
        return control.hasError('required')
          ? 'Please enter your email address.'
          : 'Please enter a valid email address.';
      case 'password':
        return control.hasError('required')
          ? 'Please create a password.'
          : 'Your password does not meet all the requirements below.';
      case 'confirmPassword':
        return control.hasError('required')
          ? 'Please confirm your password.'
          : 'Passwords do not match.';
      case 'acceptTerms':
        return 'You must accept the Terms & Conditions to continue.';
    }
  }

  submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { fullName, email, password } = this.form.getRawValue();
    this.isSubmitting.set(true);
    this.auth.register({ fullName, email, password }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigateByUrl('/browse');
      },
      error: (err: Error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }
}
