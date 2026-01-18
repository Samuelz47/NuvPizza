import { Component, OnInit, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoService, Pedido } from '../../../core/services/pedido.service';
import { NotificacaoService } from '../../../core/services/notificacao.service';

@Component({
  selector: 'app-painel-pedidos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './painel-pedidos.html',
  styleUrls: ['./painel-pedidos.css']
})
export class PainelPedidosComponent implements OnInit {
  private pedidoService = inject(PedidoService);
  private notificacaoService = inject(NotificacaoService);

  pedidos = signal<Pedido[]>([]);
  loading = signal<boolean>(true); // Adicionei loading

  ngOnInit() {
    this.carregarPedidos();
    this.iniciarListenersRealTime();
  }

  carregarPedidos() {
    this.loading.set(true);
    this.pedidoService.getPedidos().subscribe({
      next: (lista) => {
        // Ordena: Mais recentes primeiro
        const ordenados = lista.sort((a, b) => new Date(b.dataPedido).getTime() - new Date(a.dataPedido).getTime());
        this.pedidos.set(ordenados);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar pedidos', err);
        this.loading.set(false);
      }
    });
  }

  iniciarListenersRealTime() {
    // 1. Ouve novos pedidos
    this.notificacaoService.ouvirNovoPedido().subscribe((novoPedido: any) => {
      console.log("Novo pedido recebido no painel:", novoPedido);
      this.pedidos.update(listaAtual => [novoPedido, ...listaAtual]);
    });

    // 2. Ouve mudança de status
    this.notificacaoService.ouvirAtualizacaoStatus().subscribe((dados: { pedidoId: string, novoStatus: number }) => {
      console.log("Status atualizado no painel:", dados);
      this.pedidos.update(listaAtual => 
        listaAtual.map(p => {
          if (p.id === dados.pedidoId) {
            return { ...p, statusPedido: dados.novoStatus };
          }
          return p;
        })
      );
    });
  }

  avancarStatus(pedido: Pedido) {
    // Lógica alinhada com o C# (StatusPedido.cs)
    // 1 (Criado) -> 3 (Em Preparo) [Pula o 2 se for manual]
    // 3 (Em Preparo) -> 4 (Saiu para Entrega)
    // 4 (Saiu para Entrega) -> 5 (Entrega/Concluído)
    
    let proximoStatus = pedido.statusPedido + 1;

    // Se estiver "Criado" (1), o próximo lógico para a cozinha é "Em Preparo" (3), 
    // a menos que você use o "Confirmado" (2) manualmente. 
    // Vamos assumir sequencial simples por enquanto:
    
    if (pedido.statusPedido >= 5) return; 

    this.pedidoService.atualizarStatus(pedido.id, proximoStatus).subscribe({
      next: () => console.log(`Status atualizado para ${proximoStatus}`),
      error: (err) => alert('Erro ao atualizar status: ' + (err.error?.message || err.message))
    });
  }

  // --- MAPEAMENTO CORRETO COM O C# ---
  getNomeStatus(status: number): string {
    switch (status) {
      case 1: return 'Pendente (Criado)';
      case 2: return 'Pagamento Confirmado';
      case 3: return 'Em Preparo';
      case 4: return 'Saiu p/ Entrega';
      case 5: return 'Entregue';
      case 0: return 'Cancelado';
      default: return `Desconhecido (${status})`;
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-pendente';   // Amarelo
      case 2: return 'status-confirmado'; // Azul
      case 3: return 'status-preparo';    // Laranja
      case 4: return 'status-entrega';    // Roxo
      case 5: return 'status-concluido';  // Verde
      case 0: return 'status-cancelado';  // Vermelho
      default: return '';
    }
  }
}