import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoService, Pedido } from '../../../core/services/pedido.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificacaoService } from '../../../core/services/notificacao.service'; // <--- Importe

@Component({
  selector: 'app-painel-pedidos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './painel-pedidos.html',
  styleUrls: ['./painel-pedidos.css']
})
export class PainelPedidosComponent implements OnInit, OnDestroy {
  private pedidoService = inject(PedidoService);
  private authService = inject(AuthService);
  private notificacaoService = inject(NotificacaoService); // <--- Injete

  pedidos = signal<Pedido[]>([]);
  loading = signal<boolean>(false);
  
  // Não precisamos mais do 'atualizacaoAutomatica' (setInterval)

  ngOnInit() {
    this.carregarPedidos();
    
    // Inicia a conexão com o WebSocket
    this.notificacaoService.iniciarConexao();

    // 1. Escuta Novos Pedidos
    this.notificacaoService.receberNovoPedido.subscribe((novoPedido) => {
      // Adiciona no topo da lista
      this.pedidos.update(listaAtual => [novoPedido, ...listaAtual]);
    });

    // 2. Escuta Atualizações de Status (ex: Pagamento Aprovado)
    this.notificacaoService.receberAtualizacaoStatus.subscribe(({ id, status }) => {
      this.pedidos.update(listaAtual => {
        return listaAtual.map(p => {
          if (p.id === id) {
            return { ...p, statusPedido: status }; // Atualiza o status
          }
          return p;
        });
      });
    });
  }

  ngOnDestroy() {
    // Se quiser desligar a conexão ao sair da tela, pode chamar um método de stop aqui
    // Mas geralmente deixamos aberto se for single page application
  }

  carregarPedidos() {
    this.loading.set(true);
    this.pedidoService.getPedidos(1, 50).subscribe({
      next: (dados) => {
        // Ordenação mantida
        const ordenados = dados.sort((a, b) => {
           if (a.statusPedido !== b.statusPedido) return a.statusPedido - b.statusPedido;
           return new Date(b.dataPedido).getTime() - new Date(a.dataPedido).getTime();
        });
        this.pedidos.set(ordenados);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
      }
    });
  }
  
  // ... Resto dos métodos (avancarStatus, getNomeStatus, sair) iguais ...
  avancarStatus(pedido: Pedido) { /* ... */ }
  getNomeStatus(status: number): string { /* ... */ return ''; }
  sair() { this.authService.logout(); }
}