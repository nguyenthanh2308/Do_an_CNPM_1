import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinnerComponent],
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent {
    registerForm: FormGroup;
    loading = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private toastService: ToastService,
        private router: Router
    ) {
        // Redirect if already logged in
        if (this.authService.isAuthenticated()) {
            this.router.navigate(['/manager/dashboard']);
        }

        this.registerForm = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', [Validators.required]],
            fullName: ['', [Validators.required]],
            phone: ['', [Validators.pattern(/^\d{10,15}$/)]]
        }, {
            validators: this.passwordMatchValidator
        });
    }

    // Custom validator to check if passwords match
    passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
        const password = control.get('password');
        const confirmPassword = control.get('confirmPassword');

        if (!password || !confirmPassword) {
            return null;
        }

        return password.value === confirmPassword.value ? null : { passwordMismatch: true };
    }

    get username() {
        return this.registerForm.get('username');
    }

    get email() {
        return this.registerForm.get('email');
    }

    get password() {
        return this.registerForm.get('password');
    }

    get confirmPassword() {
        return this.registerForm.get('confirmPassword');
    }

    get fullName() {
        return this.registerForm.get('fullName');
    }

    get phone() {
        return this.registerForm.get('phone');
    }

    onSubmit() {
        if (this.registerForm.valid) {
            this.loading = true;

            // Remove confirmPassword before sending to API
            const { confirmPassword, ...registerData } = this.registerForm.value;

            this.authService.register(registerData).subscribe({
                next: (response) => {
                    this.loading = false;
                    this.toastService.success('Registration successful!');

                    // Auto-navigate based on role
                    const user = this.authService.getCurrentUser();
                    if (user?.role === 'Manager' || user?.role === 'Admin') {
                        this.router.navigate(['/manager/dashboard']);
                    } else if (user?.role === 'Customer') {
                        this.router.navigate(['/customer/home']);
                    } else {
                        this.router.navigate(['/']);
                    }
                },
                error: (err) => {
                    this.loading = false;
                    const errorMessage = err.error?.message || 'Registration failed. Please try again.';
                    this.toastService.error(errorMessage);
                }
            });
        } else {
            // Mark all fields as touched to show validation errors
            Object.keys(this.registerForm.controls).forEach(key => {
                this.registerForm.get(key)?.markAsTouched();
            });
        }
    }
}
