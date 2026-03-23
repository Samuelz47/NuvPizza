import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // Importante para o ngModel
import { Router } from '@angular/router';
import { ProdutoService } from '../../../core/services/produto.service';
import { Produto, CategoriaProduto, TamanhoProduto } from '../../../core/models/produto.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-gerenciar-produtos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './gerenciar-produtos.html',
  styleUrls: ['./gerenciar-produtos.css']
})
export class GerenciarProdutosComponent implements OnInit {
  private produtoService = inject(ProdutoService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  produtos: Produto[] = [];

  // Controle do Modal
  exibindoFormulario = false;
  produtoEmEdicao: any = {}; // Objeto temporário para o form
  arquivoSelecionado: File | null = null;
  isLoading = false;

  // Enums para o select do HTML
  categorias = [
    { id: 1, nome: 'Pizza' }, { id: 2, nome: 'Bebida' }, { id: 3, nome: 'Combo' }, { id: 4, nome: 'Sobremesa' }, { id: 5, nome: 'Acompanhamento' }
  ];
  tamanhos = [
    { id: 0, nome: 'Único' }, { id: 1, nome: 'Pequena' }, { id: 2, nome: 'Média' }, { id: 3, nome: 'Grande' }, { id: 4, nome: 'Gigante' }
  ];

  get categoriasParaCombo() {
    return this.categorias.filter(c => c.id !== 3); // Remove categoria Combo
  }

  get tamanhosPizza() {
    return this.tamanhos.filter(t => t.id !== 0);
  }

  getTamanhosPorCategoria(categoriaId: number) {
    if (categoriaId === 1) return this.tamanhosPizza;
    return this.tamanhos;
  }

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.produtoService.getAll().subscribe({
      next: (dados) => {
        console.log('Produtos carregados:', dados);
        this.produtos = dados;
        this.cdr.detectChanges(); // Força atualização da view
      },
      error: (err) => console.error('Erro ao carregar produtos:', err)
    });
  }

  navegarParaCupons() {
    this.router.navigate(['/admin/cupons']);
  }

  novoProduto() {
    this.produtoEmEdicao = {
      categoria: 1,
      tamanho: 3, // Default para Grande em Pizza
      preco: '',
      precoPromocional: '',
      ativo: true,
      nome: '',
      descricao: '',
      comboTemplates: []
    };
    this.arquivoSelecionado = null;
    this.exibindoFormulario = true;
  }

  editar(prod: Produto) {
    this.produtoEmEdicao = {
      ...prod,
      // Garante que sejam números e não nulos
      categoria: prod.categoria !== undefined && prod.categoria !== null ? Number(prod.categoria) : 1,
      tamanho: prod.tamanho !== undefined && prod.tamanho !== null ? Number(prod.tamanho) : 0,
      preco: prod.preco !== undefined && prod.preco !== null ? Number(prod.preco).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '',
      precoPromocional: prod.precoPromocional !== undefined && prod.precoPromocional !== null ? Number(prod.precoPromocional).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '',
      comboTemplates: prod.comboTemplates ? [...prod.comboTemplates] : []
    };
    this.arquivoSelecionado = null;
    this.exibindoFormulario = true;
  }

  onFileSelected(event: any) {
    this.arquivoSelecionado = event.target.files[0];
  }

  // Reseta o tamanho se não for Pizza (1)
  onCategoriaChange() {
    if (this.produtoEmEdicao.categoria !== 1) {
      this.produtoEmEdicao.tamanho = 0; // Único
    } else {
      // Se mudou pra pizza e está como Único, muda pra Grande
      if (this.produtoEmEdicao.tamanho === 0) {
        this.produtoEmEdicao.tamanho = 3;
      }
    }
    if (this.produtoEmEdicao.categoria === 3 && !this.produtoEmEdicao.comboTemplates) {
      this.produtoEmEdicao.comboTemplates = [];
    }
  }

  // --- Funcoes para o construtor do Combo ---
  adicionarItemCombo() {
    if (!this.produtoEmEdicao.comboTemplates) {
      this.produtoEmEdicao.comboTemplates = [];
    }
    this.produtoEmEdicao.comboTemplates.push({
      quantidade: 1,
      categoriaPermitida: 1, // Default para Pizza
      tamanhoObrigatorio: 3, // Default para Grande em Pizza
      valorCobertura: 0
    });
  }

  onComboSlotCategoriaChange(template: any) {
    if (template.categoriaPermitida !== 1) {
      template.tamanhoObrigatorio = 0; // Único
    } else {
      if (template.tamanhoObrigatorio === 0) {
        template.tamanhoObrigatorio = 3; // Grande
      }
    }
  }

  removerItemCombo(index: number) {
    this.produtoEmEdicao.comboTemplates.splice(index, 1);
  }

  getDescricaoCategoriaCombo(cat: number): string {
    const found = this.categorias.find(c => c.id == cat);
    return found ? found.nome : '';
  }

  // Formata o preço enquanto o usuário digita ou ao sair do campo
  onPrecoInput(valor: string) {
    // Apenas para sanitização se necessário
  }

  onPrecoBlur() {
    this.produtoEmEdicao.preco = this.formatarPreco(this.produtoEmEdicao.preco);
  }

  onPrecoPromocionalBlur() {
    this.produtoEmEdicao.precoPromocional = this.formatarPreco(this.produtoEmEdicao.precoPromocional);
  }

  private formatarPreco(valorInput: any): string {
    if (!valorInput) return '';

    let valor = valorInput.toString();

    // Se já tiver vírgula, respeita o que foi digitado (apenas ajustando pontos de milhar)
    if (valor.includes(',')) {
      // Remove tudo que não é digito e nem vírgula
      valor = valor.replace(/[^\d,]/g, '');

      let partes = valor.split(',');
      let inteiros = partes[0];
      let decimais = partes[1].substring(0, 2); // limita a 2 casas

      // Formata a parte inteira com pontos de milhar
      if (inteiros) {
        inteiros = parseInt(inteiros).toLocaleString('pt-BR');
      } else {
        inteiros = '0';
      }

      // Reconstrói
      return `${inteiros},${decimais}`;
    }
    else {
      // Se não tiver vírgula, assume que é inteiro e coloca ,00
      let numero = parseFloat(valor.replace(/\D/g, ''));
      if (isNaN(numero)) return '';
      return numero.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
  }

  getImagemUrl(imagemUrl: string | undefined): string {
    if (!imagemUrl) return 'assets/no-image.png'; // Ou uma imagem placeholder
    if (imagemUrl.startsWith('http')) return imagemUrl;
    // Remove barra inicial se houver para evitar //
    const cleanUrl = imagemUrl.startsWith('/') ? imagemUrl.substring(1) : imagemUrl;
    return `${environment.apiUrl}/${cleanUrl}`;
  }

  salvar() {
    // Validações básicas de tela
    if (!this.produtoEmEdicao.nome || this.produtoEmEdicao.nome.trim() === '') {
      alert('⚠️ O nome do produto é obrigatório.');
      return;
    }

    if (!this.produtoEmEdicao.preco || this.produtoEmEdicao.preco === '' || this.produtoEmEdicao.preco == 0) {
      alert('⚠️ O preço do produto é obrigatório e deve ser maior que zero.');
      return;
    }

    this.isLoading = true;

    // Converte "1.234,56" para number 1234.56 antes de enviar
    if (typeof this.produtoEmEdicao.preco === 'string' && this.produtoEmEdicao.preco) {
      let precoLimpo = this.produtoEmEdicao.preco.replace(/\./g, '').replace(',', '.');
      this.produtoEmEdicao.preco = parseFloat(precoLimpo);
    }

    if (typeof this.produtoEmEdicao.precoPromocional === 'string' && this.produtoEmEdicao.precoPromocional) {
      let precoLimpo = this.produtoEmEdicao.precoPromocional.replace(/\./g, '').replace(',', '.');
      this.produtoEmEdicao.precoPromocional = parseFloat(precoLimpo);
    } else if (!this.produtoEmEdicao.precoPromocional) {
      this.produtoEmEdicao.precoPromocional = null;
    }

    const preco = parseFloat(this.produtoEmEdicao.preco?.toString().replace(',', '.') || '0');
    const precoPromocional = this.produtoEmEdicao.precoPromocional ? parseFloat(this.produtoEmEdicao.precoPromocional.toString().replace(',', '.')) : null;

    if (precoPromocional !== null && precoPromocional >= preco) {
      alert('O preço promocional deve ser MENOR que o preço original.');
      this.isLoading = false;
      return;
    }

    if (this.produtoEmEdicao.id) {
      // Edição
      this.produtoService.update(this.produtoEmEdicao.id, this.produtoEmEdicao, this.arquivoSelecionado)
        .subscribe({
          next: () => {
            console.log('Produto atualizado com sucesso');
            this.exibindoFormulario = false;
            this.carregar();
            this.isLoading = false;
            this.cdr.detectChanges();
            this.produtoEmEdicao = {}; // Reset para segurança
          },
          error: (err) => {
            console.error('Erro ao atualizar produto:', err);
            alert('❌ Erro ao atualizar o produto. Verifique se os dados estão corretos e preenchidos.');
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
    } else {
      // Criação
      this.produtoService.create(this.produtoEmEdicao, this.arquivoSelecionado)
        .subscribe({
          next: (resp) => {
            console.log('Produto criado com sucesso:', resp);
            this.exibindoFormulario = false;
            this.carregar();
            this.isLoading = false;
            this.cdr.detectChanges();
            this.produtoEmEdicao = {}; // Reset para segurança
          },
          error: (err) => {
            console.error('Erro ao criar produto:', err);
            alert('❌ Erro ao criar o produto. Preencha os campos obrigatórios e números válidos antes de salvar.');
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
    }
  }

  excluir(id: number) {
    if (confirm('Tem certeza que deseja excluir?')) {
      this.produtoService.delete(id).subscribe(() => this.carregar());
    }
  }

  getDescricaoCategoria(id: number): string {
    const categoria = this.categorias.find(c => c.id === id);
    return categoria ? categoria.nome : 'Desconhecida';
  }

  getDescricaoTamanho(id: number): string {
    const tamanho = this.tamanhos.find(t => t.id === id);
    return tamanho ? tamanho.nome : 'N/A';
  }
}