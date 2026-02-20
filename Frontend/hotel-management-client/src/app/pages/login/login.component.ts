import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinnerComponent],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent {
    loginForm: FormGroup;
    loading = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private toastService: ToastService,
        private router: Router,
        private route: ActivatedRoute
    ) {
        // Redirect if already logged in
        if (this.authService.isAuthenticated()) {
            this.router.navigate(['/manager/dashboard']);
        }

        this.loginForm = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            password: ['', [Validators.required, Validators.minLength(6)]]
        });
    }

    get username() {
        return this.loginForm.get('username');
    }

    get password() {
        return this.loginForm.get('password');
    }

    onSubmit() {
        if (this.loginForm.valid) {
            this.loading = true;
            this.authService.login(this.loginForm.value).subscribe({
                next: (response) => {
                    this.loading = false;
                    this.toastService.success('Login successful!');

                    // Get return URL from query params or default to dashboard
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
                    const user = this.authService.getCurrentUser();

                    // Navigate based on role
                    if (returnUrl) {
                        this.router.navigateByUrl(returnUrl);
                    } else if (user?.role === 'Manager' || user?.role === 'Admin') {
                        this.router.navigate(['/manager/dashboard']);
                    } else if (user?.role === 'Receptionist') {
                        // TODO: receptionist module not yet implemented
                        this.router.navigate(['/manager/dashboard']);
                    } else if (user?.role === 'Housekeeping') {
                        // TODO: housekeeping module not yet implemented
                        this.router.navigate(['/manager/dashboard']);
                    } else {
                        // Customer or unknown role
                        this.toastService.error(`The "${user?.role}" portal is not available yet. Please contact an administrator.`);
                        this.authService.logout();
                    }
                },
                error: (err) => {
                    this.loading = false;
                    const errorMessage = err.error?.message || 'Invalid username or password';
                    this.toastService.error(errorMessage);
                }
            });
        } else {
            // Mark all fields as touched to show validation errors
            Object.keys(this.loginForm.controls).forEach(key => {
                this.loginForm.get(key)?.markAsTouched();
            });
        }
    }
}
