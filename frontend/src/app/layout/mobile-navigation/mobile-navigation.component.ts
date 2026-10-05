import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services';

@Component({
  selector: 'app-mobile-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mobile-navigation.component.html',
  styleUrls: ['./mobile-navigation.component.css']
})
export class MobileNavigationComponent {
  authService = inject(AuthService);
}
