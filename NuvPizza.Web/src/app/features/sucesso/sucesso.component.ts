import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router'; // Adicionei ActivatedRoute

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
  `]
})
export class SucessoComponent implements OnInit {
  private route = inject(ActivatedRoute); // Para ler a URL
  
  dados: any = null;

  ngOnInit() {
    // O Mercado Pago manda os dados via Query Params na URL
    // Ex: /sucesso?collection_status=approved&payment_id=123456...
    this.route.queryParams.subscribe(params => {
      if (params && params['collection_status']) {
        console.log('Dados do MP via URL:', params);

        this.dados = {
          status: params['collection_status'], // approved, pending, rejected
          paymentId: params['payment_id'] || params['collection_id'],
          externalReference: params['external_reference']
        };
      } 
      // Fallback: Se não vier da URL, tenta ver se veio do state (navegação interna)
      else if (history.state.dadosPagamento) {
         this.dados = history.state.dadosPagamento;
      }
    });
  }
}