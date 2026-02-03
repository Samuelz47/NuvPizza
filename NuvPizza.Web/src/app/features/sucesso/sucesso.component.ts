import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PedidoService } from '../../core/services/pedido.service';
import { NotificacaoService } from '../../core/services/notificacao.service'; // Importe o Notification

@Component({
  selector: 'app-sucesso',
  templateUrl: './sucesso.html',
  styleUrls: ['./sucesso.css']
})
export class SucessoComponent implements OnInit {
  pedidoId: string = '';
  // Usando Signal para reatividade na tela
  statusTexto = signal('Aguardando confirmação...'); 
  statusCor = signal('text-warning'); // Amarelo
  isConfirmado = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pedidoService: PedidoService,
    private notificacaoService: NotificacaoService
  ) {}

  ngOnInit(): void {
    // 1. Pega os parâmetros da URL
    this.route.queryParams.subscribe(params => {
      // O MP manda o ID externo como 'external_reference' ou você pode ter passado na rota
      const mpStatus = params['collection_status'] || params['status'];
      
      // TRUQUE VISUAL: Se o MP disse que aprovou na URL, já mostramos verde!
      if (mpStatus === 'approved') {
        this.atualizarParaConfirmado();
      }
    });

    // 2. Escuta o SignalR (Caso o usuário fique na tela esperando)
    this.notificacaoService.ouvirAtualizacaoStatus().subscribe((dados: any) => {
      console.log('Update recebido na tela de sucesso:', dados);
      if (dados.novoStatus === 2 || dados.novoStatus === 3) {
        this.atualizarParaConfirmado();
      }
    });
  }

  atualizarParaConfirmado() {
    this.statusTexto.set('Pagamento Confirmado!');
    this.statusCor.set('text-success'); // Verde (classe do Bootstrap)
    this.isConfirmado.set(true);
  }

  voltarCardapio() {
    this.router.navigate(['/']);
  }
}