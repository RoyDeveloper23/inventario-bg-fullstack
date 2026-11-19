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
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'productos'
      },
      {
        path: 'productos',
        loadComponent: () => import('./features/productos/product-list/product-list.component')
          .then(({ ProductListComponent }) => ProductListComponent)
      },
      {
        path: 'productos/nuevo',
        loadComponent: () => import('./features/productos/product-form/product-form.component')
          .then(({ ProductFormComponent }) => ProductFormComponent)
      },
      {
        path: 'productos/:id/editar',
        loadComponent: () => import('./features/productos/product-form/product-form.component')
          .then(({ ProductFormComponent }) => ProductFormComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
