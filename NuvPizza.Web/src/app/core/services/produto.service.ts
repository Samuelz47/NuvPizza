import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Produto } from '../models/produto.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProdutoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/produtos`; // Certifique-se que o environment tem a URL certa

  getAll() {
    return this.http.get<Produto[]>(this.apiUrl);
  }

  getById(id: number) {
    return this.http.get<Produto>(`${this.apiUrl}/${id}`);
  }

  // Criar produto com imagem requer FormData
  create(produto: any, arquivo: File | null): Observable<Produto> {
    const formData = new FormData();

    // Adiciona os campos de texto
    formData.append('nome', produto.nome);
    formData.append('descricao', produto.descricao);
    // Garante formato com vírgula para backends PT-BR
    formData.append('preco', produto.preco.toString().replace('.', ','));
    formData.append('categoria', produto.categoria.toString());
    formData.append('tamanho', produto.tamanho.toString());
    formData.append('ativo', produto.ativo ? 'true' : 'false');

    // Se tiver imagem, adiciona
    if (arquivo) {
      formData.append('imagem', arquivo);
    }

    return this.http.post<Produto>(this.apiUrl, formData);
  }

  update(id: number, produto: any, arquivo: File | null): Observable<void> {
    const formData = new FormData();
    formData.append('id', id.toString());
    formData.append('nome', produto.nome);
    formData.append('descricao', produto.descricao);
    // Garante formato com vírgula para backends PT-BR
    formData.append('preco', produto.preco.toString().replace('.', ','));
    formData.append('categoria', produto.categoria.toString());
    formData.append('tamanho', produto.tamanho.toString());
    formData.append('ativo', produto.ativo ? 'true' : 'false');

    if (arquivo) {
      formData.append('imagem', arquivo);
    }

    return this.http.put<void>(`${this.apiUrl}/${id}`, formData);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}