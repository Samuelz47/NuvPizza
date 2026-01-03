import { Component } from '@angular/core';
import { CheckoutComponent } from './features/checkout/checkout.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CheckoutComponent], // Importando o Checkout aqui
  template: `<app-checkout></app-checkout>`
})
export class AppComponent {}