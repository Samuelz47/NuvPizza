import { Routes } from '@angular/router';
import { Router } from '@angular/router'; 
import { inject } from '@angular/core'; 

// Componentes Públicos
import { CardapioComponent } from './features/cardapio/cardapio.component';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { SucessoComponent } from './features/sucesso/sucesso.component';
import { LoginComponent } from './features/admin/login/login.component'; 
import { AcompanharPedidoComponent } from './features/admin/acompanhar-pedido/acompanhar-pedido.component'; 

// Componentes Admin
import { PainelPedidosComponent } from './features/admin/painel-pedidos/painel-pedidos.component';
import { AdminHomeComponent } from './features/admin/admin-home/admin-home.component';
import { GerenciarProdutosComponent } from './features/admin/gerenciar-produtos/gerenciar-produtos.component';

import { AuthService } from './core/services/auth.service'; 

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
  // Rota raiz agora aponta para o Cardápio (Home do Cliente)
  { path: '', component: CardapioComponent }, 

  { path: 'checkout', component: CheckoutComponent },
  { path: 'sucesso', component: SucessoComponent },
  { path: 'acompanhar/:id', component: AcompanharPedidoComponent },
  
  // Área Administrativa
  { path: 'login', component: LoginComponent },
  { 
    path: 'admin', 
    canActivate: [authGuard], // Protege todas as rotas filhas
    children: [
      { path: 'home', component: AdminHomeComponent }, // Escolha: Pedidos ou Produtos
      { path: 'painel', component: PainelPedidosComponent },
      { path: 'produtos', component: GerenciarProdutosComponent },
      // Se entrar em /admin sem nada, vai pra home do admin
      { path: '', redirectTo: 'home', pathMatch: 'full' }
    ]
  },

  // Rota coringa (404) volta pro cardápio
  { path: '**', redirectTo: '' }
];