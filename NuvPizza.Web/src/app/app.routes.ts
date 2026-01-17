import { Routes } from '@angular/router';
import { Router } from '@angular/router'; // Importe Router
import { inject } from '@angular/core'; // Importe inject
import { CheckoutComponent } from './features/checkout/checkout.component';
import { SucessoComponent } from './features/sucesso/sucesso.component';
import { PainelPedidosComponent } from './features/admin/painel-pedidos/painel-pedidos.component';
import { LoginComponent } from './features/admin/login/login.component'; // Importe
import { AuthService } from './core/services/auth.service'; // Importe

// Guard funcional simples (embutido)
const authGuard = () => {
  const authService = inject(AuthService);
  if (authService.isAuthenticated()) {
    return true;
  }
  const router = inject(Router); // Precisa importar Router também lá em cima se der erro
  router.navigate(['/login']);
  return false;
};

export const routes: Routes = [
  { path: '', redirectTo: 'checkout', pathMatch: 'full' }, 
  { path: 'checkout', component: CheckoutComponent },
  { path: 'sucesso', component: SucessoComponent },
  
  // Nova rota de Login
  { path: 'login', component: LoginComponent },

  // Rota Protegida
  { 
    path: 'admin/pedidos', 
    component: PainelPedidosComponent,
    canActivate: [authGuard] // <--- AQUI ESTÁ O CADEADO
  },

  { path: '**', redirectTo: '' }
];