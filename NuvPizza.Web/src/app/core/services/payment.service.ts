import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  // Se usar Proxy, apiUrl pode ser vazia ou '/'
  private apiUrl = environment.apiUrl; 

  criarPreferencia(dados: any): Observable<any> {
    // Atenção à rota: /Pagamento/criar-link
    return this.http.post<any>(`${this.apiUrl}/Pagamento/criar-link`, dados);
  }
}