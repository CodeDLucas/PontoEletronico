import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, NotificationService } from '../../../services';
import { RegisterRequest } from '../../../models';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;

  // Password requirements checker
  passwordRequirements = {
    minLength: false,
    hasLowerCase: false,
    hasUpperCase: false,
    hasNumber: false
  };

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.createForm();
  }

  private createForm(): void {
    this.registerForm = this.formBuilder.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      employeeCode: [''],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    // Listen to password changes to update requirements checker
    this.registerForm.get('password')?.valueChanges.subscribe(password => {
      this.updatePasswordRequirements(password || '');
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
    } else {
      if (confirmPassword?.hasError('passwordMismatch')) {
        const errors = { ...confirmPassword.errors };
        delete errors['passwordMismatch'];
        confirmPassword.setErrors(Object.keys(errors).length ? errors : null);
      }
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid && !this.isLoading) {
      this.isLoading = true;
      const registerData: RegisterRequest = {
        fullName: this.registerForm.value.fullName,
        email: this.registerForm.value.email,
        employeeCode: this.registerForm.value.employeeCode,
        password: this.registerForm.value.password,
        confirmPassword: this.registerForm.value.confirmPassword
      };

      this.authService.register(registerData).subscribe({
        next: (response) => {
          if (this.notificationService.handleApiResult(
            response,
            'Conta criada com sucesso! Faça login para continuar.',
            'Erro ao criar conta. Tente novamente.'
          )) {
            this.router.navigate(['/login']);
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Erro no registro:', error);
          this.notificationService.handleHttpError(error, 'Erro ao criar conta. Tente novamente.');
          this.isLoading = false;
        }
      });
    }
  }

  getFullNameErrorMessage(): string {
    const control = this.registerForm.get('fullName');
    if (control?.hasError('required')) return 'Nome completo é obrigatório';
    if (control?.hasError('minlength')) return 'Nome deve ter pelo menos 2 caracteres';
    return '';
  }

  getEmployeeCodeErrorMessage(): string {
    const control = this.registerForm.get('employeeCode');
    if (control?.hasError('minlength')) return 'Código deve ter pelo menos 2 caracteres';
    return '';
  }

  getEmailErrorMessage(): string {
    const control = this.registerForm.get('email');
    if (control?.hasError('required')) return 'Email é obrigatório';
    if (control?.hasError('email')) return 'Email deve ter um formato válido';
    return '';
  }

  getPasswordErrorMessage(): string {
    const control = this.registerForm.get('password');
    if (control?.hasError('required')) return 'Senha é obrigatória';
    if (control?.hasError('minlength')) return 'Senha deve ter pelo menos 6 caracteres';
    return '';
  }

  getConfirmPasswordErrorMessage(): string {
    const control = this.registerForm.get('confirmPassword');
    if (control?.hasError('required')) return 'Confirmação de senha é obrigatória';
    if (control?.hasError('passwordMismatch')) return 'Senhas não coincidem';
    return '';
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  updatePasswordRequirements(password: string): void {
    this.passwordRequirements = {
      minLength: password.length >= 6,
      hasLowerCase: /[a-z]/.test(password),
      hasUpperCase: /[A-Z]/.test(password),
      hasNumber: /\d/.test(password)
    };
  }

  areAllPasswordRequirementsMet(): boolean {
    return Object.values(this.passwordRequirements).every(req => req);
  }

}
