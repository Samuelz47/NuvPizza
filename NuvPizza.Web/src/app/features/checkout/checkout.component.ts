import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { PedidoService } from '../../core/services/pedido.service';
import { CarrinhoService } from '../../core/services/carrinho.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css']
})
export class CheckoutComponent {
  private pedidoService = inject(PedidoService);
  public carrinhoService = inject(CarrinhoService);
  private router = inject(Router);
  private http = inject(HttpClient);

  // Sinais de Estado
  loading = signal<boolean>(false);
  errorMessage = signal<string>('');
  buscandoCep = signal<boolean>(false);
  bairroNaoAtendido = signal<boolean>(false);
  freteLabel = signal<string>('---');

  idPedidoCriado = signal<string | null>(null);

  // Controle de UI do Pagamento
  tipoPagamento = signal<'ONLINE' | 'ENTREGA'>('ONLINE');
  opcaoEntregaSelecionada = signal<string>('');
  trocoPara = signal<string>(''); // <--- NOVO: Controle do Troco

  // MAPEAMENTO STRINGS
  readonly PAGAMENTO_ENUM = {
    NaoDefinido: 'NaoDefinido',
    Pix: 'Pix',
    Dinheiro: 'Dinheiro',
    CartaoCredito: 'CartaoCredito',
    CartaoDebito: 'CartaoDebito',
    CartaoEntrega: 'CartaoEntrega',
    MercadoPago: 'MercadoPago'
  };

  pedido: any = {
    nomeCliente: '',
    emailCliente: '',
    telefoneCliente: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    observacao: '',
    formaPagamento: 'NaoDefinido',
    itens: []
  };

  constructor() {
    this.selecionarTipoPagamento('ONLINE');
  }

  adicionarItemTeste() {
    this.carrinhoService.adicionar({
      id: 1,
      nome: 'Pizza de Teste (Calabresa)',
      preco: 45.90,
      imagemUrl: ''
    });
  }

  buscarCep() {
    const cep = this.pedido.cep?.replace(/\D/g, '');
    if (cep?.length !== 8) return;

    this.buscandoCep.set(true);
    this.bairroNaoAtendido.set(false);
    this.freteLabel.set('---');
    this.carrinhoService.valorFrete.set(0);

    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (dados) => {
        this.buscandoCep.set(false);
        if (!dados.erro) {
          this.pedido.logradouro = dados.logradouro;
          this.pedido.bairro = dados.bairro;

          // Tenta encontrar o bairro na lista da API local para pegar o frete
          this.http.get<any[]>(`${environment.apiUrl}/bairros`).subscribe({
            next: (bairros) => {
              const bairroNome = (dados.bairro || '').toLowerCase().trim();
              const encontrado = bairros.find(
                b => b.nome.toLowerCase().trim() === bairroNome
              );
              if (encontrado) {
                this.carrinhoService.valorFrete.set(encontrado.valorFrete);
                this.freteLabel.set(`R$ ${encontrado.valorFrete.toFixed(2).replace('.', ',')}`);
                this.bairroNaoAtendido.set(false);
              } else {
                this.bairroNaoAtendido.set(true);
                this.freteLabel.set('Não atendemos');
              }
            }
          });

          setTimeout(() => document.getElementById('numeroInput')?.focus(), 100);
        } else {
          this.errorMessage.set('CEP não encontrado.');
        }
      },
      error: () => {
        this.buscandoCep.set(false);
        this.errorMessage.set('Erro ao buscar CEP.');
      }
    });
  }

  selecionarTipoPagamento(tipo: 'ONLINE' | 'ENTREGA') {
    this.tipoPagamento.set(tipo);

    if (tipo === 'ONLINE') {
      this.pedido.formaPagamento = this.PAGAMENTO_ENUM.MercadoPago;
      this.opcaoEntregaSelecionada.set('');
      this.trocoPara.set(''); // Limpa troco se for online
    } else {
      this.pedido.formaPagamento = this.PAGAMENTO_ENUM.NaoDefinido;
    }
  }

  selecionarOpcaoEntrega(opcaoUI: string) {
    this.opcaoEntregaSelecionada.set(opcaoUI);

    // Limpa troco se mudar para algo que não seja dinheiro
    if (opcaoUI !== 'Dinheiro') {
      this.trocoPara.set('');
    }

    switch (opcaoUI) {
      case 'Cartão de Crédito':
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.CartaoCredito;
        break;
      case 'Cartão de Débito':
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.CartaoDebito;
        break;
      case 'Dinheiro':
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.Dinheiro;
        break;
      default:
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.NaoDefinido;
    }
  }

  finalizarPedido() {
    if (!this.validar()) return;

    this.loading.set(true);
    this.errorMessage.set('');

    const itensReais = this.carrinhoService.itens().map(item => ({
      produtoId: item.produtoId,
      produtoSecundarioId: item.produtoSecundarioId || null,
      bordaId: item.bordaId || null,
      quantidade: item.quantidade,
      escolhasCombo: item.escolhasCombo || []
    }));

    // --- LÓGICA DO TROCO ---
    // Adiciona a informação do troco na observação para a cozinha ler
    let obsFinal = this.pedido.observacao || '';
    if (this.pedido.formaPagamento === this.PAGAMENTO_ENUM.Dinheiro && this.trocoPara()) {
      obsFinal += ` | (Precisa de troco para: R$ ${this.trocoPara()})`;
    }

    const payload = {
      ...this.pedido,
      observacao: obsFinal,
      numero: this.pedido.numero.toString(),
      valorFrete: this.carrinhoService.valorFrete(),
      valorTotal: this.carrinhoService.totalComFrete(),
      itens: itensReais
    };

    console.log('Payload Enviado:', payload);

    this.pedidoService.createPedido(payload).subscribe({
      next: (resp: any) => {
        this.loading.set(false);

        // --- CORREÇÃO DO ID ---
        // O seu controller retorna { pedido: { id: "..." }, paymentLink: "..." }
        // Então pegamos de resp.pedido.id
        const idGerado = resp.pedido?.id || resp.pedido?.Id;

        console.log("ID Recuperado:", idGerado); // Confira no console!

        this.carrinhoService.limpar();

        if (this.tipoPagamento() === 'ONLINE') {
          const url = resp.paymentLink || resp.data;
          if (url && typeof url === 'string' && url.startsWith('http')) {
            window.location.href = url;
          } else {
            // Se não gerou link, vai pra sucesso com o ID
            this.router.navigate(['/sucesso'], { state: { id: idGerado } });
          }
        } else {
          // Pagamento na Entrega: Vai pra sucesso com o ID
          this.router.navigate(['/sucesso'], { state: { id: idGerado } });
        }
      },
      error: (err: any) => {
        console.error('Erro Backend:', err);
        this.loading.set(false);

        if (err.error?.errors) {
          const keys = Object.keys(err.error.errors);
          const firstError = err.error.errors[keys[0]] ? err.error.errors[keys[0]][0] : 'Erro de validação.';
          this.errorMessage.set(`Erro: ${firstError}`);
        } else if (err.error?.title) {
          this.errorMessage.set(err.error.title);
        } else {
          this.errorMessage.set('Não foi possível realizar o pedido.');
        }
      }
    });
  }

  validar(): boolean {
    if (!this.pedido.nomeCliente || !this.pedido.telefoneCliente ||
      !this.pedido.cep || !this.pedido.numero || !this.pedido.logradouro) {
      this.errorMessage.set('Por favor, preencha todos os campos obrigatórios (*).');
      return false;
    }

    if (!this.pedido.formaPagamento || this.pedido.formaPagamento === 'NaoDefinido') {
      if (this.tipoPagamento() === 'ENTREGA') {
        this.errorMessage.set('Selecione como deseja pagar na entrega.');
      } else {
        this.errorMessage.set('Forma de pagamento inválida.');
      }
      return false;
    }

    return true;
  }
}