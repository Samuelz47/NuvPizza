import { Component, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CarrinhoService } from '../../../core/services/carrinho.service';

@Component({
  selector: 'app-carrinho-float',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './carrinho-float.html',
  styleUrls: ['./carrinho-float.css']
})
export class CarrinhoFloatComponent {
  private carrinhoService = inject(CarrinhoService);
  private router = inject(Router);

  quantidade = this.carrinhoService.quantidadeTotal;
  total = this.carrinhoService.valorTotal;
  itens = this.carrinhoService.itens;

  // Painel aberto/fechado
  painelAberto = signal(false);

  // Sinal de animação ao adicionar item
  animando = signal(false);
  private qtdAnterior = this.quantidade();

  constructor() {
    // Dispara a animação sempre que a quantidade aumentar
    effect(() => {
      const qtdNova = this.quantidade();
      if (qtdNova > this.qtdAnterior) {
        this.animando.set(true);
        setTimeout(() => this.animando.set(false), 600);
      }
      this.qtdAnterior = qtdNova;
    });
  }

  togglePainel() {
    this.painelAberto.update(v => !v);
  }

  decrementar(id: number) {
    this.carrinhoService.decrementar(id);
  }

  adicionarItem(item: any) {
    this.carrinhoService.adicionar(item);
  }

  removerItem(id: number) {
    this.carrinhoService.remover(id);
  }

  irParaCheckout() {
    this.painelAberto.set(false);
    this.router.navigate(['/checkout']);
  }
}