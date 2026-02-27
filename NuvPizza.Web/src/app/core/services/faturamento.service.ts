import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface FaturamentoDTO {
    faturamento: number;
    quantidadePedidos: number;
    frete: number;
    ticketMedio: number;
}

@Injectable({
    providedIn: 'root'
})
export class FaturamentoService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/faturamento`;

    obterFaturamento(dataInicial?: string, dataFinal?: string): Observable<FaturamentoDTO> {
        let params = new HttpParams();

        if (dataInicial) {
            params = params.set('inicial', `${dataInicial}T00:00:00`);
        }

        if (dataFinal) {
            params = params.set('final', `${dataFinal}T23:59:59`);
        }

        return this.http.get<FaturamentoDTO>(this.apiUrl, { params });
    }
}
