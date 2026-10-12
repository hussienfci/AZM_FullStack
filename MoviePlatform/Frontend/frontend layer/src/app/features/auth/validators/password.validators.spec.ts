import { FormControl, FormGroup } from '@angular/forms';

import { matchFieldsValidator, passwordStrengthValidator } from './password.validators';

describe('passwordStrengthValidator', () => {
  const validate = (value: string) => passwordStrengthValidator(new FormControl(value));

  it('ignores empty values (left to Validators.required)', () => {
    expect(validate('')).toBeNull();
  });

  it('lists every unmet rule', () => {
    expect(validate('abc')).toEqual({
      passwordStrength: { minLength: true, uppercase: true, number: true, special: true },
    });
  });

  it('accepts a password meeting all rules', () => {
    expect(validate('Popcorn#2026')).toBeNull();
  });
});

describe('matchFieldsValidator', () => {
  const buildForm = (password: string, confirm: string) =>
    new FormGroup(
      { password: new FormControl(password), confirm: new FormControl(confirm) },
      { validators: matchFieldsValidator('password', 'confirm') },
    );

  it('flags mismatching values on the group and the confirm control', () => {
    const form = buildForm('Secret#123', 'Secret#124');
    expect(form.hasError('passwordMismatch')).toBe(true);
    expect(form.controls.confirm.hasError('passwordMismatch')).toBe(true);
  });

  it('clears the error once the values match', () => {
    const form = buildForm('Secret#123', 'Secret#124');
    form.controls.confirm.setValue('Secret#123');
    expect(form.valid).toBe(true);
    expect(form.controls.confirm.errors).toBeNull();
  });
});
