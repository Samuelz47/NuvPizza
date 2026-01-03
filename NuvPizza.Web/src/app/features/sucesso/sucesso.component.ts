import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser'; // <--- Importante

@Component({
  selector: 'app-sucesso',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './sucesso.html',
  styles: [`
    .sucesso-container { text-align: center; padding: 30px; font-family: sans-serif; max-width: 600px; margin: 0 auto; }
    .card { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
    h1 { color: #28a745; margin-bottom: 10px; }
    h1.pendente { color: #ffc107; }
    
    .qr-area { margin: 20px 0; padding: 15px; border: 2px dashed #ccc; border-radius: 8px; background: #f9f9f9; }
    .qr-img { max-width: 200px; margin-bottom: 15px; }
    textarea { width: 100%; height: 80px; padding: 10px; border: 1px solid #ddd; border-radius: 4px; font-size: 0.9rem; resize: none; }
    
    .btn-voltar { display: inline-block; margin-top: 20px; padding: 10px 20px; background: #009ee3; color: white; text-decoration: none; border-radius: 5px; cursor: pointer; border: none; }
  `]
})
export class SucessoComponent implements OnInit {
  private sanitizer = inject(DomSanitizer); // <--- Injeta o Sanitizer
  
  dados: any = null;
  qrCodeSeguro: SafeUrl | null = null; // <--- Variável para a imagem segura

  ngOnInit() {
    const navigation = history.state;
    
    // Verificação dupla: as vezes o dado vem direto, as vezes vem dentro de 'data' (dependendo do seu Result Wrapper no backend)
    // Se o seu backend retorna Result<T>, o dado real pode estar em navigation.dadosPagamento.data
    const payload = navigation.dadosPagamento;

    if (payload) {
        // Normaliza: se tiver propriedade .data usa ela, senão usa o objeto todo
        this.dados = payload.data || payload; 
        console.log('Dados recebidos e normalizados:', this.dados);

        // Se tiver QR Code, sanitiza a URL
        if (this.dados?.qrCodeBase64) {
            this.qrCodeSeguro = this.sanitizer.bypassSecurityTrustUrl(
                'data:image/png;base64,' + this.dados.qrCodeBase64
            );
        }
    }
  }

  copiarCodigo() {
    if (this.dados?.qrCodeCopiaCola) {
      navigator.clipboard.writeText(this.dados.qrCodeCopiaCola);
      alert('Código Pix copiado!');
    }
  }
}