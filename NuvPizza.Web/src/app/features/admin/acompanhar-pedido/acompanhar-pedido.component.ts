import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PedidoService } from '../../../core/services/pedido.service';

@Component({
  selector: 'app-acompanhar-pedido',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './acompanhar-pedido.html',
  styleUrls: ['./acompanhar-pedido.css']
})
export class AcompanharPedidoComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private pedidoService = inject(PedidoService);

  pedido = signal<any>(null);
  carregando = signal(true);
  intervaloAtualizacao: any;

  // Mapa de status para a barra de progresso
  steps = [
    { status: 1, label: 'Recebido', icon: 'bi-send' },
    { status: 2, label: 'Confirmado', icon: 'bi-check-circle' },
    { status: 3, label: 'Preparando', icon: 'bi-fire' },
    { status: 4, label: 'Saiu p/ Entrega', icon: 'bi-bicycle' },
    { status: 5, label: 'Finalizado', icon: 'bi-house-door-fill' }
  ];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.carregarPedido(id);

      // Atualiza a cada 10 segundos automaticamente
      this.intervaloAtualizacao = setInterval(() => {
        this.carregarPedido(id, false); // false = não mostra loading
      }, 10000);
    }
  }

  ngOnDestroy() {
    if (this.intervaloAtualizacao) clearInterval(this.intervaloAtualizacao);
  }

  carregarPedido(id: string, mostrarLoading = true) {
    if (mostrarLoading) this.carregando.set(true);

    console.log('Tentando buscar pedido ID:', id); // <--- DEBUG 1

    this.pedidoService.getPedidoPorId(id).subscribe({
      next: (dados) => {
        console.log('Pedido Encontrado!', dados); // <--- DEBUG 2

        if (!dados) {
          console.error('O serviço retornou null ou undefined');
          this.carregando.set(false);
          return;
        }

        // Conversão simples do status caso venha string
        dados.statusPedido = this.normalizarStatus(dados.statusPedido);
        this.pedido.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro fatal ao buscar pedido:', err); // <--- DEBUG 3
        this.carregando.set(false);
      }
    });
  }

  normalizarStatus(status: any): number {
    if (typeof status === 'number') return status;
    const mapa: any = {
      'criado': 1,
      'confirmado': 2,
      'empreparo': 3,
      'saiuparaentrega': 4,
      'entrega': 5,
      'entregue': 5,
      'finalizado': 5,
      'cancelado': 0
    };
    return mapa[status?.toString().toLowerCase().trim()] || 1;
  }

  // Verifica se o passo já foi concluído ou é o atual
  isStepActive(stepStatus: number): boolean {
    const atual = this.pedido()?.statusPedido || 0;
    return atual >= stepStatus;
  }

  // Verifica se é EXATAMENTE o passo atual (para animação de pulso)
  isCurrentStep(stepStatus: number): boolean {
    return this.pedido()?.statusPedido === stepStatus;
  }
}