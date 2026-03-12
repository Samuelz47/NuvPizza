# 🍕 NuvPizza

![Status CI](https://github.com/Samuelz47/NuvPizza/actions/workflows/ci.yml/badge.svg)
![Status CD](https://github.com/Samuelz47/NuvPizza/actions/workflows/deploy-api.yml/badge.svg)

**NuvPizza** é uma plataforma moderna e completa de delivery para pizzarias. O sistema gerencia desde a escolha dos sabores e bordas até o pagamento online e notificações em tempo real para a cozinha e administração.

Este projeto foi desenvolvido com foco em performance, segurança e escalabilidade, estando atualmente em uso em produção.

## 🚀 Tecnologias

### Backend
- **.NET 8 (C#)** — Web API com foco em alta performance.
- **Clean Architecture** — Separação clara de responsabilidades (Domain, Application, Infrastructure, API).
- **Entity Framework Core** — ORM para persistência de dados com SQLite.
- **Identity & JWT** — Sistema robusto de autenticação e autorização via Bearer Token.
- **SignalR** — Comunicação em tempo real para notificações instantâneas de novos pedidos.
- **Redis** — Cache distribuído para otimização de consultas frequentes.
- **Serilog** — Logs estruturados para monitoramento.
- **Health Checks** — Monitoramento de integridade da API e integrações externas.
- **Rate Limiting** — Proteção contra spam e ataques de força bruta no checkout e login.

### Frontend
- **Angular 21** — Framework moderno para uma interface Single Page Application (SPA) veloz.
- **SignalR Client** — Atualizações automáticas no painel administrativo sem necessidade de refresh.
- **Tailwind CSS/Sass** — Interface moderna, limpa e responsiva.

## 🔗 Integrações

O sistema se conecta com diversos serviços externos para automatizar a operação:
- **Mercado Pago** — Processamento de pagamentos online (Pix, Cartão).
- **WhatsApp API** — Geração automática de links de pedido para comunicação direta.
- **Gmail (SMTP)** — Envio de confirmações de pedidos por e-mail via conta oficial.
- **ViaCep** — Busca automática de endereço a partir do CEP.

## 🛠️ Estrutura do Projeto

- `NuvPizza.API`: Ponto de entrada da aplicação, controladores e middlewares.
- `NuvPizza.Application`: Lógica de negócio, serviços, DTOs e mapeamentos.
- `NuvPizza.Domain`: Entidades, enums e interfaces fundamentais.
- `NuvPizza.Infrastructure`: Implementações de acesso a dados (EF Core) e serviços externos.
- `NuvPizza.Tests`: Suíte de testes unitários e de integração com xUnit.
- `NuvPizza.Web`: Frontend Angular completo.

## 📦 Deploy & CI/CD

O fluxo de entrega é automatizado via **GitHub Actions**:
- **Integração Contínua (CI)**: Build e testes automáticos em cada Push ou Pull Request.
- **Entrega Contínua (CD)**: Deploy automático do Backend na VPS **Hostinger** ao realizar push na branch `main`.
- **Frontend**: Hospedado nativamente no **Vercel** com deploy automático.

## 🚦 Monitoramento de Saúde

A API possui endpoints de Health Check:
- `/health`: Status simplificado para bots de monitoramento (ex: UptimeRobot).
- `/health/details`: Status detalhado de cada componente (Banco, Redis, APIs Externas).

## 📝 Licença

Este projeto é de uso privado da NuvPizza. Maiores informações entrar em contato com o administrador.
