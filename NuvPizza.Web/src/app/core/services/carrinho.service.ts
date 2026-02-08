import { Injectable, signal, computed } from '@angular/core';

export interface ItemCarrinho {
  id: number;
  nome: string;
  preco: number;
  quantidade: number;
  imagem?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CarrinhoService {
  private key = 'nuvpizza_carrinho';
  
  itens = signal<ItemCarrinho[]>(this.carregarDoStorage());

  // A CORREÇÃO ESTÁ AQUI: Adicione <number> depois de computed
  valorTotal = computed<number>(() => {
    return this.itens().reduce((acc, item) => acc + (item.preco * item.quantidade), 0);
  });
  
  quantidadeTotal = computed<number>(() => {
    return this.itens().reduce((acc, item) => acc + item.quantidade, 0);
  });

  constructor() {}

  // ... (o restante do código continua igual: adicionar, remover, etc.)
  
  adicionar(produto: any) {
    const listaAtual = this.itens();
    const itemExistente = listaAtual.find(i => i.id === produto.id);

    if (itemExistente) {
      const novaLista = listaAtual.map(i => 
        i.id === produto.id ? { ...i, quantidade: i.quantidade + 1 } : i
      );
      this.itens.set(novaLista);
    } else {
      const novoItem: ItemCarrinho = {
        id: produto.id,
        nome: produto.nome,
        preco: produto.preco,
        quantidade: 1,
        imagem: produto.imagemUrl 
      };
      this.itens.update(lista => [...lista, novoItem]);
    }
    this.salvarNoStorage(this.itens());
  }

  remover(id: number) {
    this.itens.update(lista => lista.filter(i => i.id !== id));
    this.salvarNoStorage(this.itens());
  }

  decrementar(id: number) {
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
    localStorage.removeItem(this.key);
  }

  private carregarDoStorage(): ItemCarrinho[] {
    const salvo = localStorage.getItem(this.key);
    return salvo ? JSON.parse(salvo) : [];
  }

  private salvarNoStorage(itens: ItemCarrinho[]) {
    localStorage.setItem(this.key, JSON.stringify(itens));
  }
}