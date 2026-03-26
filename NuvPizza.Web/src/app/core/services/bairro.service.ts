import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface Bairro {
    id: number;
    nome: string;
    valorFrete: number;
}

@Injectable({
    providedIn: 'root'
})
export class BairroService {
    private http = inject(HttpClient);

    getBairros(): Observable<Bairro[]> {
        return this.http.get<Bairro[]>(`${environment.apiUrl}/bairros`);
    }
}
