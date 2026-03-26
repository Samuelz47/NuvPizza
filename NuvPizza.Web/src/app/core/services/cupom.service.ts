import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Cupom, CupomForRegistration } from '../models/cupom.model';

@Injectable({
    providedIn: 'root'
})
export class CupomService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/cupom`;

    // Usado no Painel Admin — lista todos os cupons
    getCupons(): Observable<Cupom[]> {
        return this.http.get<Cupom[]>(this.apiUrl);
    }

    // Usado no Checkout — busca um cupom pelo código digitado pelo cliente
    getCupomPorCodigo(codigo: string, telefone?: string): Observable<Cupom> {
        let url = `${this.apiUrl}/${codigo}`;
        if (telefone) {
            url += `?telefone=${telefone}`;
        }
        return this.http.get<Cupom>(url);
    }

    // Usado no Painel Admin — cria um novo cupom
    createCupom(cupom: CupomForRegistration): Observable<Cupom> {
        return this.http.post<Cupom>(this.apiUrl, cupom);
    }

    // Usado no Painel Admin — remove um cupom por ID
    deleteCupom(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
