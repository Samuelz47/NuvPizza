import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { CriarPreferenciaRequest, CriarPreferenciaResponse } from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  criarPreferencia(dados: CriarPreferenciaRequest) {
    // O retorno agora é tipado como CriarPreferenciaResponse
    return this.http.post<CriarPreferenciaResponse>(`${this.apiUrl}/pagamento/criar-link`, dados);
  }
}