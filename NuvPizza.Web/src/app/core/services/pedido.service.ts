import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
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

export interface PaginacaoMeta {
  totalCount: number;
  pageSize: number;
  totalPages: number;
  pageNumber: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/pedido`;

  // 1. Listar Pedidos (Usado no Painel Admin)
  getPedidos(pageNumber: number = 1, pageSize: number = 20, dataFiltro?: string): Observable<{ itens: Pedido[], paginacao: PaginacaoMeta | null }> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);

    if (dataFiltro) {
      // Envia a data selecionada como string de tempo local (sem o 'Z' de UTC)
      // Isso garante que o servidor compare com o horário local dele, evitando erros de fuso
      params = params.set('DataInicio', `${dataFiltro}T00:00:00`);
      params = params.set('DataFim', `${dataFiltro}T23:59:59`);
    }

    return this.http.get<Pedido[]>(this.apiUrl, { params, observe: 'response' }).pipe(
      map((response: HttpResponse<Pedido[]>) => {
        const paginacaoHeader = response.headers.get('X-Pagination');
        let paginacao: PaginacaoMeta | null = null;
        if (paginacaoHeader) {
          const parsed = JSON.parse(paginacaoHeader);
          // O backend retorna as chaves em PascalCase (TotalPages, etc). Mapeando para camelCase:
          paginacao = {
            totalCount: parsed.TotalCount,
            pageSize: parsed.PageSize,
            totalPages: parsed.TotalPages,
            pageNumber: parsed.PageNumber,
            hasNextPage: parsed.HasNextPage,
            hasPreviousPage: parsed.HasPreviousPage
          };
        }
        return { itens: response.body || [], paginacao };
      })
    );
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