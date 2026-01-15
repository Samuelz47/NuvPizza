import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoService, Pedido } from '../../../core/services/pedido.service';

@Component({
  selector: 'app-painel-pedidos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './painel-pedidos.html',
  styles: [`
    .painel-container { padding: 20px; max-width: 1200px; margin: 0 auto; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .grid-pedidos { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
    
    /* Cores dos Cards baseadas no Status */
    .card { border: 1px solid #ddd; border-radius: 8px; padding: 15px; background: white; box-shadow: 0 2px 5px rgba(0,0,0,0.1); border-left: 5px solid #ccc; transition: transform 0.2s; }
    .card:hover { transform: translateY(-3px); }
    
    .status-0 { border-left-color: #ffc107; background: #fffdf5; } /* Pendente (Amarelo) */
    .status-1 { border-left-color: #fd7e14; background: #fff5eb; } /* Preparo (Laranja) */
    .status-2 { border-left-color: #17a2b8; background: #f0fcff; } /* Saiu Entrega (Azul) */
    .status-3 { border-left-color: #28a745; background: #f0fff4; opacity: 0.7; } /* Entregue (Verde) */
    .status-4 { border-left-color: #dc3545; background: #fff5f5; opacity: 0.6; } /* Cancelado (Vermelho) */

    .card-header { display: flex; justify-content: space-between; font-weight: bold; margin-bottom: 10px; font-size: 1.1rem; }
    .card-body p { margin: 5px 0; color: #555; }
    .itens-lista { margin-top: 10px; border-top: 1px solid #eee; padding-top: 5px; }
    .item-row { display: flex; justify-content: space-between; font-size: 0.95rem; }
    
    .actions { margin-top: 15px; display: flex; gap: 10px; }
    .btn { flex: 1; padding: 8px; border: none; border-radius: 4px; cursor: pointer; font-weight: bold; color: white; transition: opacity 0.2s; }
    .btn:hover { opacity: 0.9; }
    .btn-avancar { background-color: #007bff; }
    .btn-cancelar { background-color: #dc3545; }
  `]
})
export class PainelPedidosComponent implements OnInit, OnDestroy {
  private pedidoService = inject(PedidoService);
  
  pedidos = signal<Pedido[]>([]);
  loading = signal<boolean>(false);
  atualizacaoAutomatica: any;

  ngOnInit() {
    this.carregarPedidos();

    // Polling: Atualiza a cada 15 segundos
    this.atualizacaoAutomatica = setInterval(() => {
      this.carregarPedidos(true); // true = silencioso (sem loading)
    }, 15000);
  }

  ngOnDestroy() {
    if (this.atualizacaoAutomatica) {
      clearInterval(this.atualizacaoAutomatica);
    }
  }

  carregarPedidos(silencioso = false) {
    if (!silencioso) this.loading.set(true);

    this.pedidoService.getPedidos(1, 50).subscribe({
      next: (dados) => {
        // Ordena: Pendentes primeiro, depois Preparo, etc.
        // E dentro do status, os mais antigos primeiro (FIFO)
        const ordenados = dados.sort((a, b) => {
           if (a.statusPedido !== b.statusPedido) {
             return a.statusPedido - b.statusPedido;
           }
           return new Date(a.dataPedido).getTime() - new Date(b.dataPedido).getTime();
        });
        this.pedidos.set(ordenados);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar pedidos', err);
        this.loading.set(false);
      }
    });
  }

  avancarStatus(pedido: Pedido) {
    if (pedido.statusPedido >= 3) return; // Já entregue ou cancelado

    const proximoStatus = pedido.statusPedido + 1;
    
    this.pedidoService.atualizarStatus(pedido.id, proximoStatus).subscribe({
      next: () => {
        this.carregarPedidos(true); // Recarrega a lista
      },
      error: (err) => alert('Erro ao atualizar status: ' + (err.error || err.message))
    });
  }

  getNomeStatus(status: number): string {
    switch(status) {
      case 0: return 'Pendente';
      case 1: return 'Em Preparo';
      case 2: return 'Saiu p/ Entrega';
      case 3: return 'Entregue';
      case 4: return 'Cancelado';
      default: return 'Desconhecido';
    }
  }
}