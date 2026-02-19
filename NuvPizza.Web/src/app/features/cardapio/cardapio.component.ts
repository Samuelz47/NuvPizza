import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProdutoService } from '../../core/services/produto.service';
import { CarrinhoService } from '../../core/services/carrinho.service';
import { Produto, CategoriaProduto, TamanhoProduto } from '../../core/models/produto.model';
import { CarrinhoFloatComponent } from '../../shared/components/carrinho-float/carrinho-float.component';

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

  produtos: Produto[] = [];
  categoriaAtiva: CategoriaProduto = CategoriaProduto.Pizza;
  
  // Helpers para o HTML
  categorias = [
    { id: CategoriaProduto.Pizza, nome: 'Pizzas' },
    { id: CategoriaProduto.Bebida, nome: 'Bebidas' },
    { id: CategoriaProduto.Combo, nome: 'Combos' }
  ];

  // Modal
  modalAberto = false;
  saborPrincipal: Produto | null = null;
  saborSecundario: Produto | null = null;
  
  listaSaboresCompativeis: Produto[] = []; // Para o 2º sabor

  ngOnInit() {
    this.carregarProdutos();
  }

  carregarProdutos() {
    this.produtoService.getAll().subscribe(dados => {
      this.produtos = dados;
    });
  }

  // Filtra na tela para não ficar fazendo request toda hora
  get produtosFiltrados() {
    return this.produtos.filter(p => p.categoria === this.categoriaAtiva);
  }

  selecionarProduto(produto: Produto) {
    if (produto.categoria === CategoriaProduto.Pizza) {
      // Abre o modal para personalizar
      this.abrirModalPizza(produto);
    } else {
      // Bebida adiciona direto
      this.carrinhoService.adicionar(produto);
      // Feedback visual simples (opcional)
      alert('Adicionado!'); 
    }
  }

  abrirModalPizza(pizza: Produto) {
    this.saborPrincipal = pizza;
    this.saborSecundario = null;
    
    // Busca pizzas do mesmo tamanho para ser o 2º sabor
    this.listaSaboresCompativeis = this.produtos.filter(p => 
      p.categoria === CategoriaProduto.Pizza &&
      p.tamanho === pizza.tamanho &&
      p.id !== pizza.id
    );

    this.modalAberto = true;
  }

  confirmarPizza() {
    if (!this.saborPrincipal) return;

    let nomeFinal = `Pizza ${this.getNomeTamanho(this.saborPrincipal.tamanho)}: ${this.saborPrincipal.nome}`;
    let precoFinal = this.saborPrincipal.preco;
    let imgFinal = this.saborPrincipal.imagemUrl;

    if (this.saborSecundario) {
      nomeFinal += ` / ${this.saborSecundario.nome}`;
      // Regra: Cobra pela maior
      if (this.saborSecundario.preco > precoFinal) {
        precoFinal = this.saborSecundario.preco;
      }
    }

    // Cria o objeto para o carrinho
    const item = {
      id: this.saborPrincipal.id, // ID referência
      nome: nomeFinal,
      preco: precoFinal,
      imagem: imgFinal,
      quantidade: 1,
      observacao: this.saborSecundario ? 'Meio a Meio' : ''
    };

    this.carrinhoService.adicionar(item);
    this.modalAberto = false;
  }

  getNomeTamanho(t: number): string {
    return TamanhoProduto[t] || '';
  }
}