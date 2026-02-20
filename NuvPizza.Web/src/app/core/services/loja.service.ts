import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class LojaService {
    private http = inject(HttpClient);

    abrirLoja(horaEncerramento: string): Observable<any> {
        // A DTO do backend pede horaDeEncerramento no formato TimeSpan, string como '23:59:00' funciona.
        return this.http.post(`${environment.apiUrl}/loja/abrir`, { horaDeEncerramento: horaEncerramento });
    }

    fecharLoja(): Observable<any> {
        return this.http.post(`${environment.apiUrl}/loja/fechar`, {});
    }
}
