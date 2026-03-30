import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { PedidoService } from '../../core/services/pedido.service';
import { CupomService } from '../../core/services/cupom.service';
import { CarrinhoService } from '../../core/services/carrinho.service';
import { LojaService } from '../../core/services/loja.service';
import { BairroService, Bairro } from '../../core/services/bairro.service';
import { environment } from '../../environments/environment';
import { Cupom } from '../../core/models/cupom.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css']
})
export class CheckoutComponent {
  private pedidoService = inject(PedidoService);
  private cupomService = inject(CupomService);
  public carrinhoService = inject(CarrinhoService);
  private lojaService = inject(LojaService);
  private bairroService = inject(BairroService);
  private router = inject(Router);
  private http = inject(HttpClient);

  // Sinais de Estado
  loading = signal<boolean>(false);
  errorMessage = signal<string>('');
  buscandoTelefone = signal<boolean>(false);
  buscandoCep = signal<boolean>(false);
  bairroNaoAtendido = signal<boolean>(false);
  bairroEncontrado = signal<boolean>(false);
  freteLabel = signal<string>('---');
  lojaAberta = signal<boolean>(true);
  bairros = signal<Bairro[]>([]);

  // Popup Telefone Inicial
  mostrarPopupTelefone = signal<boolean>(true);
  telefonePopup = signal<string>('');
  buscandoTelefonePopup = signal<boolean>(false);

  idPedidoCriado = signal<string | null>(null);

  // Sinais do Cupom
  codigoCupomInput = signal<string>('');
  cupomAplicado = signal<Cupom | null>(null);
  cupomErro = signal<string>('');
  aplicandoCupom = signal<boolean>(false);
  freteOriginal = signal<number>(0); // guarda frete antes do cupom
  isRetirada = signal<boolean>(false);

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
    bairroNome: '',
    pontoReferencia: '',
    isRetirada: false,
    observacao: '',
    formaPagamento: 'NaoDefinido',
    itens: [],
    cupom: ''
  };

  constructor() {
    this.selecionarTipoPagamento('ONLINE');
  }

  ngOnInit() {
    this.lojaService.getStatus().subscribe({
      next: (status) => {
        this.lojaAberta.set(status.estaAberta);
      }
    });

    this.bairroService.getBairros().subscribe({
      next: (data) => {
        this.bairros.set(data);
      }
    });
  }

  buscarCep() {
    const cep = this.pedido.cep?.replace(/\D/g, '');
    if (cep?.length !== 8) return;

    this.buscandoCep.set(true);
    this.bairroNaoAtendido.set(false);
    this.bairroEncontrado.set(false);
    this.freteLabel.set('---');
    this.carrinhoService.valorFrete.set(0);

    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (dados) => {
        this.buscandoCep.set(false);
        if (!dados.erro) {
          this.pedido.logradouro = dados.logradouro;
          this.pedido.bairroNome = dados.bairro;

          this.onBairroChange(); // Reaproveita lógica de validação/frete

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

  formatarTelefone(event: Event, isPopup: boolean = false) {
    const input = event.target as HTMLInputElement;
    let digits = input.value.replace(/\D/g, '').substring(0, 11);
    let formatted = '';

    if (digits.length > 0) formatted = '(' + digits.substring(0, 2);
    if (digits.length >= 2) formatted += ') ';
    if (digits.length >= 3) {
      if (digits.length <= 6) {
        formatted += digits.substring(2);
      } else if (digits.length <= 10) {
        formatted += digits.substring(2, 6) + '-' + digits.substring(6);
      } else {
        formatted += digits.substring(2, 7) + '-' + digits.substring(7);
      }
    }

    input.value = formatted;
    if (isPopup) {
      this.telefonePopup.set(formatted);
    } else {
      this.pedido.telefoneCliente = formatted;
    }
  }

  fecharPopupTelefone() {
    this.mostrarPopupTelefone.set(false);
  }

  confirmarTelefonePopup() {
    const apenasNumeros = this.telefonePopup().replace(/\D/g, '');

    if (apenasNumeros.length >= 10) {
      this.buscandoTelefonePopup.set(true);

      this.pedidoService.getUltimoEnderecoPorTelefone(apenasNumeros).subscribe({
        next: (dados) => {
          this.buscandoTelefonePopup.set(false);
          this.mostrarPopupTelefone.set(false);

          if (dados) {
            this.pedido.telefoneCliente = this.telefonePopup(); // Mantém a digitação original
            if (!this.pedido.nomeCliente) this.pedido.nomeCliente = dados.nomeCliente;
            if (!this.pedido.emailCliente) this.pedido.emailCliente = dados.emailCliente;
            if (!this.pedido.logradouro) this.pedido.logradouro = dados.logradouro;
            if (!this.pedido.numero) this.pedido.numero = dados.numero;
            if (!this.pedido.bairroNome) this.pedido.bairroNome = dados.bairroNome;
            if (!this.pedido.pontoReferencia) this.pedido.pontoReferencia = dados.pontoReferencia;
            if (!this.pedido.cep) this.pedido.cep = dados.cep;

            if (this.pedido.bairroNome) {
              this.onBairroChange();
            }
          } else {
            this.pedido.telefoneCliente = this.telefonePopup();
          }
        },
        error: () => {
          this.buscandoTelefonePopup.set(false);
          this.mostrarPopupTelefone.set(false);
          this.pedido.telefoneCliente = this.telefonePopup();
        }
      });
    } else {
      this.pedido.telefoneCliente = this.telefonePopup();
      this.mostrarPopupTelefone.set(false);
    }
  }

  onTelefoneInput() {
    const telefoneBruto = this.pedido.telefoneCliente || '';
    const apenasNumeros = telefoneBruto.replace(/\D/g, '');

    if (apenasNumeros.length === 11) {
      this.buscandoTelefone.set(true);
      this.pedidoService.getUltimoEnderecoPorTelefone(apenasNumeros).subscribe({
        next: (dados) => {
          this.buscandoTelefone.set(false);
          if (dados) {
            // Só preenche se os campos estiverem vazios (para não sobrescrever se o cliente já começou a digitar)
            if (!this.pedido.nomeCliente) this.pedido.nomeCliente = dados.nomeCliente;
            if (!this.pedido.emailCliente) this.pedido.emailCliente = dados.emailCliente;
            if (!this.pedido.logradouro) this.pedido.logradouro = dados.logradouro;
            if (!this.pedido.numero) this.pedido.numero = dados.numero;
            if (!this.pedido.bairroNome) this.pedido.bairroNome = dados.bairroNome;
            if (!this.pedido.pontoReferencia) this.pedido.pontoReferencia = dados.pontoReferencia;
            if (!this.pedido.cep) this.pedido.cep = dados.cep;

            // Dispara validação de bairro para calcular frete
            if (this.pedido.bairroNome) {
              this.onBairroChange();
            }
          }
        },
        error: () => {
          this.buscandoTelefone.set(false);
          // Não mostramos erro se não encontrar, pois pode ser um cliente novo
        }
      });
    }
  }

  onBairroChange() {
    if (this.isRetirada()) {
      this.carrinhoService.valorFrete.set(0);
      this.bairroEncontrado.set(true);
      this.bairroNaoAtendido.set(false);
      this.freteLabel.set('Grátis (Retirada)');
      return;
    }

    const bairroNome = (this.pedido.bairroNome || '').toLowerCase().trim();
    const encontrado = this.bairros().find(
      b => b.nome.toLowerCase().trim() === bairroNome
    );

    if (encontrado) {
      this.carrinhoService.valorFrete.set(encontrado.valorFrete);
      this.bairroEncontrado.set(true);
      this.bairroNaoAtendido.set(false);
      if (encontrado.valorFrete === 0) {
        this.freteLabel.set('Grátis 🎉');
      } else {
        this.freteLabel.set(`R$ ${encontrado.valorFrete.toFixed(2).replace('.', ',')}`);
      }
    } else {
      this.bairroEncontrado.set(false);
      this.bairroNaoAtendido.set(true);
      this.freteLabel.set('Não atendemos');
      this.carrinhoService.valorFrete.set(0);
    }
  }

  selecionarTipoEntrega(tipo: 'ENTREGA' | 'RETIRADA') {
    const isRetirada = tipo === 'RETIRADA';
    this.isRetirada.set(isRetirada);
    this.pedido.isRetirada = isRetirada;

    if (isRetirada) {
      this.pedido.bairroNome = '';
      this.pedido.logradouro = '';
      this.pedido.numero = '';
      this.pedido.pontoReferencia = '';
      this.onBairroChange();
    } else {
      this.freteLabel.set('---');
    }
  }

  // ---- LÓGICA DO CUPOM ----

  aplicarCupom() {
    const codigo = this.codigoCupomInput().trim().toUpperCase();
    if (!codigo) return;

    this.aplicandoCupom.set(true);
    this.cupomErro.set('');

    const telefone = this.pedido.telefoneCliente?.replace(/\D/g, '') || '';
    this.cupomService.getCupomPorCodigo(codigo, telefone).subscribe({
      next: (cupom) => {
        console.log('[DEBUG] Cupom recebido:', cupom);
        this.aplicandoCupom.set(false);
        if (!cupom.ativo) {
          this.cupomErro.set('Este cupom está inativo.');
          return;
        }

        // Validação de Pedido Mínimo do Cupom
        const subtotal = this.carrinhoService.valorTotal();
        if (cupom.pedidoMinimo > 0 && subtotal < cupom.pedidoMinimo) {
          this.cupomErro.set(`Este cupom exige um pedido mínimo de R$ ${cupom.pedidoMinimo.toFixed(2).replace('.', ',')}`);
          return;
        }

        this.cupomAplicado.set(cupom);
        this.pedido.cupom = cupom.codigo;

        // Aplica frete grátis visualmente se necessário
        if (cupom.freteGratis) {
          this.freteOriginal.set(this.carrinhoService.valorFrete());
          this.carrinhoService.valorFrete.set(0);
          this.freteLabel.set('Grátis 🎉 (cupom)');
        }

        // O desconto em % é calculado no backend e também refletido no totalComFrete
        // Aqui apenas guardamos o cupom; cálculo visual é feito via getter
      },
      error: () => {
        this.aplicandoCupom.set(false);
        this.cupomErro.set('Cupom inválido ou não encontrado.');
      }
    });
  }

  removerCupom() {
    // Restaura frete original se cupom tinha frete grátis
    const cupom = this.cupomAplicado();
    if (cupom?.freteGratis) {
      this.carrinhoService.valorFrete.set(this.freteOriginal());
      // Restaura label de frete
      const frete = this.freteOriginal();
      this.freteLabel.set(frete === 0 ? 'Grátis 🎉' : `R$ ${frete.toFixed(2).replace('.', ',')}`);
    }
    this.cupomAplicado.set(null);
    this.codigoCupomInput.set('');
    this.cupomErro.set('');
    this.pedido.cupom = '';
    this.freteOriginal.set(0);
  }

  // Subtotal com desconto aplicado (apenas visual; o backend também verifica)
  get subtotalComDesconto(): number {
    const subtotal = this.carrinhoService.valorTotal();
    const cupom = this.cupomAplicado();
    if (!cupom || cupom.descontoPorcentagem <= 0) return subtotal;
    return subtotal * (1 - cupom.descontoPorcentagem / 100);
  }

  get totalFinalComDesconto(): number {
    return this.subtotalComDesconto + this.carrinhoService.valorFrete();
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
      case 'Pix':
        this.pedido.formaPagamento = this.PAGAMENTO_ENUM.Pix;
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

    // Tenta converter o número para int, se não for número válido manda 0 para evitar quebra no C#
    const numeroStr = this.pedido.numero?.toString() || '0';
    const numeroInt = parseInt(numeroStr, 10);
    const numeroFinal = isNaN(numeroInt) ? 0 : numeroInt;

    const payload = {
      ...this.pedido,
      telefoneCliente: this.pedido.telefoneCliente?.replace(/\D/g, ''),
      observacao: obsFinal,
      numero: numeroFinal,
      valorFrete: this.carrinhoService.valorFrete(),
      valorTotal: this.totalFinalComDesconto,
      itens: itensReais,
      codigoCupom: this.pedido.cupom || null
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
            // Se não gerou link (Erro de token do Mercado Pago, etc)
            this.errorMessage.set('Ops! Ocorreu um erro ao gerar o link de pagamento do Mercado Pago. Verifique as credenciais da loja.');
          }
        } else {
          // Pagamento na Entrega: Vai pra sucesso com o ID
          this.router.navigate(['/sucesso'], { state: { id: idGerado } });
        }
      },
      error: (err: any) => {
        console.error('Erro Backend RAW:', err);
        console.error('Erro Backend Body:', JSON.stringify(err.error, null, 2));
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
    if (!this.pedido.nomeCliente || !this.pedido.telefoneCliente) {
      this.errorMessage.set('Nome e telefone são obrigatórios.');
      return false;
    }

    if (!this.isRetirada()) {
      if (!this.pedido.bairroNome || !this.pedido.logradouro || !this.pedido.numero) {
        this.errorMessage.set('Para entrega, informe bairro, rua e número.');
        return false;
      }
    }

    if (!this.pedido.formaPagamento || this.pedido.formaPagamento === 'NaoDefinido') {
      if (this.tipoPagamento() === 'ENTREGA') {
        this.errorMessage.set('Selecione como deseja pagar na entrega.');
      } else {
        this.errorMessage.set('Forma de pagamento inválida.');
      }
      return false;
    }

    if (this.carrinhoService.valorTotal() < 20) {
      this.errorMessage.set('O valor mínimo para pedidos é de R$ 20,00 (subtotal).');
      return false;
    }

    return true;
  }
}