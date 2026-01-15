import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

// Garanta que você tem essa interface (ou similar) no seu arquivo de models
export interface Pedido {
  id: string;
  dataPedido: Date;
  valorTotal: number;
  statusPedido: number; // 0=Pendente, 1=Preparo, 2=Saiu, 3=Entregue, 4=Cancelado
  itens: any[];
  // Adicione outros campos que vêm no DTO
}

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Pedido`;

  // --- NOVO MÉTODO: Listar Pedidos ---
  getPedidos(pageNumber: number = 1, pageSize: number = 50): Observable<Pedido[]> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);
      // Se quiser filtrar por status no futuro, adicione aqui
    
    return this.http.get<Pedido[]>(this.apiUrl, { params });
  }

  // Método para avançar o status (usaremos nos botões do card)
  atualizarStatus(id: string, novoStatus: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/status`, { statusDoPedido: novoStatus });
  }
}