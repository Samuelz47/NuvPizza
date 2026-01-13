export interface CriarPreferenciaRequest {
  titulo: string;
  quantidade: number;
  precoUnitario: number;
  emailPagador?: string;
  externalReference?: string; // <--- AGORA SIM: string (para aceitar o GUID)
}

export interface CriarPreferenciaResponse {
  url: string; // O link para onde vamos redirecionar o cliente
}