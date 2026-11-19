import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login-page/login-page.component')
      .then(({ LoginPageComponent }) => LoginPageComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layouts/main-layout/main-layout.component')
      .then(({ MainLayoutComponent }) => MainLayoutComponent),
    children: []
  },
  { path: '**', redirectTo: '' }
];
