import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PaymentService } from '../../core/services/payment.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="checkout-container">
      <h2>Finalizar Pedido</h2>
      
      <div class="summary">
        <p>Total a pagar: <strong>{{ amount() | currency:'BRL' }}</strong></p>
        
        <div class="info-box">
           <p>Para sua segurança, o pagamento será realizado na página oficial do Mercado Pago.</p>
           <p class="methods-label">Aceitamos:</p>
           <div class="methods">
              <span class="badge">💳 Cartão</span>
              <span class="badge">💠 Pix</span>
              <span class="badge">🍎 Apple Pay</span>
              <span class="badge">🤖 Google Pay</span>
           </div>
        </div>
      </div>
      
      <button (click)="irParaPagamento()" [disabled]="loading()" class="btn-pagar">
        @if (loading()) {
          <span>Gerando link seguro...</span>
        } @else {
          Pagar Agora ➔
        }
      </button>

      @if (errorMessage()) {
        <div class="error">{{ errorMessage() }}</div>
      }
    </div>
  `,
  styles: [`
    .checkout-container { max-width: 500px; margin: 2rem auto; padding: 2rem; background: #fff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); text-align: center; font-family: sans-serif; }
    .summary { margin-bottom: 2rem; }
    .info-box { background: #f8f9fa; padding: 15px; border-radius: 8px; margin-top: 15px; border: 1px solid #e9ecef; }
    .methods-label { font-size: 0.85rem; color: #666; margin-bottom: 5px; margin-top: 10px; text-transform: uppercase; letter-spacing: 0.5px; }
    .methods { display: flex; justify-content: center; gap: 8px; flex-wrap: wrap; }
    .badge { background: #fff; border: 1px solid #ddd; padding: 4px 10px; border-radius: 15px; font-size: 0.85rem; color: #555; }
    .btn-pagar { background: #009ee3; color: white; border: none; padding: 16px 30px; font-size: 1.1rem; border-radius: 50px; cursor: pointer; width: 100%; font-weight: bold; transition: all 0.2s; box-shadow: 0 4px 6px rgba(0,158,227,0.3); }
    .btn-pagar:hover { background: #0081b8; transform: translateY(-1px); box-shadow: 0 6px 8px rgba(0,158,227,0.4); }
    .btn-pagar:active { transform: translateY(0); }
    .btn-pagar:disabled { background: #ccc; cursor: not-allowed; box-shadow: none; }
    .error { color: #d8000c; background: #ffd2d2; padding: 10px; margin-top: 15px; border-radius: 6px; font-size: 0.9rem; }
  `]
})
export class CheckoutComponent {
  private paymentService = inject(PaymentService);
  
  amount = signal<number>(100.00); 
  loading = signal<boolean>(false);
  errorMessage = signal<string>('');

  irParaPagamento() {
    this.loading.set(true);
    this.errorMessage.set('');

    const dadosCompra = {
      titulo: "Pedido NuvPizza",
      quantidade: 1,
      precoUnitario: this.amount(),
      // Email não é obrigatório no request da preferência, mas ajuda o MP
      // Se não tiver login, pode enviar um genérico ou deixar que o MP peça lá
      emailPagador: "cliente_app@nuvpizza.com" 
    };

    this.paymentService.criarPreferencia(dadosCompra).subscribe({
      next: (resp: any) => {
        if (resp && resp.url) {
            // AQUI ACONTECE A MÁGICA
            // O navegador sai do seu site e vai para o Mercado Pago
            window.location.href = resp.url;
        } else {
            this.loading.set(false);
            this.errorMessage.set('Não foi possível gerar o link de pagamento.');
        }
      },
      error: (err) => {
        console.error("Erro API:", err);
        this.loading.set(false);
        this.errorMessage.set('Erro de comunicação com o servidor.');
      }
    });
  }
}