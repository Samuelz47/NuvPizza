import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // Importante para usar [(ngModel)]
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-container">
      <div class="card">
        <h2>Área Restrita 🔒</h2>
        <form (submit)="fazerLogin($event)">
          <div class="form-group">
            <label>Email</label>
            <input type="email" [(ngModel)]="email" name="email" required>
          </div>
          
          <div class="form-group">
            <label>Senha</label>
            <input type="password" [(ngModel)]="password" name="password" required>
          </div>

          <button type="submit" [disabled]="loading()" class="btn-login">
            {{ loading() ? 'Entrando...' : 'Acessar Painel' }}
          </button>

          @if (erro()) {
            <p class="error">{{ erro() }}</p>
          }
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-container { height: 100vh; display: flex; align-items: center; justify-content: center; background: #f0f2f5; }
    .card { background: white; padding: 2rem; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); width: 100%; max-width: 400px; text-align: center; }
    .form-group { margin-bottom: 15px; text-align: left; }
    input { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; } /* box-sizing vital para input não estourar */
    .btn-login { width: 100%; padding: 12px; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem; }
    .btn-login:disabled { background: #ccc; }
    .error { color: red; margin-top: 10px; }
  `]
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  loading = signal(false);
  erro = signal('');

  fazerLogin(event: Event) {
    event.preventDefault(); // Evita recarregar a página
    this.loading.set(true);
    this.erro.set('');

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(['/admin/pedidos']);
      },
      error: (err) => {
        this.loading.set(false);
        this.erro.set('Email ou senha inválidos.');
        console.error(err);
      }
    });
  }
}