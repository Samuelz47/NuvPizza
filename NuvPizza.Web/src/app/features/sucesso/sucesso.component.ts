import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-sucesso',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="sucesso-container">
      <div class="icon">🎉</div>
      <h1>Pedido Recebido!</h1>
      <p>Seu pagamento foi processado com sucesso.</p>
      <p>A pizza já vai pro forno!</p>
      <a routerLink="/" class="btn-voltar">Voltar para Início</a>
    </div>
  `,
  styles: [`
    .sucesso-container { text-align: center; padding: 50px; font-family: sans-serif; }
    .icon { font-size: 80px; margin-bottom: 20px; }
    h1 { color: #28a745; }
    .btn-voltar { 
      display: inline-block; margin-top: 20px; padding: 10px 20px; 
      background: #009ee3; color: white; text-decoration: none; border-radius: 5px; 
    }
  `]
})
export class SucessoComponent {}