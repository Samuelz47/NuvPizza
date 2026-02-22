import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProdutoService } from '../../core/services/produto.service';
import { CarrinhoService } from '../../core/services/carrinho.service';
import { LojaService, StatusLoja } from '../../core/services/loja.service';
import { Produto, CategoriaProduto, TamanhoProduto } from '../../core/models/produto.model';
import { CarrinhoFloatComponent } from '../../shared/components/carrinho-float/carrinho-float.component';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-cardapio',
  standalone: true,
  imports: [CommonModule, CarrinhoFloatComponent, FormsModule],
  templateUrl: './cardapio.html',
  styleUrls: ['./cardapio.css']
})
export class CardapioComponent implements OnInit {
  private produtoService = inject(ProdutoService);
  private carrinhoService = inject(CarrinhoService);
  private lojaService = inject(LojaService);
  private cdr = inject(ChangeDetectorRef);

  produtos: Produto[] = [];
  categoriaAtiva: CategoriaProduto = CategoriaProduto.Pizza;

  // Status da Loja
  lojaAberta = false;
  horaFechamento: string | null = null;

  // Helpers para o HTML
  categorias = [
    { id: CategoriaProduto.Pizza, nome: '🍕 Pizzas' },
    { id: CategoriaProduto.Bebida, nome: '🥤 Bebidas' },
    { id: CategoriaProduto.Combo, nome: '🍱 Combos' }
  ];

  // Modal
  modalAberto = false;
  modoMeioAMeio = false;
  saborPrincipal: Produto | null = null;
  saborSecundario: Produto | null = null;
  bordaSelecionada: Produto | null = null;

  listaSaboresCompativeis: Produto[] = []; // Para o 2º sabor

  // Modal de Combos
  modalComboAberto = false;
  comboSelecionado: Produto | null = null;
  // Guardamos as escolhas na mesma ordem dos templates do combo
  escolhasCombo: any[] = []; // { template: ComboItemTemplate, escolhido: Produto, secundario: Produto?, borda: Produto? }
  // Opcoes para cada slot para preencher os selects
  opcoesCombo: { [index: number]: Produto[] } = {};

  ngOnInit() {
    this.carregarProdutos();
    this.carregarStatusLoja();
  }

  carregarStatusLoja() {
    this.lojaService.getStatus().subscribe({
      next: (status) => {
        this.lojaAberta = status.estaAberta;
        if (status.dataHoraFechamento) {
          const dt = new Date(status.dataHoraFechamento);
          this.horaFechamento = dt.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
        } else {
          this.horaFechamento = null;
        }
        this.cdr.detectChanges();
      }
    });
  }

  carregarProdutos() {
    this.produtoService.getAll().subscribe(dados => {
      this.produtos = dados;
      this.cdr.detectChanges(); // Força o Angular a renderizar os produtos logo que eles chegam da API
    });
  }

  // Filtra na tela para não ficar fazendo request toda hora
  get produtosFiltrados() {
    return this.produtos.filter(p => p.ativo && p.categoria === this.categoriaAtiva);
  }

  // Lista de bordas para o modal da pizza (todos os acompanhamentos ativos)
  get listaBordas() {
    return this.produtos.filter(p =>
      p.categoria === CategoriaProduto.Acompanhamento &&
      p.ativo
    );
  }

  // Agrupa pizzas ativas por tamanho (ordem crescente de tamanho)
  get pizzasPorTamanho(): { tamanho: number; nome: string; pizzas: Produto[] }[] {
    const pizzasAtivas = this.produtos.filter(
      p => p.categoria === CategoriaProduto.Pizza && p.ativo
    );

    // Monta um mapa de tamanhoId -> lista de pizzas
    const mapa = new Map<number, Produto[]>();
    for (const pizza of pizzasAtivas) {
      if (!mapa.has(pizza.tamanho)) mapa.set(pizza.tamanho, []);
      mapa.get(pizza.tamanho)!.push(pizza);
    }

    // Ordena por tamanho (enum numérico crescente) e monta o resultado
    return Array.from(mapa.entries())
      .sort(([a], [b]) => a - b)
      .map(([tamanho, pizzas]) => ({
        tamanho,
        nome: this.getNomeTamanho(tamanho),
        pizzas
      }));
  }


  selecionarProduto(produto: Produto) {
    if (produto.categoria === CategoriaProduto.Pizza) {
      this.abrirModalPizza(produto);
    } else if (produto.categoria === CategoriaProduto.Combo) {
      this.abrirModalCombo(produto);
    } else {
      this.carrinhoService.adicionar(produto);
    }
  }

  // ---- LOGICA DO COMBO BUILDER ----
  abrirModalCombo(combo: Produto) {
    this.comboSelecionado = combo;
    this.escolhasCombo = [];
    this.opcoesCombo = {};

    // Configura escolhas baseadas nos templates e carrega os arrays de opcoes
    let templates = combo.comboTemplates || [];
    templates.forEach((template, index) => {
      // Filtra os produtos daquela categoria e tamanho obrigatorio
      // O backend manda Enum como string ("Pizza", "Bebida"). Precisamos converter ou buscar o id
      const catPermitidaStr = template.categoriaPermitida as unknown as string;
      const catId = typeof template.categoriaPermitida === 'number'
        ? template.categoriaPermitida
        : CategoriaProduto[catPermitidaStr as keyof typeof CategoriaProduto];

      const tamObrigatorioStr = template.tamanhoObrigatorio as unknown as string;
      const tamId = typeof template.tamanhoObrigatorio === 'number'
        ? template.tamanhoObrigatorio
        : TamanhoProduto[tamObrigatorioStr as keyof typeof TamanhoProduto];

      this.opcoesCombo[index] = this.produtos.filter(p =>
        p.ativo &&
        p.categoria === catId &&
        (tamId === TamanhoProduto.Unico || p.tamanho === tamId)
      );

      // Inicializa as escolhas de modelo vazias
      this.escolhasCombo.push({
        template: template,
        escolhido: null,
        meioAMeio: false, // se a categoria for pizza, user pode ligar meio a meio
        secundario: null,
        borda: null
      });
    });

    this.modalComboAberto = true;
    this.cdr.detectChanges(); // Força a atualização da view
  }

  confirmarCombo() {
    if (!this.comboSelecionado) return;

    // TODO: Verify if all are populated before submitting
    let allValid = true;
    for (let eq of this.escolhasCombo) {
      if (!eq.escolhido) {
        allValid = false;
      }
    }

    if (!allValid) {
      alert('Por favor, faça todas as escolhas do combo antes de adicionar ao carrinho.');
      return;
    }

    // Criar o payload complexo (String de representacao e o DTO pro carrinho)
    let precoBase = this.comboSelecionado.preco;
    let precoBordaTotal = 0;
    let bordaNomes: string[] = [];

    let comboItem = {
      id: `combo_${this.comboSelecionado.id}_${new Date().getTime()}`,
      produtoId: this.comboSelecionado.id,
      nome: this.comboSelecionado.nome,
      precoBase: precoBase,
      preco: precoBase, // vai acumular bordas
      imagem: this.comboSelecionado.imagemUrl,
      quantidade: 1,
      escolhasCombo: [] as any[]
    };

    // Para cada template adiciona valor extra de borda e adiciona na string de desc
    let subDescricoes: string[] = [];

    this.escolhasCombo.forEach(e => {
      let strName = e.escolhido.nome;
      if (e.secundario) {
        strName += ` / ${e.secundario.nome}`;
      }
      if (e.borda) {
        strName += ` (Borda: ${e.borda.nome})`;
        comboItem.preco += e.borda.preco;
        precoBordaTotal += e.borda.preco;
        bordaNomes.push(e.borda.nome);
      }

      subDescricoes.push(`+ ${strName}`);

      comboItem.escolhasCombo.push({
        comboItemTemplateId: e.template.id,
        produtoEscolhidoId: e.escolhido.id,
        produtoSecundarioId: e.secundario ? e.secundario.id : undefined,
        bordaId: e.borda ? e.borda.id : undefined
      });
    });

    // Anexar no nome (sem a borda, ela fica no campo separado)
    comboItem.nome += ' - ' + subDescricoes.join(', ');

    const comboItemFinal: any = {
      ...comboItem,
      nomeBorda: precoBordaTotal > 0 ? bordaNomes.join(' + ') : undefined,
      precoBorda: precoBordaTotal > 0 ? precoBordaTotal : undefined,
    };

    this.carrinhoService.adicionar(comboItemFinal);
    this.modalComboAberto = false;
  }
  // ---------------------------------

  abrirModalPizza(pizza: Produto) {
    this.saborPrincipal = pizza;
    this.saborSecundario = null;
    this.bordaSelecionada = null;
    this.modoMeioAMeio = false;

    // Busca pizzas do mesmo tamanho para ser o 2º sabor
    this.listaSaboresCompativeis = this.produtos.filter(p =>
      p.categoria === CategoriaProduto.Pizza &&
      p.tamanho === pizza.tamanho &&
      p.id !== pizza.id &&
      p.ativo
    );

    this.modalAberto = true;
  }

  confirmarPizza() {
    if (!this.saborPrincipal) return;

    let nomeFinal = `Pizza ${this.getNomeTamanho(this.saborPrincipal.tamanho)}: ${this.saborPrincipal.nome}`;
    let precoFinal = this.saborPrincipal.preco;
    let imgFinal = this.saborPrincipal.imagemUrl;

    const meioAMeioSelecionado = this.modoMeioAMeio && this.saborSecundario;
    const borda = this.bordaSelecionada;

    if (meioAMeioSelecionado) {
      nomeFinal += ` / ${this.saborSecundario!.nome}`;
      // Regra: Soma dos dois sabores dividido por 2
      precoFinal = (this.saborPrincipal.preco + this.saborSecundario!.preco) / 2;
    }

    if (borda) {
      nomeFinal += ` (Borda: ${borda.nome})`;
      precoFinal += borda.preco;
    }

    const idSec = meioAMeioSelecionado ? this.saborSecundario!.id : 0;
    const idBor = borda ? borda.id : 0;
    const strId = `${this.saborPrincipal.id}_${idSec}_${idBor}`;

    // Cria o objeto para o carrinho
    const item = {
      id: strId,
      produtoId: this.saborPrincipal.id,
      produtoSecundarioId: meioAMeioSelecionado ? this.saborSecundario!.id : undefined,
      bordaId: borda ? borda.id : undefined,
      nome: nomeFinal,
      precoBase: meioAMeioSelecionado
        ? (this.saborPrincipal.preco + this.saborSecundario!.preco) / 2
        : this.saborPrincipal.preco,
      preco: precoFinal,
      nomeBorda: borda ? borda.nome : undefined,
      precoBorda: borda ? borda.preco : undefined,
      imagem: imgFinal,
      quantidade: 1,
      observacao: meioAMeioSelecionado ? 'Meio a Meio' : ''
    };

    this.carrinhoService.adicionar(item);
    this.modalAberto = false;
  }

  getNomeTamanho(t: any): string {
    if (typeof t === 'number') return TamanhoProduto[t] || '';
    return t || '';
  }

  getNomeCategoria(c: any): string {
    const val = typeof c === 'number' ? CategoriaProduto[c] : c;
    return val || '';
  }

  getTamanhoTemplate(t: any): string {
    const val = typeof t === 'number' ? TamanhoProduto[t] : t;
    if (!val || val === 'Unico') return '';
    return val;
  }

  getImagemUrl(imagemUrl: string | undefined): string {
    if (!imagemUrl) return 'assets/logo.png';
    if (imagemUrl.startsWith('http')) return imagemUrl;

    // Se for da pasta assets (local), não usa o apiUrl do backend
    if (imagemUrl.startsWith('assets/')) {
      return imagemUrl; // Removida a barra do início para não quebrar a rota no Angular
    }

    const cleanUrl = imagemUrl.startsWith('/') ? imagemUrl.substring(1) : imagemUrl;
    return `${environment.apiUrl}/${cleanUrl}`;
  }
}