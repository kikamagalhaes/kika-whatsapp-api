# Implantação no VPS

Projeto Compose exclusivo: `kika-whatsapp`, em `/opt/kika-whatsapp/source/deploy`.
O Traefik existente usa a rede do host e alcança a API pelo IP privado da rede
`kika-whatsapp_backend` (`172.16.9.0/24`, gateway `172.16.9.1`). A API usa a porta
5090 somente dentro do contêiner; nenhum Compose publica portas no host.

O Dockerfile compila em múltiplas etapas e executa a aplicação sem root. O
Compose define healthcheck HTTP, filesystem somente leitura, limites de recursos
e `restart: unless-stopped`. Não é necessário instalar .NET ou Nginx no host.

## Segredo de produção

O arquivo `/etc/kika-whatsapp/production.env` pertence a `root`, com permissão
`600`. Ele contém a variável `WhatsApp__VerifyToken`, definida pelo responsável.
Não copie o arquivo para o projeto, contexto de build ou Git. O Compose usa
`format: raw`: o valor deve estar em uma única linha, sem aspas delimitadoras,
e caracteres como `$` são preservados literalmente.

Não imprima `docker inspect` completo nem `docker compose config` sem `--quiet`,
pois esses comandos podem exibir o ambiente de produção. O log HTTP do ASP.NET
Core permanece em `Warning` para não registrar URLs com o token de verificação.

## Subir e validar internamente

Execute no diretório `deploy` do VPS:

```sh
docker compose -f compose.yml config --quiet
docker compose -f compose.yml -f compose.https.yml config --quiet
docker compose -f compose.yml build api
docker compose -f compose.yml up -d --no-deps --wait --wait-timeout 90 api
docker compose -f compose.yml ps
```

O Compose base mantém `traefik.enable=false`. Teste `/health` e os endpoints de
webhook pelo IP privado, usando `Host: api.kikamagalhaes.com.br`. O GET com token
válido deve retornar o challenge com status 200; o inválido, 403; POST JSON, 200.
Use o token apenas em memória, sem URLs com segredo em argumentos ou logs.

A rede tem `internal: true`: a API não recebe conectividade externa direta nem
saída para a internet. O Traefik permanece responsável pelo acesso público.

## Habilitar HTTPS após validar DNS

O registro A de `api.kikamagalhaes.com.br` deve apontar para `179.199.133.126`;
um eventual AAAA também deve alcançar este VPS. Se o DNS estiver incorreto,
pare antes de executar o comando seguinte. Não altere o DNS automaticamente.

```sh
docker compose -f compose.yml -f compose.https.yml up -d --no-deps --wait --wait-timeout 90 api
```

As labels usam `websecure` e `letsencrypt`, seguindo os serviços existentes.
Não há alteração global nem reinício do Traefik. Valide o certificado e repita
os testes em `https://api.kikamagalhaes.com.br/webhooks/whatsapp`.

Após habilitar HTTPS, mantenha ambos os arquivos em comandos futuros de `up`.
Usar somente o Compose base recria a API com o roteamento público desativado.
Para parar apenas a API, use `docker compose -f compose.yml stop api`.

O endpoint `/health` serve apenas para verificar disponibilidade. Nenhuma
configuração no painel Meta, envio de mensagens ou automação faz parte desta
implantação.
