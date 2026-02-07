import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';
import { ToastContainerComponent } from '../../shared/components/toast/toast-container.component';

@Component({
    selector: 'app-manager-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ToastContainerComponent],
    templateUrl: './manager-layout.component.html',
    styleUrls: ['./manager-layout.component.css']
})
export class ManagerLayoutComponent {
    currentUser = computed(() => this.authService.currentUserSignal());

    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    logout() {
        if (confirm('Are you sure you want to logout?')) {
            this.authService.logout();
        }
    }
}
