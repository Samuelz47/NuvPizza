import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { Subject } from 'rxjs';
// Importe a interface Pedido do seu service de pedidos
import { Pedido } from './pedido.service'; 

@Injectable({
  providedIn: 'root'
})
export class NotificacaoService {
  private hubConnection!: HubConnection;
  
  // A URL deve bater com o "app.MapHub" do Program.cs
  // Se sua API for http://localhost:5269, vira http://localhost:5269/notificacao
  private url = `${environment.apiUrl}/notificacao`; 

  // Canais para os componentes se inscreverem
  receberNovoPedido = new Subject<Pedido>();
  receberAtualizacaoStatus = new Subject<{id: string, status: number}>();

  iniciarConexao() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.url)
      .withAutomaticReconnect() // Tenta reconectar se a internet cair
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('🔌 SignalR Conectado!'))
      .catch(err => console.error('Erro ao conectar SignalR:', err));

    // --- OUVINTES (Devem ser iguais aos nomes no "SendAsync" do Backend) ---
    
    // 1. Quando chega pedido novo
    this.hubConnection.on('NovoPedidoRecebido', (pedido: Pedido) => {
      console.log('🔔 Novo Pedido via SignalR:', pedido);
      this.receberNovoPedido.next(pedido);
      this.tocarSom();
    });

    // 2. Quando o status muda (ex: Pagamento Aprovado)
    this.hubConnection.on('StatusPedidoAtualizado', (pedidoId: string, novoStatus: number) => {
      console.log('🔄 Status Atualizado via SignalR:', pedidoId, novoStatus);
      this.receberAtualizacaoStatus.next({ id: pedidoId, status: novoStatus });
      this.tocarSom();
    });
  }

  // "Cereja do Bolo": Tocar um sininho 🔔
  private tocarSom() {
    // Você pode baixar um mp3 curto e colocar em src/assets/alert.mp3
    // Ou usar um link externo para testar:
    const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
    audio.play().catch(err => console.warn('Navegador bloqueou o som automático (clique na tela primeiro).'));
  }
}