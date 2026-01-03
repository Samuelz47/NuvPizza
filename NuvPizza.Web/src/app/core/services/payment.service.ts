import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { PagamentoRequest, PagamentoResponse } from '../models/payment.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Pagamento`;

  processarPagamento(dados: PagamentoRequest): Observable<PagamentoResponse> {
    return this.http.post<PagamentoResponse>(this.apiUrl, dados);
  }
}