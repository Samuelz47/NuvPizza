import { Routes } from '@angular/router';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { SucessoComponent } from './features/sucesso/sucesso.component';

export const routes: Routes = [
  { path: '', component: CheckoutComponent }, // Home é o checkout por enquanto
  { path: 'sucesso', component: SucessoComponent } // Nova rota
];