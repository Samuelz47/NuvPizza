import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface Motoboy {
    id: string;
    nome: string;
    telefone: string;
    ativo: boolean;
    dataCadastro: Date;
}

export interface FaturamentoMotoboy {
    motoboyId: string;
    nomeMotoboy: string;
    totalFrete: number;
    quantidadeEntregas: number;
    dataInicial: Date;
    dataFinal: Date;
}

@Injectable({
    providedIn: 'root'
})
export class MotoboyService {
    private apiUrl = `${environment.apiUrl}/api/motoboy`;

    constructor(private http: HttpClient) { }

    obterTodos(): Observable<Motoboy[]> {
        return this.http.get<Motoboy[]>(this.apiUrl);
    }

    obterAtivos(): Observable<Motoboy[]> {
        return this.http.get<Motoboy[]>(`${this.apiUrl}/ativos`);
    }

    obterPorId(id: string): Observable<Motoboy> {
        return this.http.get<Motoboy>(`${this.apiUrl}/${id}`);
    }

    criar(motoboy: any): Observable<Motoboy> {
        return this.http.post<Motoboy>(this.apiUrl, motoboy);
    }

    atualizar(id: string, motoboy: any): Observable<Motoboy> {
        return this.http.put<Motoboy>(`${this.apiUrl}/${id}`, motoboy);
    }

    deletar(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    obterFaturamento(id: string, dataInicial: string, dataFinal: string): Observable<FaturamentoMotoboy> {
        return this.http.get<FaturamentoMotoboy>(`${this.apiUrl}/${id}/faturamento?dataInicial=${dataInicial}&dataFinal=${dataFinal}`);
    }
}
