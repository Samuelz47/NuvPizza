import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PedidoService } from '../../core/services/pedido.service';
import { NotificacaoService } from '../../core/services/notificacao.service';

@Component({
  selector: 'app-sucesso',
  standalone: true,
  imports: [CommonModule, RouterModule], 
  templateUrl: './sucesso.html',
  styleUrls: ['./sucesso.css']
})
export class SucessoComponent implements OnInit {
  private route = inject(ActivatedRoute);
  public router = inject(Router);
  private pedidoService = inject(PedidoService);
  private notificacaoService = inject(NotificacaoService);

  // Signals
  idPedido = signal<string | null>(null);
  statusTexto = signal('Processando...'); 
  statusCor = signal('text-warning'); 
  isConfirmado = signal(false);

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    const externalRef = params['external_reference']; 
    const stateId = history.state?.id;

    // 1. Prioridade: ID vindo do Checkout (Pagamento na Entrega)
    if (stateId) {
        this.idPedido.set(stateId);
        // Pagamento na entrega = Confirmado Imediato
        this.atualizarParaConfirmado();
    } 
    // 2. Prioridade: ID vindo do Mercado Pago
    else if (externalRef) {
        this.idPedido.set(externalRef);
        const mpStatus = params['collection_status'] || params['status'];
        if (mpStatus === 'approved') {
            this.atualizarParaConfirmado();
        } else {
            this.statusTexto.set('Aguardando confirmação do pagamento...');
        }
    }

    // 3. Atualização em Tempo Real
    this.notificacaoService.ouvirAtualizacaoStatus().subscribe((dados: any) => {
      if (dados.novoStatus >= 1) { 
        this.atualizarParaConfirmado();
      }
    });
  }

  atualizarParaConfirmado() {
    this.statusTexto.set('Pedido Recebido! A cozinha já vai começar.');
    this.statusCor.set('text-success'); 
    this.isConfirmado.set(true);
  }

  acompanharPedido() {
    if (this.idPedido()) {
      this.router.navigate(['/acompanhar', this.idPedido()]);
    } else {
      this.router.navigate(['/']);
    }
  }
}