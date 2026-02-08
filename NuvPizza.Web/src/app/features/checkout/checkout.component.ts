import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router'; 
import { HttpClient } from '@angular/common/http'; 
import { PedidoService } from '../../core/services/pedido.service';
import { CarrinhoService } from '../../core/services/carrinho.service'; 

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.html',
  styles: [`
    /* --- DESIGN PREMIUM CSS --- */
    .checkout-bg { 
      background-color: #e3f2fd; /* Fundo cinza bem clarinho e moderno */
      min-height: 100vh; 
      padding: 3rem 1rem; 
      display: flex; 
      justify-content: center; 
      font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
    }
    
    .checkout-container {
      max-width: 1100px;
      width: 100%;
    }

    .card-custom {
      background: #fff;
      border: none;
      border-radius: 16px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.06); /* Sombra suave */
      padding: 2rem;
      margin-bottom: 20px;
    }

    h2 { font-weight: 800; color: #2c3e50; letter-spacing: -0.5px; }
    h3 { font-weight: 700; color: #34495e; font-size: 1.3rem; margin-bottom: 1.5rem; }

    .form-label {
      font-weight: 600;
      color: #58687a;
      font-size: 0.9rem;
      margin-bottom: 6px;
    }

    .form-control {
      border: 2px solid #eaeff4;
      border-radius: 10px;
      padding: 12px;
      font-size: 1rem;
      transition: all 0.3s ease;
    }

    .form-control:focus {
      border-color: #009ee3;
      box-shadow: 0 0 0 4px rgba(0, 158, 227, 0.1);
    }

    .input-readonly { 
      background-color: #f8f9fa; 
      color: #6c757d; 
      border-color: #f0f0f0; 
      cursor: default;
    }

    /* Input com Ícone/Spinner */
    .input-icon-group { position: relative; }
    .input-icon-right {
      position: absolute;
      right: 15px;
      top: 50%;
      transform: translateY(-50%);
      color: #009ee3;
    }

    /* Opções de Pagamento Estilizadas */
    .payment-option {
      border: 2px solid #eaeff4;
      border-radius: 12px;
      padding: 20px;
      cursor: pointer;
      transition: all 0.2s;
      background: #fff;
      position: relative;
      overflow: hidden;
    }

    .payment-option:hover {
      border-color: #b3e0f5;
      transform: translateY(-2px);
    }

    .payment-option.selected {
      border-color: #009ee3;
      background-color: #f0f9ff;
    }

    .payment-option.selected::after {
      content: '✔';
      position: absolute;
      top: 10px;
      right: 10px;
      color: #009ee3;
      font-weight: bold;
    }

    /* Resumo do Pedido */
    .summary-item {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px dashed #eee;
      color: #555;
    }
    
    .total-row {
      display: flex;
      justify-content: space-between;
      margin-top: 20px;
      font-size: 1.4rem;
      font-weight: 800;
      color: #2c3e50;
    }

    .btn-pagar {
      width: 100%;
      padding: 18px;
      background: linear-gradient(135deg, #009ee3 0%, #0077b5 100%);
      color: white;
      border: none;
      border-radius: 50px;
      font-weight: 700;
      font-size: 1.1rem;
      cursor: pointer;
      box-shadow: 0 10px 20px rgba(0, 158, 227, 0.25);
      transition: transform 0.2s, box-shadow 0.2s;
    }

    .btn-pagar:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 15px 25px rgba(0, 158, 227, 0.35);
    }

    .btn-pagar:disabled {
      background: #bdc3c7;
      box-shadow: none;
      cursor: not-allowed;
    }

    /* Botão de Teste */
    .btn-teste {
      background: #fff3cd;
      color: #856404;
      border: 1px solid #ffeeba;
      font-weight: bold;
      padding: 8px 15px;
      border-radius: 20px;
      font-size: 0.85rem;
      display: inline-flex;
      align-items: center;
      gap: 5px;
      margin-bottom: 20px;
      cursor: pointer;
    }
  `]
})
export class CheckoutComponent {
  private pedidoService = inject(PedidoService);
  public carrinhoService = inject(CarrinhoService);
  private router = inject(Router);
  private http = inject(HttpClient); 

  loading = signal<boolean>(false);
  errorMessage = signal<string>('');
  
  buscandoCep = signal<boolean>(false);
  tipoPagamento = signal<'ONLINE' | 'ENTREGA'>('ONLINE'); 

  pedido: any = {
    nomeCliente: '',
    emailCliente: 'cliente@teste.com',
    telefoneCliente: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    observacao: '',
    formaPagamento: 'Pix', 
    itens: [] 
  };

  // --- BOTÃO DE TESTE ---
  adicionarItemTeste() {
    this.carrinhoService.adicionar({
      id: 1, // <--- GARANTA QUE ESSE ID EXISTE NO SEU BANCO
      nome: 'Pizza Calabresa (Teste)',
      preco: 45.90,
      imagemUrl: ''
    });
  }
  // ----------------------

  constructor() {
    // Se quiser bloquear acesso sem itens, descomente:
    // if (this.carrinhoService.quantidadeTotal() === 0) { this.router.navigate(['/']); }
  }

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
          // Foca no número automaticamente
          setTimeout(() => document.getElementById('numeroInput')?.focus(), 100);
        } else {
          this.pedido.logradouro = '';
          this.pedido.bairro = '';
          this.errorMessage.set('CEP não encontrado.');
        }
      },
      error: () => {
        this.buscandoCep.set(false);
        this.errorMessage.set('Erro ao buscar CEP.');
      }
    });
  }

  selecionarPagamento(tipo: 'ONLINE' | 'ENTREGA') {
    this.tipoPagamento.set(tipo);
    // Mapeia para o que o Backend espera (Enum ou String tratada)
    if (tipo === 'ONLINE') {
      this.pedido.formaPagamento = 'Pix'; 
    } else {
      this.pedido.formaPagamento = 'Pagar na Entrega'; 
    }
  }

  finalizarPedido() {
    if (!this.validar()) return;

    this.loading.set(true);
    this.errorMessage.set('');

    const itensReais = this.carrinhoService.itens().map(item => ({
      produtoId: item.id,
      quantidade: item.quantidade
    }));

    const payload = {
        ...this.pedido,
        numero: Number(this.pedido.numero),
        itens: itensReais
    };

    this.pedidoService.createPedido(payload).subscribe({
      next: (resp: any) => {
        this.carrinhoService.limpar();

        if (this.tipoPagamento() === 'ONLINE') {
            const url = resp.paymentLink || resp.linkPagamento || resp.data;
            if (url) {
                window.location.href = url;
            } else {
                this.router.navigate(['/sucesso']); 
            }
        } else {
            this.router.navigate(['/sucesso']);
        }
      },
      error: (err: any) => {
        console.error(err);
        this.loading.set(false);
        
        // Tratamento de erro mais bonito
        if (err.error?.errors) {
            const msg = Object.values(err.error.errors)[0] as string[];
            this.errorMessage.set(msg[0]);
        } else {
            this.errorMessage.set('Não foi possível realizar o pedido. Tente novamente.');
        }
      }
    });
  }

  validar(): boolean {
    if (!this.pedido.nomeCliente || !this.pedido.telefoneCliente || !this.pedido.cep || !this.pedido.numero || !this.pedido.logradouro) {
      this.errorMessage.set('Por favor, preencha todos os campos obrigatórios (*).');
      return false;
    }
    return true;
  }
}