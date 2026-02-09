import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http'; 
import { PedidoService } from '../../core/services/pedido.service';
import { CarrinhoService } from '../../core/services/carrinho.service'; 

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
  
  // Controle do Modal
  mostrarModalSucesso = signal<boolean>(false);
  idPedidoCriado = signal<string | null>(null);

  // Controle de UI do Pagamento
  tipoPagamento = signal<'ONLINE' | 'ENTREGA'>('ONLINE'); 
  opcaoEntregaSelecionada = signal<string>('');

  // MAPEAMENTO STRINGS (Igual ao C#)
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
    emailCliente: '', // <--- NOVO CAMPO ADICIONADO
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

  // --- BOTÃO DE TESTE ---
  adicionarItemTeste() {
    this.carrinhoService.adicionar({
      id: 1, 
      nome: 'Pizza de Teste (Calabresa)',
      preco: 45.90,
      imagemUrl: ''
    });
  }

  // --- BUSCA CEP ---
  buscarCep() {
    const cep = this.pedido.cep?.replace(/\D/g, ''); 
    if (cep?.length !== 8) return;

    this.buscandoCep.set(true);
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (dados) => {
        this.buscandoCep.set(false);
        if (!dados.erro) {
          this.pedido.logradouro = dados.logradouro;
          this.pedido.bairro = dados.bairro;
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

  // --- SELEÇÃO DE PAGAMENTO ---
  selecionarTipoPagamento(tipo: 'ONLINE' | 'ENTREGA') {
    this.tipoPagamento.set(tipo);
    
    if (tipo === 'ONLINE') {
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.MercadoPago; 
        this.opcaoEntregaSelecionada.set('');
    } else {
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.NaoDefinido; 
    }
  }

  selecionarOpcaoEntrega(opcaoUI: string) {
      this.opcaoEntregaSelecionada.set(opcaoUI);
      
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

  // --- FINALIZAR PEDIDO ---
  finalizarPedido() {
    if (!this.validar()) return;

    this.loading.set(true);
    this.errorMessage.set('');

    const itensReais = this.carrinhoService.itens().map(item => ({
      produtoId: item.id,
      quantidade: item.quantidade,
      precoUnitario: item.preco 
    }));

    // Constrói o payload com todos os dados
    const payload = {
        ...this.pedido,
        // Garante que numero vai como string para evitar erro
        numero: this.pedido.numero.toString(), 
        valorFrete: this.carrinhoService.valorFrete(), 
        valorTotal: this.carrinhoService.totalComFrete(),
        itens: itensReais
    };

    console.log('Payload Enviado:', payload);

    this.pedidoService.createPedido(payload).subscribe({
      next: (resp: any) => {
        this.loading.set(false);
        
        const idGerado = resp.id || resp.Id || resp.numero;
        this.idPedidoCriado.set(idGerado);
        
        this.carrinhoService.limpar();

        if (this.tipoPagamento() === 'ONLINE') {
            const url = resp.paymentLink || resp.linkPagamento || resp.data;
            if (url) {
                window.location.href = url;
            } else {
                this.mostrarModalSucesso.set(true);
            }
        } else {
            this.mostrarModalSucesso.set(true);
        }
      },
      error: (err: any) => {
        console.error('Erro Backend:', err);
        this.loading.set(false);
        
        if (err.error?.errors) {
            // Pega o primeiro erro da lista
            const keys = Object.keys(err.error.errors);
            const firstError = err.error.errors[keys[0]][0];
            this.errorMessage.set(`Erro: ${firstError}`);
        } else if (err.error?.title) {
            this.errorMessage.set(err.error.title);
        } else {
            this.errorMessage.set('Não foi possível realizar o pedido. Verifique os dados.');
        }
      }
    });
  }

  validar(): boolean {
    // Adicionei emailCliente na validação
    if (!this.pedido.nomeCliente || !this.pedido.emailCliente || !this.pedido.telefoneCliente || 
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

  fecharModal() {
      this.mostrarModalSucesso.set(false);
      this.router.navigate(['/']); 
  }
}