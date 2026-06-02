# CardioTrack API

API REST de **acompanhamento de saude cardiaca**. Permite que um usuario crie uma
conta, autentique-se, registre medicoes (pressao arterial, frequencia cardiaca,
oxigenacao do sangue, peso corporal e sintomas) e consulte relatorios o
historico das medicoes e um resumo agregado pronto para alimentar graficos no
front-end.


## Sumario

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Como executar](#como-executar)
- [Configuracao](#configuracao)
- [Endpoints](#endpoints)
- [Autenticacao](#autenticacao)
- [Tratamento de erros](#tratamento-de-erros)
- [Testes](#testes)
- [Documentacao da API](#documentacao-da-api)

## Tecnologias

- **.NET 9** (ASP.NET Core Web API)
- **Entity Framework Core 9** com provider **MySQL** (Pomelo)
- **MySQL 8** (executado via Docker durante o desenvolvimento)
- **JWT** para autenticacao (Bearer)
- **FluentValidation** para validacao de entrada
- **Swagger / OpenAPI** para documentacao
- **xUnit** para testes; **Testcontainers** para os testes de integracao

## Arquitetura

Arquitetura em camadas, com cada camada em um projeto separado dentro da solution.
A separacao deixa a modularizacao explicita e facilita os testes. As dependencias
apontam sempre para dentro (API -> Application -> Domain), com a Infrastructure
implementando os contratos definidos na Application.

| Projeto | Responsabilidade |
|---------|------------------|
| `CardioTrack.Domain` | Entidades e regras de negocio puras (`Usuario`, `Medicao`, enums e invariantes). |
| `CardioTrack.Application` | Casos de uso, DTOs, contratos de repositorio/seguranca, servicos de aplicacao e validacoes. |
| `CardioTrack.Infrastructure` | EF Core, `DbContext`, repositorios, migrations, geracao de token JWT e hashing de senha. |
| `CardioTrack.Api` | Controllers, middleware de erros, configuracao de Swagger, JWT e CORS. |
| `CardioTrack.Tests.Unit` | Testes unitarios das regras de dominio e servicos. |
| `CardioTrack.Tests.Integration` | Testes dos endpoints contra um MySQL real (Testcontainers). |

Dentro de cada camada o codigo e organizado por funcionalidade (**Usuarios**,
**Medicoes** e **Relatorios**), reforcando a modularizacao tambem por dominio.

## Estrutura de pastas

```
back-es2/
├── CardioTrack.slnx              # Solution
├── docker-compose.yml            # MySQL 8 para desenvolvimento local
├── docs/
│   ├── swagger.yaml              # Especificacao OpenAPI 3.0
│   └── guia-frontend.md          # Guia de integracao para o front-end
├── src/
│   ├── CardioTrack.Domain/
│   ├── CardioTrack.Application/
│   ├── CardioTrack.Infrastructure/
│   └── CardioTrack.Api/
└── tests/
    ├── CardioTrack.Tests.Unit/
    └── CardioTrack.Tests.Integration/
```

## Como executar

### Pre-requisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para o MySQL local e os testes de integracao)

### 1. Subir o MySQL

O `docker-compose.yml` provisiona um MySQL 8 local (banco `cardiotrack`, usuario
`cardiotrack` / senha `cardiotrack`, porta `3306`):

```bash
docker compose up -d
```

### 2. Configurar a connection string

Por padrao a API le a connection string em `ConnectionStrings:CARDIOTRACK_CONNECTION`
(ver [Configuracao](#configuracao)). Para apontar para o MySQL local, exporte a
variavel de ambiente antes de rodar (sobrepoe o `appsettings.json`):

```powershell
# PowerShell (Windows)
$env:ConnectionStrings__CARDIOTRACK_CONNECTION = "Server=localhost;Port=3306;Database=cardiotrack;User Id=cardiotrack;Password=cardiotrack;"
```

```bash
# bash (Linux/macOS)
export ConnectionStrings__CARDIOTRACK_CONNECTION="Server=localhost;Port=3306;Database=cardiotrack;User Id=cardiotrack;Password=cardiotrack;"
```

### 3. Aplicar as migrations

As migrations **nao** sao aplicadas automaticamente ao iniciar a API. Crie o schema
com o EF Core CLI:

```bash
dotnet tool install --global dotnet-ef        # caso ainda nao tenha
dotnet ef database update \
  --project src/CardioTrack.Infrastructure \
  --startup-project src/CardioTrack.Api
```

### 4. Rodar a API

```bash
dotnet run --project src/CardioTrack.Api
```

A API sobe em `http://localhost:5189` (e `https://localhost:7045`). Em ambiente de
desenvolvimento, o Swagger UI fica disponivel em
`http://localhost:5189/swagger`.

## Configuracao

As configuracoes ficam em `src/CardioTrack.Api/appsettings.json` e podem ser
sobrepostas por variaveis de ambiente (use `__` como separador de secao).

| Chave | Descricao | Padrao |
|-------|-----------|--------|
| `ConnectionStrings:CARDIOTRACK_CONNECTION` | Connection string do MySQL. | — |
| `Jwt:Issuer` | Emissor do token. | `CardioTrack.Api` |
| `Jwt:Audience` | Audiencia do token. | `CardioTrack.Client` |
| `Jwt:SecretKey` | Chave de assinatura HMAC-SHA256 (**minimo 32 bytes**). | — |
| `Jwt:ExpirationMinutes` | Validade do token, em minutos. | `480` |
| `Cors:AllowedOrigins` | Lista de origens liberadas para o front-end. | `http://localhost:3000`, `http://localhost:5173` |

> **Atencao:** `Jwt:SecretKey` e a connection string contem segredos. Em producao,
> defina-os por variavel de ambiente (ou cofre de segredos) em vez de versiona-los
> no `appsettings.json`.

## Endpoints

Prefixo base: `/api`. Respostas em `application/json`. Datas em formato ISO 8601.

| Metodo | Rota | Autenticacao | Descricao |
|--------|------|:---:|-----------|
| `POST` | `/api/usuarios` | publico | Cadastra uma nova conta. |
| `POST` | `/api/usuarios/login` | publico | Autentica por e-mail e senha e retorna um JWT. |
| `POST` | `/api/medicoes` | Bearer | Registra uma medicao para o usuario autenticado. |
| `PUT` | `/api/medicoes/{id}` | Bearer | Atualiza uma medicao do usuario autenticado. |
| `DELETE` | `/api/medicoes/{id}` | Bearer | Remove uma medicao do usuario autenticado. |
| `GET` | `/api/relatorios/historico` | Bearer | Historico de medicoes (mais recente primeiro). |
| `GET` | `/api/relatorios/resumo` | Bearer | Resumo agregado das medicoes para graficos. |

Os endpoints de relatorio aceitam os parametros opcionais de query `inicio` e
`fim` (data/hora) para filtrar o periodo; quando ausentes, consideram todo o
historico. A medicao e sempre associada ao usuario dono do token — nunca a um id
informado no corpo.

O contrato completo (corpos de requisicao/resposta, codigos de status e schemas)
esta em [`docs/swagger.yaml`](docs/swagger.yaml) e no
[guia do front-end](docs/guia-frontend.md).

## Autenticacao

1. O cliente cria a conta em `POST /api/usuarios` e faz login em
   `POST /api/usuarios/login`.
2. O login retorna `token` (JWT) e `expiraEm`.
3. Nas rotas protegidas, envie o cabecalho `Authorization: Bearer <token>`.

O token e assinado com HMAC-SHA256 e carrega as claims `sub` (id do usuario),
`email`, `name` (nome completo) e `jti`. Emissor, audiencia, assinatura e validade
sao validados a cada requisicao (com tolerancia de relogio de 1 minuto).

## Tratamento de erros

Erros sao retornados como
[`ProblemDetails`](https://datatracker.ietf.org/doc/html/rfc7807)
(`application/problem+json`). Falhas de validacao usam `ValidationProblemDetails`,
com o dicionario `errors` (campo -> lista de mensagens).

| Status | Quando ocorre |
|:---:|---------------|
| `400 Bad Request` | Dados invalidos (validacao). |
| `401 Unauthorized` | Credenciais invalidas ou token ausente/invalido. |
| `404 Not Found` | Recurso inexistente. |
| `409 Conflict` | Conflito de estado (ex.: e-mail ja cadastrado). |
| `500 Internal Server Error` | Falha inesperada (sem vazar detalhes internos). |

## Testes

```bash
# Testes unitarios (nao dependem de Docker)
dotnet test tests/CardioTrack.Tests.Unit

# Testes de integracao (exigem Docker em execucao)
dotnet test tests/CardioTrack.Tests.Integration

# Toda a suite
dotnet test
```

Os testes de integracao sobem a API em memoria contra um MySQL real provisionado
em um contêiner efemero (Testcontainers), exercitando o mesmo provider usado em
producao, incluindo migrations e conversoes.

## Documentacao da API

- **Swagger UI** (em desenvolvimento): `http://localhost:5189/swagger`
- **OpenAPI (arquivo):** [`docs/swagger.yaml`](docs/swagger.yaml)
