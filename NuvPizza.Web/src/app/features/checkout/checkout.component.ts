import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; 
import { loadMercadoPago } from '@mercadopago/sdk-js';
import { environment } from '../../environments/environment';
import { PaymentService } from '../../core/services/payment.service';
import { PagamentoRequest } from '../../core/models/payment.model';

declare var window: any;

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="checkout-container">
      <h2>Finalizar Pedido</h2>
      
      <div class="summary">
        <p>Total a pagar: <strong>{{ amount() | currency:'BRL' }}</strong></p>
      </div>
      
      <div id="paymentBrick_container"></div>

      @if (loading()) {
        <div class="loading">
          <span class="spinner">↻</span> Processando pagamento...
        </div>
      }

      @if (errorMessage()) {
        <div class="error">
          {{ errorMessage() }}
        </div>
      }
    </div>
  `,
  styles: [`
    .checkout-container { 
      max-width: 600px; 
      margin: 2rem auto; 
      padding: 1.5rem; 
      font-family: sans-serif; 
      background: #fff;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      border-radius: 8px;
    }
    h2 { text-align: center; color: #333; }
    .summary { 
      margin-bottom: 20px; 
      padding: 15px; 
      background: #f8f9fa; 
      border-radius: 6px; 
      text-align: center;
      font-size: 1.2rem;
    }
    .error { 
      background-color: #ffe6e6; 
      color: #d8000c; 
      padding: 10px; 
      margin-top: 15px; 
      border-radius: 4px; 
      text-align: center;
    }
    .loading { 
      color: #009ee3; 
      margin-top: 15px; 
      font-weight: bold; 
      text-align: center; 
    }
    .spinner { display: inline-block; animation: spin 1s infinite linear; }
    @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
  `]
})
export class CheckoutComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private router = inject(Router);
  
  // Signals
  amount = signal<number>(100.00); 
  loading = signal<boolean>(false);
  errorMessage = signal<string>('');
  
  pedidoIdTeste = 123; 

  async ngOnInit() {
    await loadMercadoPago();
    const mp = new window.MercadoPago(environment.mercadoPagoPublicKey);
    const bricksBuilder = mp.bricks();

    const renderPaymentBrick = async (bricksBuilder: any) => {
      const settings = {
        initialization: {
          amount: this.amount(),
          payer: {
            email: 'test_user_123@test.com', 
          },
        },
        customization: {
          visual: {
            style: {
              theme: "default", 
            }
          },
          paymentMethods: {
            ticket: "all",
            bankTransfer: "all",
            creditCard: "all",
            debitCard: "all",
            mercadoPago: "all",
          },
        },
        callbacks: {
          onReady: () => {
            console.log('Brick carregado e pronto.');
          },
          onSubmit: async (paymentFormData: any) => {
            console.log('1. Botão Pagar clicado! Dados do Brick:', paymentFormData);
            
            this.loading.set(true);
            this.errorMessage.set('');

            // Mapeia os dados e blinda o telefone
            const request: PagamentoRequest = {
              pedidoId: this.pedidoIdTeste, 
              transactionAmount: paymentFormData.transaction_amount,
              token: paymentFormData.token,
              description: "Pedido NuvPizza - Teste",
              paymentMethodId: paymentFormData.payment_method_id,
              installments: paymentFormData.installments,
              issuerId: paymentFormData.issuer_id,
              payer: {
                email: paymentFormData.payer.email,
                firstName: "Cliente Teste", 
                // Se o brick não mandar telefone, mandamos um fake pra não travar o backend
                phone: "11999999999", 
                identification: {
                  type: paymentFormData.payer.identification.type,
                  number: paymentFormData.payer.identification.number
                }
              }
            };

            console.log('2. Enviando requisição para API...', request);

            return new Promise((resolve, reject) => {
              this.paymentService.processarPagamento(request).subscribe({
                next: (res) => {
                  console.log('3. SUCESSO! Resposta da API:', res);
                  this.loading.set(false);
                  
                  resolve(true); // Avisa o Brick que deu certo
                  this.router.navigate(['/sucesso']); // Redireciona
                },
                error: (err) => {
                  console.error('3. ERRO! Falha na requisição:', err);
                  this.loading.set(false);
                  
                  // Mensagem amigável para o usuário
                  if (err.status === 0) {
                     this.errorMessage.set('Erro de Conexão: Backend desligado ou bloqueado.');
                  } else if (err.status === 400) {
                     this.errorMessage.set('Dados inválidos. Verifique o cartão.');
                  } else {
                     this.errorMessage.set(`Erro no sistema: ${err.status}`);
                  }
                  
                  reject(); // Avisa o Brick que deu erro
                }
              });
            });
          },
          onError: (error: any) => {
            console.error('Erro interno do Brick:', error);
            this.errorMessage.set('Ocorreu um erro ao carregar o módulo de pagamento.');
          },
        },
      };
      
      window.paymentBrickController = await bricksBuilder.create(
        "payment",
        "paymentBrick_container",
        settings
      );
    };

    renderPaymentBrick(bricksBuilder);
  }
}