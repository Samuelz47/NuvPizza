// Bate com o CupomDTO do backend
export interface Cupom {
    id: number;
    codigo: string;
    descontoPorcentagem: number;
    freteGratis: boolean;
    ativo: boolean;
}

// Bate com o CupomForRegistrationDTO do backend
export interface CupomForRegistration {
    codigo: string;
    descontoPorcentagem: number;
    freteGratis: boolean;
}
