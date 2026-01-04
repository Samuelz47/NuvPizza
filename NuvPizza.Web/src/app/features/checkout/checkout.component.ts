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
          <span class="spinner">↻</span> Processando...
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
  loading = signal<boolean>(false);
  errorMessage = signal<string>('');
  pedidoIdTeste = 123; 

  async ngOnInit() {
    await loadMercadoPago();
    const mp = new window.MercadoPago(environment.mercadoPagoPublicKey, {
      locale: 'pt-BR'
    });
    const bricksBuilder = mp.bricks();

    const renderPaymentBrick = async (bricksBuilder: any) => {
      const settings = {
        initialization: {
          amount: this.amount(),
          payer: {
            email: 'test_user_123@test.com',
            firstName: 'Cliente',
            lastName: 'Teste',
            entityType: 'individual', 
            // identification: ... (removido para o usuário preencher)
          },
        },
        customization: {
          visual: {
            style: { theme: "default" },
            hidePaymentButton: false
          },
          paymentMethods: {
            // --- CONFIGURAÇÕES FINAIS ---
            ticket: [],              // Sem Boleto
            bankTransfer: ['pix'],   // Apenas Pix
            creditCard: "all",       // Crédito liberado
            debitCard: [],           // <--- REMOVIDO (Resolve o problema da Caixa)
            mercadoPago: "all",      // Saldo MP
            
            // --- PARCELAMENTO 3x ---
            maxInstallments: 3,      
            minInstallments: 1,      
            
            // Mantive a lista de exclusão por segurança, mas o debitCard: [] já deve resolver
            excludedPaymentMethods: ['pec', 'bolbradesco'] 
          },
        },
        callbacks: {
          onReady: () => console.log('Brick pronto.'),
          onSubmit: async (brickResponse: any) => {
            this.ngZone.run(() => { this.loading.set(true); this.errorMessage.set(''); });

            try {
              const dados = brickResponse.formData || brickResponse;
              
              const request: PagamentoRequest = {
                pedidoId: this.pedidoIdTeste, 
                transactionAmount: dados.transaction_amount,
                token: dados.token || '',
                description: "Pedido NuvPizza",
                paymentMethodId: dados.payment_method_id,
                installments: dados.installments,
                issuerId: dados.issuer_id ? String(dados.issuer_id) : '',
                payer: {
                  email: dados.payer?.email || 'test_user_123@test.com',
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
                        let msg = err.error?.message || err.error || 'Erro ao processar pagamento.';
                        
                        if (typeof msg !== 'string') {
                            msg = JSON.stringify(msg);
                        }

                        this.errorMessage.set(msg);
                        reject();
                    });
                  }
                });
              });

            } catch (error) {
              this.ngZone.run(() => { this.loading.set(false); this.errorMessage.set('Erro interno.'); });
              return Promise.reject(); 
            }
          },
          onError: (error: any) => {
            console.error('Erro Brick:', error);
            this.ngZone.run(() => this.errorMessage.set('Erro ao carregar módulo de pagamento.'));
          },
        },
      };
      
      window.paymentBrickController = await bricksBuilder.create("payment", "paymentBrick_container", settings);
    };

    renderPaymentBrick(bricksBuilder);
  }
}