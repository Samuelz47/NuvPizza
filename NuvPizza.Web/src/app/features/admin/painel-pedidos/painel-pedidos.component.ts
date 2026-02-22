import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PedidoService, PaginacaoMeta } from '../../../core/services/pedido.service';
import { NotificacaoService } from '../../../core/services/notificacao.service';
import { LojaService } from '../../../core/services/loja.service';

@Component({
  selector: 'app-painel-pedidos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './painel-pedidos.html',
  styleUrls: ['./painel-pedidos.css']
})
export class PainelPedidosComponent implements OnInit, OnDestroy {
  private pedidoService = inject(PedidoService);
  private notificacaoService = inject(NotificacaoService);
  private lojaService = inject(LojaService);

  pedidos = signal<any[]>([]);

  // Paginação
  paginaAtual = signal(1);
  readonly pageSize = 20;
  paginacaoMeta = signal<PaginacaoMeta | null>(null);

  // Array de números de página para o paginador
  paginas = computed<number[]>(() => {
    const total = this.paginacaoMeta()?.totalPages ?? 0;
    const atual = this.paginaAtual();
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    // Janela deslizante: mostra no max 5 páginas ao redor da atual
    const inicio = Math.max(1, Math.min(atual - 2, total - 4));
    return Array.from({ length: Math.min(5, total) }, (_, i) => inicio + i);
  });

  // Signals para Filtros
  termoBusca = signal('');
  statusFiltro = signal('TODOS');
  dataFiltro = signal(''); // Inicia vazio para carregar todos os pedidos por padrão

  // Computed: Pedidos filtrados a serem exibidos no HTML
  pedidosFiltrados = computed(() => {
    let lista = this.pedidos();
    const busca = this.termoBusca().toLowerCase().trim();
    const status = this.statusFiltro();

    // 1. Aplicar Busca por Texto (Nome, ID, ou Valor)
    if (busca) {
      lista = lista.filter(p => {
        const nome = p.nomeCliente?.toLowerCase() || '';
        const id = p.id?.toLowerCase() || '';
        const valor = p.valorTotal?.toString() || '';
        const numero = p.numero?.toString() || '';

        return nome.includes(busca) ||
          id.includes(busca) ||
          valor.includes(busca) ||
          numero.includes(busca);
      });
    }

    // 2. Aplicar Filtro de Status
    if (status !== 'TODOS') {
      if (status === 'PENDENTES') {
        lista = lista.filter(p => p.statusPedido !== 5 && p.statusPedido !== 0);
      } else if (status === 'FINALIZADOS') {
        lista = lista.filter(p => p.statusPedido === 5);
      } else if (status === 'CANCELADOS') {
        lista = lista.filter(p => p.statusPedido === 0);
      }
    }

    return lista;
  });

  pedidoSelecionado: any = null;
  mostrarModal: boolean = false;
  toast = signal<{ mensagem: string, tipo: 'sucesso' | 'erro' | 'info', visivel: boolean }>({
    mensagem: '', tipo: 'info', visivel: false
  });

  // Status & Countdown da Loja
  lojaAberta = signal(false);
  contagemRegressiva = signal<string | null>(null);
  pertoDeFecahr = signal(false); // true se faltam < 30 min
  private dataHoraFechamento: Date | null = null;
  private timerInterval: any = null;

  ngOnInit() {
    this.carregarPedidos();
    this.ouvirNovosPedidos();
    this.carregarStatusLoja();
  }

  ngOnDestroy() {
    if (this.timerInterval) clearInterval(this.timerInterval);
  }

  carregarStatusLoja() {
    this.lojaService.getStatus().subscribe({
      next: (status) => {
        this.lojaAberta.set(status.estaAberta);
        if (status.estaAberta && status.dataHoraFechamento) {
          this.dataHoraFechamento = new Date(status.dataHoraFechamento);
          this.iniciarContagem();
        } else {
          this.dataHoraFechamento = null;
          this.contagemRegressiva.set(null);
          this.pertoDeFecahr.set(false);
          if (this.timerInterval) { clearInterval(this.timerInterval); this.timerInterval = null; }
        }
      }
    });
  }

  private iniciarContagem() {
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.atualizarContagem();
    this.timerInterval = setInterval(() => this.atualizarContagem(), 1000);
  }

  private atualizarContagem() {
    if (!this.dataHoraFechamento) return;
    const agora = new Date();
    const diff = this.dataHoraFechamento.getTime() - agora.getTime();

    if (diff <= 0) {
      this.lojaAberta.set(false);
      this.contagemRegressiva.set(null);
      this.pertoDeFecahr.set(false);
      clearInterval(this.timerInterval);
      this.timerInterval = null;
      this.mostrarToast('A loja fechou automaticamente! 🛑', 'info');
      return;
    }

    const horas = Math.floor(diff / 3600000);
    const minutos = Math.floor((diff % 3600000) / 60000);
    const segundos = Math.floor((diff % 60000) / 1000);

    const partes: string[] = [];
    if (horas > 0) partes.push(`${horas}h`);
    partes.push(`${minutos.toString().padStart(2, '0')}min`);
    partes.push(`${segundos.toString().padStart(2, '0')}s`);

    this.contagemRegressiva.set(partes.join(' '));
    this.pertoDeFecahr.set(diff < 30 * 60 * 1000); // < 30 min
  }

  estenderHorario() {
    const minutos = prompt('Quantos minutos deseja estender?', '30');
    if (!minutos) return;
    const num = parseInt(minutos);
    if (isNaN(num) || num <= 0) { this.mostrarToast('Valor inválido.', 'erro'); return; }

    this.lojaService.estenderLoja(num).subscribe({
      next: () => {
        this.mostrarToast(`Horário estendido em ${num} minutos! ⏰`, 'sucesso');
        this.carregarStatusLoja();
      },
      error: () => this.mostrarToast('Erro ao estender horário.', 'erro')
    });
  }

  abrirLoja() {
    const hora = prompt("Qual será o horário de encerramento da loja hoje? (Ex: 23:59)");
    if (!hora) return;

    this.lojaService.abrirLoja(hora).subscribe({
      next: () => {
        this.mostrarToast(`Loja aberta até as ${hora}! 🍕`, 'sucesso');
        this.carregarStatusLoja();
      },
      error: () => this.mostrarToast('Erro ao abrir a loja. Verifique o horário.', 'erro')
    });
  }

  fecharLoja() {
    if (!confirm("Tem certeza que deseja FECHAR a loja agora? Os clientes não conseguirão fazer novos pedidos.")) return;

    this.lojaService.fecharLoja().subscribe({
      next: () => {
        this.mostrarToast('Loja fechada com sucesso. 🛑', 'sucesso');
        this.carregarStatusLoja();
      },
      error: () => this.mostrarToast('Erro ao fechar a loja.', 'erro')
    });
  }

  limparFiltros() {
    this.termoBusca.set('');
    this.statusFiltro.set('TODOS');
    this.dataFiltro.set('');
    this.paginaAtual.set(1);
    this.carregarPedidos();
  }

  irParaPagina(pagina: number) {
    const meta = this.paginacaoMeta();
    if (pagina < 1 || (meta && pagina > meta.totalPages)) return;
    this.paginaAtual.set(pagina);
    this.carregarPedidos();
  }

  proximaPagina() {
    if (this.paginacaoMeta()?.hasNextPage) this.irParaPagina(this.paginaAtual() + 1);
  }

  paginaAnterior() {
    if (this.paginacaoMeta()?.hasPreviousPage) this.irParaPagina(this.paginaAtual() - 1);
  }

  carregarPedidos() {
    this.pedidoService.getPedidos(this.paginaAtual(), this.pageSize, this.dataFiltro()).subscribe({
      next: ({ itens, paginacao }) => {
        this.paginacaoMeta.set(paginacao);
        const listaTratada = itens.map((p: any) => ({
          ...p,
          statusPedido: this.converterStatusParaNumero(p.statusPedido)
        }));
        this.ordenarLista(listaTratada);
      },
      error: (err) => {
        console.error('Erro ao carregar pedidos', err);
        this.mostrarToast('Erro ao carregar pedidos do servidor.', 'erro');
      }
    });
  }

  getCodigoPedido(pedido: any): string {
    if (!pedido || !pedido.id) return '#???';
    return `#${pedido.id.substring(0, 8).toUpperCase()}`;
  }

  verDetalhes(pedido: any) {
    this.pedidoSelecionado = pedido;
    this.mostrarModal = true;
  }

  fecharModal() {
    this.mostrarModal = false;
    this.pedidoSelecionado = null;
  }

  avancarStatus(pedido: any) {
    if (pedido.statusPedido >= 5) return;
    const novoStatus = pedido.statusPedido + 1;
    const codigo = this.getCodigoPedido(pedido);

    this.pedidoService.atualizarStatus(pedido.id, novoStatus).subscribe({
      next: () => {
        if (novoStatus === 5) this.mostrarToast(`Pedido ${codigo} Concluído! 🎉`, 'sucesso');
        else if (novoStatus === 4) this.mostrarToast(`Pedido ${codigo} saiu para entrega! 🛵`, 'info');
        else this.mostrarToast(`Status do pedido ${codigo} atualizado!`, 'info');

        this.pedidos.update(lista =>
          lista.map(p => p.id === pedido.id ? { ...p, statusPedido: novoStatus } : p)
        );
        this.ordenarLista(this.pedidos());

        if (this.pedidoSelecionado && this.pedidoSelecionado.id === pedido.id) {
          this.pedidoSelecionado.statusPedido = novoStatus;
        }
      },
      error: () => this.mostrarToast('Erro ao atualizar status.', 'erro')
    });
  }

  cancelarPedido(pedido: any) {
    const codigo = this.getCodigoPedido(pedido);
    if (!confirm(`Tem certeza que deseja cancelar o pedido ${codigo}?`)) return;

    this.pedidoService.atualizarStatus(pedido.id, 0).subscribe({
      next: () => {
        this.mostrarToast(`Pedido ${codigo} Cancelado.`, 'erro');
        this.carregarPedidos();
        this.fecharModal();
      },
      error: () => this.mostrarToast('Erro ao cancelar.', 'erro')
    });
  }

  imprimirComanda(pedido: any) {
    const dataHora = new Date(pedido.dataPedido).toLocaleString('pt-BR');
    const telefone = pedido.telefone || pedido.linkWhatsapp || 'Não informado';
    const codigo = this.getCodigoPedido(pedido);

    const totalItens = pedido.itens.reduce((acc: number, item: any) => acc + (item.total || (item.preco * item.quantidade)), 0);
    const frete = (pedido.valorFrete || 0);
    const totalGeral = (pedido.valorTotal || (totalItens + frete));

    const conteudo = `
      <html>
        <head>
          <title>Comanda ${codigo}</title>
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
            <div class="bold" style="font-size: 14px; margin-top: 5px;">${codigo}</div>
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
                  <span style="width:60%">${item.nomeProduto || item.nome}</span>
                  <span style="width:30%; text-align:right">
                    ${(item.total || item.preco * item.quantidade).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
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
            <div class="bold" style="font-size:16px">
                ${this.traduzirPagamento(pedido.formaPagamento)}
            </div>
          </div>
          <script>window.onload = function() { window.print(); }</script>
        </body>
      </html>
    `;
    const popup = window.open('', '_blank', 'width=380,height=600');
    if (popup) { popup.document.write(conteudo); popup.document.close(); }
  }

  mostrarToast(msg: string, tipo: any) {
    this.toast.set({ mensagem: msg, tipo: tipo, visivel: true });
    setTimeout(() => this.toast.update(t => ({ ...t, visivel: false })), 3000);
  }

  ordenarLista(lista: any[]) {
    // Ordenação Inteligente
    // 1. Pendentes em cima, Finalizados em Baixo
    // 2. Pendentes: Ordem Crescente de Data (Mais antigo primeiro - para a cozinha não atrasar)
    // 3. Finalizados/Cancelados: Ordem Decrescente de Data (Mais novo primeiro)

    lista.sort((a, b) => {
      const aFinalizado = a.statusPedido === 5 || a.statusPedido === 0;
      const bFinalizado = b.statusPedido === 5 || b.statusPedido === 0;

      // Se um é Grupo Finalizado e outro é Grupo Pendente
      if (aFinalizado && !bFinalizado) return 1;  // A vai pra baixo
      if (!aFinalizado && bFinalizado) return -1; // B vai pra baixo

      // Se ambos estão no mesmo grupo (Ambos Pendentes ou Ambos Finalizados)
      const dataA = new Date(a.dataPedido).getTime();
      const dataB = new Date(b.dataPedido).getTime();

      if (aFinalizado) {
        // Grupo Finalizados: Mais Recentes Primeiro (Decrescente)
        return dataB - dataA;
      } else {
        // Grupo Pendentes (Cozinha): Mais Antigos Primeiro (Crescente)
        return dataA - dataB;
      }
    });

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

  traduzirPagamento(forma: any): string {
    // Mapeamento ajustado
    const mapa: any = {
      1: 'Pix',
      2: 'Dinheiro',
      3: 'Crédito (Entrega)', // Enum 3
      4: 'Débito (Entrega)',  // Enum 4
      5: 'Cartão (Entrega)',
      6: 'Online (MP)'
    };

    if (typeof forma === 'number') return mapa[forma] || 'Outro';

    // Fallbacks para strings
    if (forma === 'MercadoPago') return 'Online (MP)';
    if (forma === 'CartaoCredito') return 'Crédito (Entrega)';
    if (forma === 'CartaoDebito') return 'Débito (Entrega)';

    return forma || 'A Definir';
  }
}