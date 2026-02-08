import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoService } from '../../../core/services/pedido.service';
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

  // Sinais e Variáveis
  pedidos = signal<any[]>([]);
  
  // Controle do Modal Manual
  pedidoSelecionado: any = null;
  mostrarModal: boolean = false;
  
  toast = signal<{ mensagem: string, tipo: 'sucesso' | 'erro' | 'info', visivel: boolean }>({
    mensagem: '', tipo: 'info', visivel: false
  });

  ngOnInit() {
    this.carregarPedidos();
    this.ouvirNovosPedidos();
  }

  carregarPedidos() {
    this.pedidoService.getPedidos().subscribe({ 
      next: (dados: any) => {
        const lista = dados.items || dados;
        // Normaliza o status para número caso venha texto
        const listaTratada = lista.map((p: any) => ({
          ...p,
          statusPedido: this.converterStatusParaNumero(p.statusPedido)
        }));
        this.ordenarLista(listaTratada);
      },
      error: (err) => console.error(err)
    });
  }

  // --- NOVA LÓGICA DE MODAL (Simples e Direta) ---
  verDetalhes(pedido: any) {
    this.pedidoSelecionado = pedido;
    this.mostrarModal = true; // O *ngIf no HTML vai fazer o resto
  }

  fecharModal() {
    this.mostrarModal = false;
    this.pedidoSelecionado = null;
  }

  avancarStatus(pedido: any) {
    if (pedido.statusPedido >= 5) return;
    const novoStatus = pedido.statusPedido + 1;
    
    this.pedidoService.atualizarStatus(pedido.id, novoStatus).subscribe({
      next: () => {
        if (novoStatus === 5) this.mostrarToast(`Pedido #${pedido.numero} Concluído! 🎉`, 'sucesso');
        else if (novoStatus === 4) this.mostrarToast(`Pedido #${pedido.numero} saiu para entrega! 🛵`, 'info');
        else this.mostrarToast(`Status atualizado!`, 'info');
        
        // Atualiza a lista localmente para não precisar recarregar tudo
        this.pedidos.update(lista => 
            lista.map(p => p.id === pedido.id ? { ...p, statusPedido: novoStatus } : p)
        );
        this.ordenarLista(this.pedidos());
        
        // Se o modal estiver aberto com este pedido, atualiza ele também
        if (this.pedidoSelecionado && this.pedidoSelecionado.id === pedido.id) {
            this.pedidoSelecionado.statusPedido = novoStatus;
        }
      },
      error: () => this.mostrarToast('Erro ao atualizar status.', 'erro')
    });
  }

  cancelarPedido(pedido: any) {
    if (!confirm(`Cancelar pedido #${pedido.numero}?`)) return;
    this.pedidoService.atualizarStatus(pedido.id, 0).subscribe({
      next: () => {
        this.mostrarToast(`Pedido #${pedido.numero} Cancelado.`, 'erro');
        this.carregarPedidos();
        this.fecharModal(); // Fecha o modal se estiver aberto
      },
      error: () => this.mostrarToast('Erro ao cancelar.', 'erro')
    });
  }

  // --- IMPRESSÃO ---
  imprimirComanda(pedido: any) {
    const dataHora = new Date(pedido.dataPedido).toLocaleString('pt-BR');
    const telefone = pedido.telefone || pedido.linkWhatsapp || 'Não informado';

    const totalItens = pedido.itens.reduce((acc: number, item: any) => acc + item.total, 0);
    const frete = (pedido.valorFrete || 0);
    const totalGeral = (pedido.valorTotal || 0);

    const conteudo = `
      <html>
        <head>
          <title>Comanda #${pedido.numero}</title>
          <style>
            @media print { body { margin: 0; padding: 0; } }
            body { font-family: 'Courier New', monospace; width: 80mm; font-size: 13px; margin: 0 auto; padding: 10px; }
            .header { text-align: center; border-bottom: 1px dashed #000; padding-bottom: 5px; margin-bottom: 10px; }
            .bold { font-weight: bold; }
            .section { border-bottom: 1px dashed #000; padding-bottom: 5px; margin-bottom: 5px; }
            .row { display: flex; justify-content: space-between; }
            .total-big { font-size: 18px; font-weight: bold; margin-top: 5px; border-top: 1px solid #000; }
          </style>
        </head>
        <body>
          <div class="header">
            <div class="bold">NUVPIZZA DELIVERY</div>
            <div>Pedido: #${pedido.numero || pedido.id.substring(0,4)}</div>
            <div style="font-size:11px">${dataHora}</div>
          </div>
          <div class="section">
            <div><span class="bold">CLIENTE:</span> ${pedido.nomeCliente}</div>
            ${telefone !== 'Não informado' ? `<div>TEL: ${telefone}</div>` : ''}
          </div>
          <div class="section">
            <div class="bold">ENTREGA:</div>
            <div>${pedido.logradouro}, ${pedido.numero}</div>
            <div>${pedido.bairroNome}</div>
            ${pedido.complemento ? `<div>Comp: ${pedido.complemento}</div>` : ''}
          </div>
          <div class="section">
            <div class="bold mb-1">ITENS:</div>
            ${pedido.itens.map((item: any) => `
               <div class="row">
                  <span style="width:10%">${item.quantidade}x</span>
                  <span style="width:60%">${item.nomeProduto}</span>
                  <span style="width:30%; text-align:right">
                    ${(item.total).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </span>
               </div>
            `).join('')}
          </div>
          ${pedido.observacao ? `<div class="section"><span class="bold">OBS:</span> ${pedido.observacao}</div>` : ''}
          <div class="section" style="text-align:right">
            <div>Subtotal: ${totalItens.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</div>
            <div>Entrega: ${frete.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</div>
            <div class="total-big">TOTAL: ${totalGeral.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</div>
          </div>
          <div style="text-align:center; margin-top:10px">
            <div>PAGAMENTO:</div>
            <div class="bold" style="font-size:16px">${pedido.formaPagamento}</div>
          </div>
          <script>window.onload = function() { window.print(); }</script>
        </body>
      </html>
    `;
    const popup = window.open('', '_blank', 'width=380,height=600');
    if(popup) { popup.document.write(conteudo); popup.document.close(); }
  }

  // Auxiliares
  mostrarToast(msg: string, tipo: any) {
    this.toast.set({ mensagem: msg, tipo: tipo, visivel: true });
    setTimeout(() => this.toast.update(t => ({ ...t, visivel: false })), 3000);
  }

  ordenarLista(lista: any[]) { 
      lista.sort((a, b) => (a.statusPedido === 5 || a.statusPedido === 0) ? 1 : -1);
      this.pedidos.set(lista);
  }

  ouvirNovosPedidos() { 
      this.notificacaoService.ouvirAtualizacaoStatus().subscribe(() => this.carregarPedidos()); 
  }

  converterStatusParaNumero(status: any) {
    if (typeof status === 'number') return status;
    const s = status ? status.toString().toLowerCase().trim() : '';
    const mapa: any = { 'criado': 1, 'aguardando': 1, 'confirmado': 2, 'pago': 2, 'empreparo': 3, 'saiuparaentrega': 4, 'entrega': 5, 'finalizado': 5, 'cancelado': 0 };
    return mapa[s] !== undefined ? mapa[s] : (parseInt(s) || 1);
  }

  getNomeStatus(status: number) { return ['Cancelado', 'Criado', 'Confirmado', 'Em Preparo', 'Saiu p/ Entrega', 'Entregue'][status] || '...'; }
  getStatusClass(status: number) { return ['status-cancelado', 'status-criado', 'status-confirmado', 'status-preparo', 'status-entrega', 'status-finalizado'][status] || ''; }
}