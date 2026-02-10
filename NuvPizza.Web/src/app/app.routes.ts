import { Routes } from '@angular/router';
import { Router } from '@angular/router'; 
import { inject } from '@angular/core'; 
import { CheckoutComponent } from './features/checkout/checkout.component';
import { SucessoComponent } from './features/sucesso/sucesso.component';
import { PainelPedidosComponent } from './features/admin/painel-pedidos/painel-pedidos.component';
import { LoginComponent } from './features/admin/login/login.component'; 
import { AuthService } from './core/services/auth.service'; 
import { AcompanharPedidoComponent } from './features/admin/acompanhar-pedido/acompanhar-pedido.component'; 

// Guard funcional
const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }
  
  // Se não estiver logado, manda pro login
  return router.createUrlTree(['/login']);
};

export const routes: Routes = [
  { path: '', redirectTo: 'checkout', pathMatch: 'full' }, 
  { path: 'checkout', component: CheckoutComponent },
  { path: 'sucesso', component: SucessoComponent }, // Agora o componente está Standalone e vai funcionar
  { path: 'login', component: LoginComponent },
  { path: 'acompanhar/:id', component: AcompanharPedidoComponent },
  
  { 
    path: 'admin/pedidos', 
    component: PainelPedidosComponent,
    canActivate: [authGuard] 
  },

  { path: '**', redirectTo: '' }
];