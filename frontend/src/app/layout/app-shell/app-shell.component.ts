import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { MobileNavigationComponent } from '../mobile-navigation/mobile-navigation.component';
import { ToastContainerComponent, DemoJourneyBarComponent } from '../../shared/components';
import { AuthService } from '../../core/services';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, TopbarComponent, MobileNavigationComponent, ToastContainerComponent, DemoJourneyBarComponent],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.css']
})
export class AppShellComponent {
  authService = inject(AuthService);
}
