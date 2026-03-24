import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable, map } from 'rxjs';

export interface Cliente {
    id: string;
    nome: string;
    telefone: string;
    email?: string;
    quantidadePedidos: number;
    valorTotalGasto: number;
    dataPrimeiroPedido: string;
    dataUltimoPedido: string;
}

export interface PaginacaoMeta {
    totalCount: number;
    pageSize: number;
    totalPages: number;
    pageNumber: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

@Injectable({ providedIn: 'root' })
export class ClienteService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Cliente`;

    getRanking(pageNumber: number = 1, pageSize: number = 15, ordenarPor: string = 'valor'): Observable<{ clientes: Cliente[], paginacao: PaginacaoMeta }> {
        let params = new HttpParams()
            .set('pageNumber', pageNumber)
            .set('pageSize', pageSize)
            .set('ordenarPor', ordenarPor);

        return this.http.get<Cliente[]>(`${this.apiUrl}/ranking`, { params, observe: 'response' }).pipe(
            map((res: HttpResponse<Cliente[]>) => {
                const paginacaoHeader = res.headers.get('X-Pagination');
                let paginacao: PaginacaoMeta = {
                    totalCount: 0,
                    pageSize: 15,
                    totalPages: 1,
                    pageNumber: 1,
                    hasNextPage: false,
                    hasPreviousPage: false
                };
                if (paginacaoHeader) {
                    const parsed = JSON.parse(paginacaoHeader);
                    paginacao = {
                        totalCount: parsed.TotalCount,
                        pageSize: parsed.PageSize,
                        totalPages: parsed.TotalPages,
                        pageNumber: parsed.PageNumber,
                        hasNextPage: parsed.HasNextPage,
                        hasPreviousPage: parsed.HasPreviousPage
                    };
                }
                return { clientes: res.body || [], paginacao };
            })
        );
    }
}
