import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FaturamentoService, FaturamentoDTO } from '../../../core/services/faturamento.service';

@Component({
  selector: 'app-admin-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-home.html',
  styleUrls: ['./admin-home.css']
})
export class AdminHomeComponent implements OnInit {
  private router = inject(Router);
  private faturamentoService = inject(FaturamentoService);

  // Estado do Dashboard
  carregando = signal<boolean>(true);
  dadosFaturamento = signal<FaturamentoDTO | null>(null);

  // Filtros (Formato YYYY-MM-DD para input date)
  dataInicial = signal<string>('');
  dataFinal = signal<string>('');
  filtroAtivo = signal<'hoje' | 'semana' | 'mes' | 'custom'>('mes'); // Padrão é mês atual

  ngOnInit() {
    this.filtrarMes(); // Carrega o mês atual por padrão
  }

  carregarFaturamento(inicio?: string, fim?: string) {
    this.carregando.set(true);
    this.faturamentoService.obterFaturamento(inicio, fim).subscribe({
      next: (dados) => {
        this.dadosFaturamento.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar faturamento', err);
        this.carregando.set(false);
      }
    });
  }

  filtrarHoje() {
    this.filtroAtivo.set('hoje');
    const hoje = new Date().toISOString().split('T')[0];
    this.dataInicial.set(hoje);
    this.dataFinal.set(hoje);
    this.carregarFaturamento(hoje, hoje);
  }

  filtrarSemana() {
    this.filtroAtivo.set('semana');
    const hoje = new Date();

    // Pega o domingo desta semana
    const primeiroDia = new Date(hoje.setDate(hoje.getDate() - hoje.getDay()));

    // Pega o sábado desta semana
    const ultimoDia = new Date(hoje.setDate(hoje.getDate() - hoje.getDay() + 6));

    const wStart = primeiroDia.toISOString().split('T')[0];
    const wEnd = ultimoDia.toISOString().split('T')[0];

    this.dataInicial.set(wStart);
    this.dataFinal.set(wEnd);
    this.carregarFaturamento(wStart, wEnd);
  }

  filtrarMes() {
    this.filtroAtivo.set('mes');
    const hoje = new Date();
    const ano = hoje.getFullYear();
    const mes = hoje.getMonth();

    const primeiroDia = new Date(ano, mes, 1);
    const ultimoDia = new Date(ano, mes + 1, 0); // O dia 0 do próximo mês volta para o último dia do mês atual

    const mStart = primeiroDia.toISOString().split('T')[0];
    const mEnd = ultimoDia.toISOString().split('T')[0];

    this.dataInicial.set(mStart);
    this.dataFinal.set(mEnd);
    this.carregarFaturamento(mStart, mEnd); // Ou pode chamar sem parâmetros para usar o padrão da API
  }

  aplicarFiltroCustomizado() {
    this.filtroAtivo.set('custom');
    if (this.dataInicial() && this.dataFinal()) {
      this.carregarFaturamento(this.dataInicial(), this.dataFinal());
    }
  }

  navegarPara(destino: 'pedidos' | 'produtos') {
    if (destino === 'pedidos') {
      this.router.navigate(['/admin/painel']);
    } else {
      this.router.navigate(['/admin/produtos']);
    }
  }
}