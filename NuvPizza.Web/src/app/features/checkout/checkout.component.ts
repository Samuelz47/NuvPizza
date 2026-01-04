import { Component, OnInit, inject, signal, NgZone, ViewEncapsulation } from '@angular/core';
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
  encapsulation: ViewEncapsulation.None, 
  template: `
    <div class="checkout-container">
      <h2>Finalizar Pedido</h2>
      
      <div class="summary">
        <p>Total a pagar: <strong>{{ amount() | currency:'BRL' }}</strong></p>
      </div>
      
      <div id="paymentBrick_container"></div>

      @if (loading()) {
        <div class="loading">
          <span class="spinner">↻</span> {{ loadingText() }}
        </div>
      }
      
      @if (errorMessage()) {
        <div class="error">{{ errorMessage() }}</div>
      }
    </div>
  `,
  styles: [`
    .checkout-container { max-width: 600px; margin: 2rem auto; padding: 1.5rem; background: #fff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); font-family: sans-serif; }
    h2 { text-align: center; color: #333; }
    .summary { margin-bottom: 20px; padding: 15px; background: #f8f9fa; border-radius: 6px; text-align: center; font-size: 1.2rem; }
    .error { background-color: #ffe6e6; color: #d8000c; padding: 10px; margin-top: 15px; border-radius: 4px; text-align: center; }
    .loading { color: #009ee3; margin-top: 15px; font-weight: bold; text-align: center; }
    .spinner { display: inline-block; animation: spin 1s infinite linear; }
    @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
  `]
})
export class CheckoutComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private router = inject(Router);
  private ngZone = inject(NgZone);
  
  amount = signal<number>(100.00); 
  loading = signal<boolean>(true);
  loadingText = signal<string>('Carregando pagamentos...');
  errorMessage = signal<string>('');
  
  pedidoIdTeste = 123; 

  async ngOnInit() {
    try {
      await loadMercadoPago();
      const mp = new window.MercadoPago(environment.mercadoPagoPublicKey, {
        locale: 'pt-BR'
      });
      this.renderPaymentBrick(mp);
    } catch (error) {
      this.loading.set(false);
      this.errorMessage.set('Erro ao carregar SDK do Mercado Pago.');
    }
  }

  async renderPaymentBrick(mp: any) {
    const bricksBuilder = mp.bricks();

    const settings = {
      initialization: {
        amount: this.amount(),
        payer: {
          email: 'cliente@teste.com', // Preenchimento automático se tiver usuário logado
        },
      },
        customization: {
          visual: {
            style: { theme: "default" },
            hidePaymentButton: false
          },
          paymentMethods: {
            creditCard: "all",
            debitCard: "all",
            prepaidCard: "all",
            
            bankTransfer: ['pix'],
            
            mercadoPago: [], 
            
            maxInstallments: 3,
          },
        },
      callbacks: {
        onReady: () => {
          this.ngZone.run(() => this.loading.set(false));
        },
        onSubmit: async (brickResponse: any) => {
          // Esse método é chamado tanto para Cartão quanto para Google/Apple Pay
          // Ambos retornam um TOKEN que enviamos para API.
          
          this.ngZone.run(() => { 
              this.loading.set(true); 
              this.loadingText.set('Processando pagamento...');
              this.errorMessage.set(''); 
          });

          try {
            const dados = brickResponse.formData || brickResponse;
            
            const request: PagamentoRequest = {
              pedidoId: this.pedidoIdTeste, 
              transactionAmount: dados.transaction_amount,
              token: dados.token || '', // GPay/ApplePay preenchem isso aqui!
              description: "Pedido NuvPizza",
              paymentMethodId: dados.payment_method_id,
              installments: dados.installments,
              issuerId: dados.issuer_id ? String(dados.issuer_id) : '',
              payer: {
                email: dados.payer?.email || 'email_nao_informado@nuvpizza.com',
                firstName: dados.payer?.first_name || "Cliente", 
                phone: "11999999999", 
                identification: {
                  type: dados.payer?.identification?.type || 'CPF',
                  number: dados.payer?.identification?.number || '00000000000'
                }
              }
            };

            return new Promise((resolve, reject) => {
              this.paymentService.processarPagamento(request).subscribe({
                next: (res) => {
                  this.ngZone.run(() => {
                      this.loading.set(false);
                      resolve(true); 
                      this.router.navigate(['/sucesso'], { state: { dadosPagamento: res } });
                  });
                },
                error: (err) => {
                  this.ngZone.run(() => {
                      this.loading.set(false);
                      let msg = err.error?.message || 'Pagamento recusado.';
                      this.errorMessage.set(msg);
                      reject();
                  });
                }
              });
            });

          } catch (error) {
            this.ngZone.run(() => { 
                this.loading.set(false); 
                this.errorMessage.set('Erro interno ao enviar dados.'); 
            });
            return Promise.reject(); 
          }
        },
        onError: (error: any) => {
          console.error('Erro Brick:', error);
          this.ngZone.run(() => {
             this.loading.set(false);
             this.errorMessage.set('Ocorreu um erro no módulo de pagamento.');
          });
        },
      },
    };
    
    window.paymentBrickController = await bricksBuilder.create("payment", "paymentBrick_container", settings);
  }
}