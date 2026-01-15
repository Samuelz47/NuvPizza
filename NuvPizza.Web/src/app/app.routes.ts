import { Routes } from '@angular/router';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { SucessoComponent } from './features/sucesso/sucesso.component';
// 1. IMPORTANTE: Verifique se o caminho do import está correto
import { PainelPedidosComponent } from './features/admin/painel-pedidos/painel-pedidos.component'; 

export const routes: Routes = [
  // Redireciona a raiz para checkout
  { path: '', redirectTo: 'checkout', pathMatch: 'full' }, 

  // Rotas normais
  { path: 'checkout', component: CheckoutComponent },
  { path: 'sucesso', component: SucessoComponent },

  // 2. A ROTA DO PAINEL (Deve estar ANTES do '**')
  { path: 'admin/pedidos', component: PainelPedidosComponent },

  // Rota Curinga (Erro 404) - TEM QUE SER A ÚLTIMA
  { path: '**', redirectTo: '' } 
];