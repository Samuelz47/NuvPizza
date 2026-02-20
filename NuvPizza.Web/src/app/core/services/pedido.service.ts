import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

// --- AQUI ESTAVA O PROBLEMA ---
// Atualizamos a interface para incluir 'numero' e 'nomeCliente'
export interface Pedido {
  id: string;
  numero: number;        // <--- Adicionado (Corrige erro do HTML)
  nomeCliente: string;   // <--- Adicionado (Corrige erro do HTML)
  dataPedido: Date;
  valorTotal: number;
  statusPedido: number;  // 1=Criado, 2=Confirmado, 3=Preparo, 4=Saiu, 5=Entregue, 0=Cancelado
  itens: any[];
  observacao?: string;
  formaPagamento: string;
}

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/pedido`;

  // 1. Listar Pedidos (Usado no Painel Admin)
  getPedidos(pageNumber: number = 1, pageSize: number = 50): Observable<Pedido[]> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);

    return this.http.get<Pedido[]>(this.apiUrl, { params });
  }

  getPedidoPorId(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  // 2. Criar Pedido (Usado no Checkout)
  createPedido(pedidoDto: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, pedidoDto);
  }

  // 3. Atualizar Status (Usado no Botão do Painel)
  atualizarStatus(id: string, novoStatus: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/status`, { statusDoPedido: novoStatus });
  }
}