import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  criar(pedido: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Pedido`, pedido);
  }
}