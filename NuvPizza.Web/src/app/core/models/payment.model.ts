export interface Identification {
  type: string;
  number: string;
}

export interface Payer {
  email: string;
  phone: string;
  firstName: string;
  identification: Identification;
}

export interface PagamentoRequest {
  pedidoId: number;
  transactionAmount: number;
  token: string;
  description: string;
  paymentMethodId: string;
  installments: number;
  payer: Payer;
  issuerId?: string;
}

export interface PagamentoResponse {
  paymentId: number;
  status: string;
  statusDetail: string;
  qrCodeBase64?: string;
  qrCodeCopiaCola?: string;
}