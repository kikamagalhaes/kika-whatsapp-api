# Kika WhatsApp API

API em **C# / ASP.NET Core** para receber webhooks do WhatsApp (Meta Cloud API): verificação do webhook (GET com challenge) e recebimento de eventos (POST JSON). Inclui health check, Swagger em desenvolvimento e deploy pronto para Docker + Traefik.

## Endpoints

- `GET /health` — verificação de saúde.
- `GET /webhooks/whatsapp` — verificação do webhook (retorna o challenge se o token confere, senão 403).
- `POST /webhooks/whatsapp` — recebimento de eventos do WhatsApp.

## Configuração (segredos fora do código)

O token de verificação **nunca** vai para o Git. Configure por variável de ambiente:

```bash
WhatsApp__VerifyToken=seu_token_aqui
```

- Desenvolvimento: use User Secrets (`dotnet user-secrets`) ou variável de ambiente.
- Produção: arquivo `/etc/kika-whatsapp/production.env` (permissão 600) + Docker Compose em `deploy/`.

## Executar localmente

Requisitos: SDK .NET 8.

```bash
dotnet run
```

A API sobe em `http://localhost:5024` (Swagger em desenvolvimento).

## Deploy

Ver `deploy/README.md`: Compose com filesystem somente leitura, execução sem root, healthcheck, limites de recursos e labels do Traefik para HTTPS com Let's Encrypt.

## Stack

C# 12, ASP.NET Core 8, Docker multi-stage, Traefik, x-forwarded headers para proxy reverso.
