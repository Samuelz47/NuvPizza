import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface StatusLoja {
    estaAberta: boolean;
    dataHoraFechamento: string | null;
}

@Injectable({
    providedIn: 'root'
})
export class LojaService {
    private http = inject(HttpClient);

    getStatus(): Observable<StatusLoja> {
        return this.http.get<StatusLoja>(`${environment.apiUrl}/loja/status`);
    }

    abrirLoja(horaEncerramento: string): Observable<any> {
        const horaFormatada = horaEncerramento.length === 5 ? `${horaEncerramento}:00` : horaEncerramento;
        return this.http.post(`${environment.apiUrl}/loja/abrir`, { horaDeEncerramento: horaFormatada });
    }

    fecharLoja(): Observable<any> {
        return this.http.post(`${environment.apiUrl}/loja/fechar`, {});
    }

    estenderLoja(minutosExtras: number): Observable<any> {
        return this.http.post(`${environment.apiUrl}/loja/estender`, { minutosExtras });
    }
}
