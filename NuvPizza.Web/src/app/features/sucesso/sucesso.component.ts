import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificacaoService } from '../../core/services/notificacao.service';

@Component({
  selector: 'app-sucesso',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="sucesso-container">
      <div class="icon-circle">✅</div>
      <h2>Pedido Recebido!</h2>
      
      <div class="status-box">
        <p>Status atual:</p>
        <h3 [class.pago]="status() === 'Em Preparo'">
            {{ statusDescricao() }}
        </h3>
      </div>

      <p class="msg">
        @if(status() === 'Pendente') {
          Aguardando confirmação do pagamento...
        } @else {
          Tudo certo! A cozinha já vai começar o preparo.
        }
      </p>

      <button (click)="irParaHome()">Voltar ao Cardápio</button>
    </div>
  `,
  styles: [`
    .sucesso-container { text-align: center; padding: 40px; max-width: 500px; margin: 0 auto; }
    .icon-circle { font-size: 4rem; margin-bottom: 20px; }
    .status-box { background: #f8f9fa; padding: 20px; border-radius: 10px; margin: 20px 0; border: 1px solid #dee2e6; }
    .pago { color: #28a745; font-weight: bold; }
    button { background: #ff4500; color: white; border: none; padding: 12px 25px; border-radius: 25px; cursor: pointer; font-size: 1rem; }
  `]
})
export class SucessoComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private notificacaoService = inject(NotificacaoService);

  status = signal<string>('Pendente');
  pedidoId: string = '';

  ngOnInit() {
    this.notificacaoService.ouvirAtualizacaoStatus().subscribe((dados: any) => {
        console.log("Notificação na tela de sucesso:", dados);
        
        // CORREÇÃO: Verifica se o status é 3 (Em Preparo) ou 2 (Confirmado)
        // O seu PagamentoController define como 3.
        if (dados.novoStatus === 3 || dados.novoStatus === 2) {
             this.status.set('Em Preparo');
             // O som já toca no notificacao.service.ts, não precisa tocar aqui de novo
             // para não dar eco, ou pode forçar um feedback visual.
        }
    });
  }

  statusDescricao() {
    return this.status();
  }

  irParaHome() {
    this.router.navigate(['/']);
  }
}