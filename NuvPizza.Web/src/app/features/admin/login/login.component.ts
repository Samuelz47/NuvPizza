import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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

          <button type="submit" [disabled]="loading()" class="btn-login" style="display: flex; align-items: center; justify-content: center; gap: 8px;">
            @if (loading()) {
              <span class="spinner">⏳</span>
            }
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
    label { display: block; margin-bottom: 5px; font-weight: bold; }
    input { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .btn-login { width: 100%; padding: 12px; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem; transition: background 0.2s; }
    .btn-login:hover { background: #0056b3; }
    .btn-login:disabled { background: #ccc; cursor: not-allowed; }
    .error { color: #dc3545; margin-top: 15px; padding: 10px; background: #ffe6e6; border-radius: 4px; font-size: 0.9rem; }
    .spinner { animation: spin 1s linear infinite; display: inline-block; }
    @keyframes spin { 100% { transform: rotate(360deg); } }
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
    event.preventDefault();
    this.loading.set(true);
    this.erro.set(''); // Limpa erro anterior

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        // Sucesso! Vai para a tela intermediária
        this.router.navigate(['/admin/home']);
      },
      error: (err: any) => { // <--- Importante: tipagem 'any'
        this.loading.set(false);
        console.error("ERRO NO LOGIN:", err); // Ajuda no debug

        // Lógica para descobrir qual foi o erro
        if (err.status === 0) {
          this.erro.set('Não foi possível conectar ao servidor. Verifique se a API está rodando.');
        } else if (err.status === 401) {
          this.erro.set('Email ou senha incorretos.');
        } else if (err.status === 404) {
          this.erro.set('Erro de configuração: Endereço da API não encontrado (404).');
        } else {
          this.erro.set('Ocorreu um erro inesperado. Tente novamente.');
        }
      }
    });
  }
}