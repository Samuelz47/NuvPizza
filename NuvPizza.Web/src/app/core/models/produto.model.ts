export interface Produto {
  id: number;
  nome: string;
  descricao: string;
  preco: number;
  imagemUrl?: string;
  categoria: CategoriaProduto;
  tamanho: TamanhoProduto;
  ativo: boolean;
}

// Tem que bater com o Enum do C# (NuvPizza.Domain/Enums)
export enum CategoriaProduto {
  Pizza = 1,
  Bebida = 2,
  Combo = 3,
  Sobremesa = 4,
  Acompanhamento = 5
}

export enum TamanhoProduto {
  Unico = 0,
  Pequena = 1,
  Media = 2,
  Grande = 3,
  Gigante = 4
}