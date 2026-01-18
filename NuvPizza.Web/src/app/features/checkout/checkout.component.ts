import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoService } from '../../core/services/pedido.service'; 
// Removemos o PaymentService pois o PedidoService vai tratar de tudo agora

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="checkout-container">
      <h2>Finalizar Pedido</h2>
      
      <div class="summary">
        <p>Total a pagar: <strong>{{ 45.90 | currency:'BRL' }}</strong></p>
        
        <div class="info-box">
           <p>⚠️ <strong>Modo de Teste:</strong></p>
           <p style="font-size: 0.9rem">Ao clicar em pagar, será criado um pedido fixo (1 Pizza) para o CEP 59150-000.</p>
        </div>
      </div>
      
      <button (click)="finalizarPedido()" [disabled]="loading()" class="btn-pagar">
        @if (loading()) {
          <span>A criar pedido...</span>
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
    .info-box { background: #fff3cd; color: #856404; padding: 15px; border-radius: 8px; margin-top: 15px; border: 1px solid #ffeeba; }
    .btn-pagar { background: #009ee3; color: white; border: none; padding: 16px 30px; font-size: 1.1rem; border-radius: 50px; cursor: pointer; width: 100%; font-weight: bold; transition: all 0.2s; box-shadow: 0 4px 6px rgba(0,158,227,0.3); }
    .btn-pagar:hover { background: #0081b8; transform: translateY(-1px); box-shadow: 0 6px 8px rgba(0,158,227,0.4); }
    .btn-pagar:disabled { background: #ccc; cursor: not-allowed; }
    .error { color: #d8000c; background: #ffd2d2; padding: 10px; margin-top: 15px; border-radius: 6px; font-size: 0.9rem; }
  `]
})
export class CheckoutComponent {
  private pedidoService = inject(PedidoService);

  loading = signal<boolean>(false);
  errorMessage = signal<string>('');

  finalizarPedido() {
    this.loading.set(true);
    this.errorMessage.set('');

    // DADOS MOCKADOS (SIMULAÇÃO DO CARRINHO)
    // Quando tivermos o carrinho real, estes dados virão de lá.
    const pedidoDto = {
      nomeCliente: "Cliente Teste Angular",
      emailCliente: "cliente.teste@nuvpizza.com", // Obrigatório p/ Mercado Pago
      telefoneCliente: "11999998888",
      cep: "59070120", // CEP Válido para o ViaCep não falhar
      numero: "123",
      complemento: "Casa Verde",
      formaPagamento: "Pix",
      itens: [
        {
          produtoId: 1, // <--- IMPORTANTE: Garanta que existe um Produto com ID 1 no seu banco
          quantidade: 1
        }
      ]
    };

    this.pedidoService.createPedido(pedidoDto).subscribe({
      next: (resp: any) => {
        console.log("Resposta do Backend:", resp);

        // O backend retorna: { pedido: {...}, paymentLink: "https://..." }
        if (resp && resp.paymentLink) {
            // Redireciona o utilizador para o Mercado Pago
            window.location.href = resp.paymentLink;
        } else {
            this.loading.set(false);
            this.errorMessage.set('Pedido criado, mas nenhum link de pagamento foi retornado.');
        }
      },
      error: (err) => {
        console.error("Erro ao criar pedido:", err);
        this.loading.set(false);
        // Tenta mostrar a mensagem de erro que vem do backend (ex: "Produto não encontrado")
        const msg = err.error?.error || err.error?.message || 'Erro de comunicação com o servidor.';
        this.errorMessage.set(msg);
      }
    });
  }
}