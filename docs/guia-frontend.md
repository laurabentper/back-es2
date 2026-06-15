# Guia de Integracao do Front-End — CardioTrack

Este guia mostra, de forma pratica, como o front-end consome a API do CardioTrack:
fluxo de autenticacao, formato das requisicoes/respostas, enums, paginas tipicas e
tratamento de erros. O contrato formal esta em [`swagger.yaml`](swagger.yaml).

## Visao geral

- **Base URL (dev):** `http://localhost:5189`
- **Formato:** JSON em todas as requisicoes e respostas.
- **Datas:** ISO 8601. `dataNascimento` usa apenas a data (`yyyy-MM-dd`); os demais
  campos de data usam data/hora (`yyyy-MM-ddTHH:mm:ssZ`).
- **Enums:** trafegam como **texto** (ex.: `"Masculino"`), nunca como numero.
- **Autenticacao:** JWT Bearer nas rotas protegidas.
- **CORS:** liberado para as origens em `Cors:AllowedOrigins` (por padrao
  `http://localhost:3000`, `http://localhost:5173` e `http://localhost:8100`).
  Adicione a origem do seu front-end nessa lista.

## Fluxo de autenticacao

```
[Cadastro]  POST /api/usuarios            -> 201 (dados do usuario)
[Login]     POST /api/usuarios/login      -> 200 { token, expiraEm, usuario }
[Uso]       Authorization: Bearer <token> nas rotas protegidas
```

1. O usuario se cadastra (ou ja possui conta).
2. Faz login e recebe `token` + `expiraEm`.
3. O front-end guarda o token (ex.: memoria/`localStorage`) e o envia no cabecalho
   `Authorization` das chamadas protegidas.
4. Ao receber `401`, redirecione para a tela de login (token ausente/expirado).

O token expira conforme `Jwt:ExpirationMinutes` (padrao **480 min / 8h**). Use
`expiraEm` para antecipar a renovacao (novo login). Nao ha endpoint de refresh.

## Enums

| Campo | Valores aceitos |
|-------|-----------------|
| `sexo` | `Masculino`, `Feminino`, `Outro`, `PrefiroNaoInformar` |

Sintomas **nao** sao enum: chegam e retornam como booleanos independentes
(`faltaDeAr`, `dorNoPeito`, `tontura`).

## Endpoints

### 1. Cadastro — `POST /api/usuarios` (publico)

Requisicao:

```json
{
  "nome": "Maria",
  "sobrenome": "Silva",
  "email": "maria.silva@example.com",
  "telefone": "+55 11 99999-0000",
  "senha": "senhaForte123",
  "confirmacaoSenha": "senhaForte123",
  "dataNascimento": "1990-05-20",
  "sexo": "Feminino",
  "paisResidencia": "Brasil"
}
```

Resposta `201 Created`:

```json
{
  "id": "3f1c...-uuid",
  "nome": "Maria",
  "sobrenome": "Silva",
  "nomeCompleto": "Maria Silva",
  "email": "maria.silva@example.com",
  "telefone": "+55 11 99999-0000",
  "dataNascimento": "1990-05-20",
  "sexo": "Feminino",
  "paisResidencia": "Brasil",
  "criadoEm": "2026-06-03T12:00:00Z"
}
```

Regras de validacao (retornam `400`; e-mail repetido retorna `409`):

| Campo | Regra |
|-------|-------|
| `nome`, `sobrenome` | obrigatorio, ate 100 caracteres |
| `email` | obrigatorio, formato valido, ate 256, **unico** |
| `telefone` | obrigatorio, ate 30 |
| `senha` | obrigatoria, **minimo 8** caracteres |
| `confirmacaoSenha` | deve ser **igual** a `senha` |
| `dataNascimento` | deve estar no passado |
| `sexo` | um dos valores do enum |
| `paisResidencia` | obrigatorio, ate 100 |

### 2. Login — `POST /api/usuarios/login` (publico)

Requisicao:

```json
{ "email": "maria.silva@example.com", "senha": "senhaForte123" }
```

Resposta `200 OK`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEm": "2026-06-03T20:00:00Z",
  "usuario": { "id": "3f1c...-uuid", "nomeCompleto": "Maria Silva", "...": "..." }
}
```

Credenciais invalidas retornam `401` (sem indicar se foi o e-mail ou a senha).

### 3. Registrar medicao — `POST /api/medicoes` (Bearer)

A medicao e sempre vinculada ao usuario do token — **nao** envie `usuarioId`.

Requisicao:

```json
{
  "pressaoSistolica": 120,
  "pressaoDiastolica": 80,
  "frequenciaCardiaca": 72,
  "oxigenacaoSangue": 98,
  "pesoCorporal": 70.5,
  "faltaDeAr": false,
  "dorNoPeito": false,
  "tontura": false,
  "registradaEm": null
}
```

`registradaEm` e opcional: omita (ou envie `null`) para usar o instante atual.

Resposta `201 Created`:

```json
{
  "id": "9a2b...-uuid",
  "usuarioId": "3f1c...-uuid",
  "pressaoSistolica": 120,
  "pressaoDiastolica": 80,
  "frequenciaCardiaca": 72,
  "oxigenacaoSangue": 98,
  "pesoCorporal": 70.5,
  "sintomas": { "faltaDeAr": false, "dorNoPeito": false, "tontura": false },
  "possuiSintomas": false,
  "registradaEm": "2026-06-03T12:00:00Z",
  "criadaEm": "2026-06-03T12:00:00Z"
}
```

Faixas validas (fora delas, `400`):

| Campo | Faixa |
|-------|-------|
| `pressaoSistolica` | 50–300 mmHg (e **maior** que a diastolica) |
| `pressaoDiastolica` | 30–200 mmHg |
| `frequenciaCardiaca` | 20–250 bpm |
| `oxigenacaoSangue` | 50–100 % |
| `pesoCorporal` | 0,5–500 kg |
| `registradaEm` | nao pode estar no futuro |

### 4. Atualizar medicao — `PUT /api/medicoes/{id}` (Bearer)

Substitui **integralmente** os valores de uma medicao existente. Envie o corpo
completo (mesmos campos e faixas do cadastro) — campos omitidos voltam ao padrao.
A medicao precisa pertencer ao usuario do token; caso contrario a API responde
`404` (o mesmo de uma medicao inexistente, para nao revelar dados de terceiros).

`PUT /api/medicoes/9a2b...-uuid`

Requisicao:

```json
{
  "pressaoSistolica": 130,
  "pressaoDiastolica": 85,
  "frequenciaCardiaca": 75,
  "oxigenacaoSangue": 97,
  "pesoCorporal": 71.0,
  "faltaDeAr": false,
  "dorNoPeito": true,
  "tontura": false,
  "registradaEm": null
}
```

Resposta `200 OK`: a `MedicaoResposta` atualizada (mesmo formato do cadastro). O
`id`, o `usuarioId` e o `criadaEm` permanecem inalterados.

### 5. Remover medicao — `DELETE /api/medicoes/{id}` (Bearer)

Remove a medicao do usuario autenticado. Mesma regra de propriedade da
atualizacao: `404` quando a medicao nao existe ou e de outro usuario.

`DELETE /api/medicoes/9a2b...-uuid`

Resposta `204 No Content` (sem corpo) quando removida com sucesso.

### 6. Historico — `GET /api/relatorios/historico` (Bearer)

Parametros de query opcionais: `inicio` e `fim` (data/hora ISO). Sem eles, retorna
todo o historico. Ordenado da medicao **mais recente para a mais antiga**.

`GET /api/relatorios/historico?inicio=2026-01-01T00:00:00Z&fim=2026-06-30T23:59:59Z`

Resposta `200 OK`:

```json
{
  "total": 2,
  "medicoes": [
    { "id": "...", "pressaoSistolica": 120, "...": "..." },
    { "id": "...", "pressaoSistolica": 118, "...": "..." }
  ]
}
```

Cada item de `medicoes` tem o mesmo formato de `MedicaoResposta` (ver acima).

### 7. Resumo — `GET /api/relatorios/resumo` (Bearer)

Mesmos parametros `inicio`/`fim`. Pensado para alimentar graficos.

Resposta `200 OK` (com medicoes no periodo):

```json
{
  "totalMedicoes": 10,
  "primeiraMedicaoEm": "2026-01-05T08:00:00Z",
  "ultimaMedicaoEm": "2026-06-01T09:30:00Z",
  "pressaoSistolica": { "media": 121.4, "minimo": 110, "maximo": 135 },
  "pressaoDiastolica": { "media": 79.2, "minimo": 70, "maximo": 88 },
  "frequenciaCardiaca": { "media": 73.5, "minimo": 60, "maximo": 90 },
  "oxigenacaoSangue": { "media": 97.8, "minimo": 95, "maximo": 99 },
  "pesoCorporal": { "media": 70.32, "minimo": 69, "maximo": 72 },
  "sintomas": {
    "faltaDeAr": 1,
    "dorNoPeito": 0,
    "tontura": 2,
    "comAlgumSintoma": 3,
    "semSintomas": 7
  }
}
```

> **Periodo sem medicoes:** `totalMedicoes` = 0, `primeiraMedicaoEm`/
> `ultimaMedicaoEm` = `null`, cada bloco de estatistica = `null` e todos os
> contadores de `sintomas` = 0. O front-end deve tratar esse caso (estado vazio).
> As medias vem arredondadas para 2 casas.

## Tratamento de erros

Erros usam o formato `ProblemDetails` (`application/problem+json`):

```json
{
  "status": 409,
  "title": "Conflito",
  "detail": "Ja existe uma conta cadastrada com este e-mail."
}
```

Erros de validacao (`400`) incluem a lista `erros`, com um item por falha
contendo `campo`, `valorInformado` e `mensagem` (o valor de campos sensiveis,
como senha, vem omitido como `***`):

```json
{
  "status": 400,
  "title": "Um ou mais campos sao invalidos.",
  "erros": [
    {
      "campo": "Senha",
      "valorInformado": "***",
      "mensagem": "A senha deve ter ao menos 8 caracteres."
    },
    {
      "campo": "ConfirmacaoSenha",
      "valorInformado": "***",
      "mensagem": "A confirmacao de senha nao corresponde a senha."
    }
  ]
}
```

Resumo dos status:

| Status | Significado | Acao sugerida no front-end |
|:---:|-------------|----------------------------|
| `400` | Validacao | Exibir mensagens por campo a partir de `erros`. |
| `401` | Nao autenticado | Voltar para o login. |
| `404` | Nao encontrado | Mensagem de recurso inexistente. |
| `409` | Conflito | Exibir `detail` (ex.: e-mail ja cadastrado). |
| `500` | Erro interno | Mensagem generica e retry. |

## Exemplos com `fetch`

```js
const BASE = "http://localhost:5189";

// Login
async function login(email, senha) {
  const resp = await fetch(`${BASE}/api/usuarios/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, senha }),
  });
  if (!resp.ok) throw await resp.json(); // ProblemDetails
  return resp.json(); // { token, expiraEm, usuario }
}

// Chamada autenticada
async function registrarMedicao(token, medicao) {
  const resp = await fetch(`${BASE}/api/medicoes`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(medicao),
  });
  if (resp.status === 401) {
    // token ausente/expirado -> redirecionar para login
  }
  if (!resp.ok) throw await resp.json();
  return resp.json();
}

// Resumo para graficos
async function obterResumo(token, inicio, fim) {
  const params = new URLSearchParams();
  if (inicio) params.set("inicio", inicio);
  if (fim) params.set("fim", fim);
  const resp = await fetch(`${BASE}/api/relatorios/resumo?${params}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!resp.ok) throw await resp.json();
  return resp.json();
}
```

## Sugestao de telas

| Tela | Endpoint(s) |
|------|-------------|
| Cadastro | `POST /api/usuarios` |
| Login | `POST /api/usuarios/login` |
| Nova medicao | `POST /api/medicoes` |
| Editar/remover medicao | `PUT /api/medicoes/{id}`, `DELETE /api/medicoes/{id}` |
| Historico (lista/tabela) | `GET /api/relatorios/historico` |
| Dashboard (graficos) | `GET /api/relatorios/resumo` |
