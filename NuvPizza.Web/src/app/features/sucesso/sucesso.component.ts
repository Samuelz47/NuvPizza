import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-sucesso',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './sucesso.html',
  styles: [`
    .sucesso-container { text-align: center; padding: 30px; font-family: sans-serif; max-width: 600px; margin: 0 auto; }
    .card { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
    h1 { color: #28a745; margin-bottom: 10px; }
    h1.pendente { color: #ffc107; }
    .btn-voltar { display: inline-block; margin-top: 20px; padding: 10px 20px; background: #009ee3; color: white; text-decoration: none; border-radius: 5px; cursor: pointer; border: none; }
    .btn-voltar:hover { background-color: #007eb5; }
  `]
})
export class SucessoComponent implements OnInit {
  private route = inject(ActivatedRoute);
  
  dados: any = null;

  ngOnInit() {
    // Lê os parâmetros da URL vindos do Mercado Pago (?collection_status=approved...)
    this.route.queryParams.subscribe(params => {
      if (params && params['collection_status']) {
        console.log('Retorno MP:', params);

        this.dados = {
          status: params['collection_status'], 
          paymentId: params['payment_id'] || params['collection_id'],
          externalReference: params['external_reference']
        };
      } 
      // Fallback: Se você navegar internamente pelo site
      else if (history.state.dadosPagamento) {
         this.dados = history.state.dadosPagamento;
      }
    });
  }
}