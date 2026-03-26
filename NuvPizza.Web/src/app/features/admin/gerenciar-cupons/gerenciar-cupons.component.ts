import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CupomService } from '../../../core/services/cupom.service';
import { Cupom, CupomForRegistration } from '../../../core/models/cupom.model';

@Component({
    selector: 'app-gerenciar-cupons',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './gerenciar-cupons.html',
    styleUrls: ['./gerenciar-cupons.css']
})
export class GerenciarCuponsComponent implements OnInit {
    private cupomService = inject(CupomService);

    cupons = signal<Cupom[]>([]);
    exibindoFormulario = false;
    isLoading = false;

    novoCupom: CupomForRegistration = {
        codigo: '',
        descontoPorcentagem: 0,
        freteGratis: false,
        pedidoMinimo: 0
    };

    toast = signal<{ mensagem: string; tipo: 'sucesso' | 'erro'; visivel: boolean }>({
        mensagem: '',
        tipo: 'sucesso',
        visivel: false
    });

    ngOnInit() {
        this.carregar();
    }

    carregar() {
        this.cupomService.getCupons().subscribe({
            next: (dados) => {
                console.log('[DEBUG] Lista de cupons carregada:', dados);
                this.cupons.set(dados);
            },
            error: (err) => {
                console.error('Erro ao carregar cupons', err);
                this.mostrarToast('Erro ao carregar cupons.', 'erro');
            }
        });
    }

    abrirFormulario() {
        this.novoCupom = { codigo: '', descontoPorcentagem: 0, freteGratis: false, pedidoMinimo: 0 };
        this.exibindoFormulario = true;
    }

    salvar() {
        // Validações client-side básicas
        const codigo = this.novoCupom.codigo.trim().toUpperCase();
        if (!codigo) {
            this.mostrarToast('O código do cupom é obrigatório.', 'erro');
            return;
        }
        if (this.novoCupom.descontoPorcentagem < 0 || this.novoCupom.descontoPorcentagem > 100) {
            this.mostrarToast('O desconto deve ser entre 0% e 100%.', 'erro');
            return;
        }
        if (this.novoCupom.descontoPorcentagem === 0 && !this.novoCupom.freteGratis) {
            this.mostrarToast('Informe um desconto ou ative o Frete Grátis.', 'erro');
            return;
        }

        this.isLoading = true;
        const payload: CupomForRegistration = { ...this.novoCupom, codigo };

        this.cupomService.createCupom(payload).subscribe({
            next: () => {
                this.isLoading = false;
                this.exibindoFormulario = false;
                this.mostrarToast('Cupom criado com sucesso!', 'sucesso');
                this.carregar();
            },
            error: (err) => {
                this.isLoading = false;
                const msg = err?.error?.message ?? 'Erro ao criar cupom.';
                this.mostrarToast(msg, 'erro');
            }
        });
    }

    excluir(id: number, codigo: string) {
        if (!confirm(`Excluir o cupom "${codigo}"? Esta ação não pode ser desfeita.`)) return;

        this.cupomService.deleteCupom(id).subscribe({
            next: () => {
                this.mostrarToast('Cupom excluído.', 'sucesso');
                this.carregar();
            },
            error: () => this.mostrarToast('Erro ao excluir cupom.', 'erro')
        });
    }

    mostrarToast(msg: string, tipo: 'sucesso' | 'erro') {
        this.toast.set({ mensagem: msg, tipo, visivel: true });
        setTimeout(() => this.toast.update(t => ({ ...t, visivel: false })), 3500);
    }
}
