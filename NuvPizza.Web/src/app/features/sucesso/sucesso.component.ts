import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // <--- OBRIGATÓRIO
import { ActivatedRoute, Router, RouterModule } from '@angular/router'; // <--- OBRIGATÓRIO
import { PedidoService } from '../../core/services/pedido.service';
import { NotificacaoService } from '../../core/services/notificacao.service';

@Component({
  selector: 'app-sucesso',
  standalone: true, // <--- ESSA LINHA RESOLVE O ERRO
  imports: [CommonModule, RouterModule], // <--- ESSA LINHA PERMITE USAR @IF E ROUTERLINK
  templateUrl: './sucesso.html',
  styleUrls: ['./sucesso.css']
})
export class SucessoComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private pedidoService = inject(PedidoService);
  private notificacaoService = inject(NotificacaoService);

  // Signals
  idPedido = signal<string | null>(null);
  statusTexto = signal('Aguardando confirmação...'); 
  statusCor = signal('text-warning'); 
  isConfirmado = signal(false);

  ngOnInit(): void {
    // 1. Captura parâmetros da URL
    this.route.queryParams.subscribe(params => {
      const mpStatus = params['collection_status'] || params['status'];
      const externalRef = params['external_reference']; // O ID do Pedido devolvido pelo MP

      // Se tiver ID na URL (volta do MP), salva
      if (externalRef) {
        this.idPedido.set(externalRef);
      } 
      // Se não, tenta pegar do state (navegação interna)
      else if (history.state?.id) {
        this.idPedido.set(history.state.id);
      }

      // Se o MP disse que está aprovado, já mostra verde
      if (mpStatus === 'approved') {
        this.atualizarParaConfirmado();
      }
    });

    // 2. Escuta o SignalR para atualizações em tempo real
    this.notificacaoService.ouvirAtualizacaoStatus().subscribe((dados: any) => {
      console.log('Update recebido na tela de sucesso:', dados);
      if (dados.novoStatus >= 1) { // 1=Recebido/Pago
        this.atualizarParaConfirmado();
      }
    });
  }

  atualizarParaConfirmado() {
    this.statusTexto.set('Pagamento Confirmado! A cozinha já vai começar.');
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