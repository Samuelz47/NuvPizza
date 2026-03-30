import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MotoboyService, Motoboy } from '../../../core/services/motoboy.service';

@Component({
    selector: 'app-gerenciar-motoboys',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './gerenciar-motoboys.html',
    styleUrls: ['./gerenciar-motoboys.css']
})
export class GerenciarMotoboysComponent implements OnInit {
    private motoboyService = inject(MotoboyService);

    motoboys: Motoboy[] = [];
    exibirModal = false;
    editando = false;
    salvando = false;
    motoboyForm: any = { nome: '', telefone: '', ativo: true };

    ngOnInit() {
        this.carregarMotoboys();
    }

    carregarMotoboys() {
        this.motoboyService.obterTodos().subscribe(data => {
            this.motoboys = data;
        });
    }

    abrirModal() {
        this.editando = false;
        this.motoboyForm = { nome: '', telefone: '', ativo: true };
        this.exibirModal = true;
    }

    editarMotoboy(motoboy: Motoboy) {
        this.editando = true;
        this.motoboyForm = { ...motoboy };
        this.exibirModal = true;
    }

    fecharModal() {
        this.exibirModal = false;
    }

    salvarMotoboy() {
        this.salvando = true;
        if (this.editando) {
            this.motoboyService.atualizar(this.motoboyForm.id, this.motoboyForm).subscribe({
                next: () => {
                    this.carregarMotoboys();
                    this.fecharModal();
                    this.salvando = false;
                },
                error: (err) => {
                    console.error('Erro ao editar', err);
                    this.salvando = false;
                }
            });
        } else {
            this.motoboyService.criar(this.motoboyForm).subscribe({
                next: () => {
                    this.carregarMotoboys();
                    this.fecharModal();
                    this.salvando = false;
                },
                error: (err) => {
                    console.error('Erro ao salvar', err);
                    this.salvando = false;
                }
            });
        }
    }

    deletarMotoboy(id: string) {
        if (confirm('Deseja realmente excluir este motoboy?')) {
            this.motoboyService.deletar(id).subscribe(() => {
                this.carregarMotoboys();
            });
        }
    }
}
