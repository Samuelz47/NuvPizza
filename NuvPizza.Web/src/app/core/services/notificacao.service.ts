import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { Subject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificacaoService {
  private hubConnection!: HubConnection;

  // --- 1. AJUSTE DE URL ---
  // O Hub fica na raiz (ex: localhost:5269/notificacao), e não dentro de /api.
  // Removemos o "/api" do environment se ele estiver lá.
  private hubUrl = environment.apiUrl.replace('/api', '') + '/notificacao';

  // Subjects privados (quem emite os dados)
  private novoPedidoSource = new Subject<any>();
  private statusAtualizadoSource = new Subject<any>();

  constructor() {
    this.iniciarConexao();
  }

  private iniciarConexao() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('🔌 SignalR Conectado em:', this.hubUrl))
      .catch(err => console.error('❌ Erro ao conectar SignalR:', err));

    // --- 2. OUVINTES (Alinhados com o C#) ---

    // O Backend manda: "NovoPedidoRecebido", payload: PedidoDTO
    this.hubConnection.on('NovoPedidoRecebido', (pedido) => {
      console.log('🔔 Novo Pedido recebido:', pedido);
      this.tocarSom();
      this.novoPedidoSource.next(pedido);
    });

    // O Backend manda: "StatusPedidoAtualizado", payload: guid, int
    this.hubConnection.on('StatusPedidoAtualizado', (pedidoId: string, novoStatus: number) => {
      console.log(`🔄 Status mudou! ID: ${pedidoId} -> Status: ${novoStatus}`);
      
      // Emitimos um objeto único para facilitar pro componente ler
      this.statusAtualizadoSource.next({ pedidoId, novoStatus });
      
      // Só toca som se for status de "Pago" (1) ou "Pronto" (3), por exemplo
      if(novoStatus === 1 || novoStatus === 3) {
          this.tocarSom();
      }
    });
  }

  // --- 3. MÉTODOS PÚBLICOS (Para os Componentes) ---

  // O Painel Admin chama este:
  ouvirNovoPedido(): Observable<any> {
    return this.novoPedidoSource.asObservable();
  }

  // A Tela de Sucesso chama este:
  ouvirAtualizacaoStatus(): Observable<any> {
    return this.statusAtualizadoSource.asObservable();
  }

  private tocarSom() {
    const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
    audio.play().catch(err => console.warn('Som bloqueado pelo navegador (interaja com a página primeiro).'));
  }
}