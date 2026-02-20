import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProdutoService } from '../../core/services/produto.service';
import { CarrinhoService } from '../../core/services/carrinho.service';
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
  private cdr = inject(ChangeDetectorRef);

  produtos: Produto[] = [];
  categoriaAtiva: CategoriaProduto = CategoriaProduto.Pizza;

  // Helpers para o HTML
  categorias = [
    { id: CategoriaProduto.Pizza, nome: '🍕 Pizzas' },
    { id: CategoriaProduto.Bebida, nome: '🥤 Bebidas' },
    { id: CategoriaProduto.Combo, nome: '🍱 Combos' },
    { id: CategoriaProduto.Sobremesa, nome: '🍰 Sobremesas' }
  ];

  // Modal
  modalAberto = false;
  modoMeioAMeio = false;
  saborPrincipal: Produto | null = null;
  saborSecundario: Produto | null = null;

  listaSaboresCompativeis: Produto[] = []; // Para o 2º sabor

  ngOnInit() {
    this.carregarProdutos();
  }

  carregarProdutos() {
    this.produtoService.getAll().subscribe(dados => {
      this.produtos = dados;
      this.cdr.detectChanges(); // Força o Angular a renderizar os produtos logo que eles chegam da API
    });
  }

  // Filtra na tela para não ficar fazendo request toda hora
  get produtosFiltrados() {
    return this.produtos.filter(p => p.categoria === this.categoriaAtiva && p.ativo);
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
    } else {
      this.carrinhoService.adicionar(produto);
    }
  }

  abrirModalPizza(pizza: Produto) {
    this.saborPrincipal = pizza;
    this.saborSecundario = null;
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

    if (meioAMeioSelecionado) {
      nomeFinal += ` / ${this.saborSecundario!.nome}`;
      // Regra: Cobra pela maior
      if (this.saborSecundario!.preco > precoFinal) {
        precoFinal = this.saborSecundario!.preco;
      }
    }

    // Cria o objeto para o carrinho
    const item = {
      id: meioAMeioSelecionado ? 'custom-' + Date.now() : this.saborPrincipal.id, // ID único para meio a meio
      nome: nomeFinal,
      preco: precoFinal,
      imagem: imgFinal,
      quantidade: 1,
      observacao: meioAMeioSelecionado ? 'Meio a Meio' : ''
    };

    this.carrinhoService.adicionar(item);
    this.modalAberto = false;
  }

  getNomeTamanho(t: number): string {
    return TamanhoProduto[t] || '';
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