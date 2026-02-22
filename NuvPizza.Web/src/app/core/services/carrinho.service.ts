import { Injectable, signal, computed } from '@angular/core';

export interface ItemCarrinho {
  id: string; // unique internal id for grouping identical setups
  produtoId: number;
  produtoSecundarioId?: number;
  bordaId?: number;
  nome: string;
  /** Preço do item SEM a borda */
  precoBase: number;
  /** Preço total (precoBase + precoBorda) */
  preco: number;
  nomeBorda?: string;
  precoBorda?: number;
  quantidade: number;
  imagem?: string;
  observacao?: string;
  escolhasCombo?: any[];
}

@Injectable({
  providedIn: 'root'
})
export class CarrinhoService {
  private key = 'nuvpizza_carrinho';

  // Lista de Itens (Carrega do localStorage ao iniciar)
  itens = signal<ItemCarrinho[]>(this.carregarDoStorage());

  // --- NOVAS PROPRIEDADES NECESSÁRIAS PARA O CHECKOUT ---

  // 1. O valor do frete (Pode ser atualizado via signal)
  valorFrete = signal<number>(0);

  // 2. Total dos Produtos (Soma de preço * quantidade)
  valorTotal = computed<number>(() => {
    return this.itens().reduce((acc, item) => acc + (item.preco * item.quantidade), 0);
  });

  // 3. Total Final (Produtos + Frete)
  totalComFrete = computed<number>(() => {
    return this.valorTotal() + this.valorFrete();
  });

  // 4. Quantidade de itens (para o badge do ícone)
  quantidadeTotal = computed<number>(() => {
    return this.itens().reduce((acc, item) => acc + item.quantidade, 0);
  });

  constructor() { }

  // --- AÇÕES DO CARRINHO ---

  adicionar(produto: any) {
    const listaAtual = this.itens();
    const itemExistente = listaAtual.find(i => i.id === produto.id);

    if (itemExistente) {
      // Se já existe, só aumenta a quantidade
      const novaLista = listaAtual.map(i =>
        i.id === produto.id ? { ...i, quantidade: i.quantidade + 1 } : i
      );
      this.itens.set(novaLista);
    } else {
      // Se não existe, cria um novo
      const novoItem: ItemCarrinho = {
        id: produto.id,
        produtoId: produto.produtoId || produto.id,
        produtoSecundarioId: produto.produtoSecundarioId,
        bordaId: produto.bordaId,
        nome: produto.nome,
        precoBase: produto.precoBase ?? produto.preco,
        preco: produto.preco,
        nomeBorda: produto.nomeBorda,
        precoBorda: produto.precoBorda,
        quantidade: 1,
        imagem: produto.imagemUrl || produto.imagem,
        observacao: produto.observacao || '',
        escolhasCombo: produto.escolhasCombo || undefined
      };
      this.itens.update(lista => [...lista, novoItem]);
    }
    this.salvarNoStorage(this.itens());
  }

  remover(id: string) {
    this.itens.update(lista => lista.filter(i => i.id !== id));
    this.salvarNoStorage(this.itens());
  }

  decrementar(id: string) {
    const listaAtual = this.itens();
    const item = listaAtual.find(i => i.id === id);

    if (item) {
      if (item.quantidade > 1) {
        const novaLista = listaAtual.map(i =>
          i.id === id ? { ...i, quantidade: i.quantidade - 1 } : i
        );
        this.itens.set(novaLista);
        this.salvarNoStorage(novaLista);
      } else {
        this.remover(id);
      }
    }
  }

  limpar() {
    this.itens.set([]);
    this.valorFrete.set(0); // Reseta o frete também
    localStorage.removeItem(this.key);
  }

  // --- PERSISTÊNCIA (LOCALSTORAGE) ---

  private carregarDoStorage(): ItemCarrinho[] {
    if (typeof localStorage !== 'undefined') {
      const salvo = localStorage.getItem(this.key);
      if (!salvo) return [];
      const itens: ItemCarrinho[] = JSON.parse(salvo);
      // Retrocompatibilidade: itens salvos antes do campo precoBase não o terão
      return itens.map(i => ({ ...i, precoBase: i.precoBase ?? i.preco }));
    }
    return [];
  }

  private salvarNoStorage(itens: ItemCarrinho[]) {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.key, JSON.stringify(itens));
    }
  }
}