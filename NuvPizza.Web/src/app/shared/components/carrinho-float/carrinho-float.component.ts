import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CarrinhoService } from '../../../core/services/carrinho.service';

@Component({
  selector: 'app-carrinho-float',
  standalone: true,
  imports: [CommonModule], // Necessário para usar o | currency
  templateUrl: './carrinho-float.html',
  styleUrls: ['./carrinho-float.css']
})
export class CarrinhoFloatComponent {
  // Injeção de dependência moderna
  private carrinhoService = inject(CarrinhoService);
  private router = inject(Router);

  // Criamos referências diretas aos SIGNALS do serviço
  // O template vai "ouvir" essas variáveis automaticamente
  quantidade = this.carrinhoService.quantidadeTotal;
  total = this.carrinhoService.valorTotal;

  irParaCheckout() {
    this.router.navigate(['/checkout']);
  }
}