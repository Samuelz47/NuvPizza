import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FaturamentoService, FaturamentoDTO } from '../../../core/services/faturamento.service';
import { LojaService, StatusLoja } from '../../../core/services/loja.service';

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
  private lojaService = inject(LojaService);

  // Estado do Dashboard
  carregando = signal<boolean>(true);
  dadosFaturamento = signal<FaturamentoDTO | null>(null);

  // Filtros (Formato YYYY-MM-DD para input date)
  dataInicial = signal<string>('');
  dataFinal = signal<string>('');
  filtroAtivo = signal<'hoje' | 'semana' | 'mes' | 'custom'>('mes'); // Padrão é mês atual

  // Configurações
  videoDestaqueInput = signal<string>('');
  salvandoVideo = signal<boolean>(false);

  // Toast interno
  toast = signal<{ mensagem: string, tipo: 'sucesso' | 'erro' | 'info', visivel: boolean }>({
    mensagem: '',
    tipo: 'info',
    visivel: false
  });

  ngOnInit() {
    this.filtrarMes(); // Carrega o mês atual por padrão
    this.carregarStatusLoja();
  }

  carregarStatusLoja() {
    this.lojaService.getStatus().subscribe({
      next: (status) => {
        this.videoDestaqueInput.set(status.videoDestaqueUrl || '');
      },
      error: () => {
        console.error('Erro ao carregar status da loja');
      }
    });
  }

  salvarVideoDestaque() {
    this.salvandoVideo.set(true);
    const url = this.videoDestaqueInput().trim() || null;

    this.lojaService.atualizarVideoDestaque(url).subscribe({
      next: () => {
        this.salvandoVideo.set(false);
        this.mostrarToast('Vídeo Destaque atualizado!', 'sucesso');
      },
      error: (err) => {
        console.error(err);
        this.salvandoVideo.set(false);
        this.mostrarToast('Erro ao atualizar Vídeo Destaque.', 'erro');
      }
    });
  }

  mostrarToast(msg: string, tipo: 'sucesso' | 'erro' | 'info') {
    this.toast.set({ mensagem: msg, tipo: tipo, visivel: true });
    setTimeout(() => this.toast.update(t => ({ ...t, visivel: false })), 3000);
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

  private getLocalDateString(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  filtrarHoje() {
    this.filtroAtivo.set('hoje');
    const hojeStr = this.getLocalDateString(new Date());
    this.dataInicial.set(hojeStr);
    this.dataFinal.set(hojeStr);
    this.carregarFaturamento(hojeStr, hojeStr);
  }

  filtrarSemana() {
    this.filtroAtivo.set('semana');
    const hoje = new Date();

    // Pega o domingo e o sábado
    const start = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate() - hoje.getDay());
    const end = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate() - hoje.getDay() + 6);

    const wStart = this.getLocalDateString(start);
    const wEnd = this.getLocalDateString(end);

    this.dataInicial.set(wStart);
    this.dataFinal.set(wEnd);
    this.carregarFaturamento(wStart, wEnd);
  }

  filtrarMes() {
    this.filtroAtivo.set('mes');
    const hoje = new Date();
    const ano = hoje.getFullYear();
    const mes = hoje.getMonth();

    const start = new Date(ano, mes, 1);
    const end = new Date(ano, mes + 1, 0); // Último dia do mês

    const mStart = this.getLocalDateString(start);
    const mEnd = this.getLocalDateString(end);

    this.dataInicial.set(mStart);
    this.dataFinal.set(mEnd);
    this.carregarFaturamento(mStart, mEnd);
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