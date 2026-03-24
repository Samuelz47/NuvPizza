import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClienteService, Cliente, PaginacaoMeta } from '../../../core/services/cliente.service';

@Component({
    selector: 'app-clientes-ranking',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './clientes-ranking.html',
    styleUrls: ['./clientes-ranking.css']
})
export class ClientesRankingComponent implements OnInit {
    private clienteService = inject(ClienteService);
    private cdr = inject(ChangeDetectorRef);

    clientes: Cliente[] = [];
    paginacao: PaginacaoMeta | null = null;
    paginaAtual = 1;
    ordenarPor = 'valor';
    carregando = false;

    ngOnInit() {
        this.carregarRanking();
    }

    carregarRanking() {
        this.carregando = true;
        this.clienteService.getRanking(this.paginaAtual, 15, this.ordenarPor).subscribe({
            next: (res) => {
                console.log('RANKING RESULT:', res);
                this.clientes = res.clientes;
                this.paginacao = res.paginacao;
                this.carregando = false;
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('RANKING ERROR:', err);
                this.carregando = false;
                this.cdr.detectChanges();
            }
        });
    }

    mudarOrdenacao(criterio: string) {
        this.ordenarPor = criterio;
        this.paginaAtual = 1;
        this.carregarRanking();
    }

    paginaAnterior() {
        if (this.paginacao?.hasPreviousPage) {
            this.paginaAtual--;
            this.carregarRanking();
        }
    }

    proximaPagina() {
        if (this.paginacao?.hasNextPage) {
            this.paginaAtual++;
            this.carregarRanking();
        }
    }

    getMedalha(index: number): string {
        const posicao = (this.paginaAtual - 1) * 15 + index;
        if (posicao === 0) return '🥇';
        if (posicao === 1) return '🥈';
        if (posicao === 2) return '🥉';
        return `${posicao + 1}º`;
    }
}
