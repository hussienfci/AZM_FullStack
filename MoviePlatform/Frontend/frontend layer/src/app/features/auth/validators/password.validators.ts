import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export interface PasswordRule {
  key: 'minLength' | 'uppercase' | 'number' | 'special';
  label: string;
  test: (value: string) => boolean;
}

export const PASSWORD_RULES: readonly PasswordRule[] = [
  { key: 'minLength', label: 'At least 8 characters', test: (v) => v.length >= 8 },
  { key: 'uppercase', label: 'One uppercase letter', test: (v) => /[A-Z]/.test(v) },
  { key: 'number', label: 'One number', test: (v) => /\d/.test(v) },
  { key: 'special', label: 'One special character', test: (v) => /[^A-Za-z0-9]/.test(v) },
];

/** Fails with `{ passwordStrength: { minLength: true, ... } }` listing every unmet rule. */
export const passwordStrengthValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value: string = control.value ?? '';
  if (!value) return null; // let Validators.required report empty values

  const failed = PASSWORD_RULES.filter((rule) => !rule.test(value));
  return failed.length
    ? { passwordStrength: Object.fromEntries(failed.map((rule) => [rule.key, true])) }
    : null;
};

/**
 * Group-level validator: sets `{ passwordMismatch: true }` on the group and on the
 * confirm control, so the error can be shown right under the confirm field.
 */
export function matchFieldsValidator(field: string, confirmField: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const control = group.get(field);
    const confirm = group.get(confirmField);
    if (!control || !confirm) return null;

    const mismatch = !!confirm.value && control.value !== confirm.value;
    const { passwordMismatch: _, ...otherErrors } = confirm.errors ?? {};
    if (mismatch) {
      confirm.setErrors({ ...otherErrors, passwordMismatch: true });
    } else {
      confirm.setErrors(Object.keys(otherErrors).length ? otherErrors : null);
    }
    return mismatch ? { passwordMismatch: true } : null;
  };
}
