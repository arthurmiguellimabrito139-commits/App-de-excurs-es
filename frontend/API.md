# Contrato da API

Base: `VITE_API_URL` (desenvolvimento: `http://localhost:5002/api`).
JSON em **camelCase**. Todo erro vem como `{ "erro": "mensagem" }`.
Rotas protegidas recebem `Authorization: Bearer <token>`.

O modo simulado (`src/mocks/mockApi.js`) devolve os mesmos formatos.

## Status usados

| Status | Quando |
|---|---|
| 200 / 201 / 204 | deu certo |
| 400 | regra quebrada ou dado inválido (a mensagem diz qual) |
| 401 | sem token, token inválido/vencido, ou login errado — o front faz logout |
| 403 | logado, mas sem o papel certo (ex.: passageiro tentando criar viagem) |
| 404 | não existe **ou não é seu** (a API não diferencia, de propósito) |
| 409 | alguém alterou o mesmo dado ao mesmo tempo (ex.: última vaga) — tente de novo |
| 413 | arquivos grandes demais |
| 429 | muitas tentativas — aguarde um minuto |

---

## Auth

| Método | Rota | Quem | Corpo |
|---|---|---|---|
| POST | `/auth/login` | público | `{ email, senha }` |
| POST | `/auth/cadastro` | público | `{ nome, email, senha }` — senha de 8 a 72 caracteres; **sempre cria Passageiro** |

Login e cadastro: no máximo 5 por minuto por IP.

```json
{
  "token": "eyJhbGciOi...",
  "usuario": { "id": 2, "nome": "Maria", "email": "maria@x.com", "tipo": "Passageiro" }
}
```

`tipo`: `"Admin"` ou `"Passageiro"`. O token vale 60 minutos. O admin é criado pela configuração da API, nunca pelo cadastro.

---

## Viagens

| Método | Rota | Quem |
|---|---|---|
| GET | `/viagens` | público |
| GET | `/viagens?disponiveis=true` | público — só com vaga e saída futura |
| GET | `/viagens/{id}` | público |
| POST | `/viagens` | Admin — `multipart/form-data` |
| PUT | `/viagens/{id}` | Admin dono — `multipart/form-data` |
| DELETE | `/viagens/{id}` | Admin dono — 204; 400 se já houver passagens vendidas |
| GET | `/viagens/{id}/passageiros` | Admin dono |

```json
{
  "id": 1, "adminId": 1,
  "titulo": "Chapada Diamantina", "destino": "Lençóis, BA", "descricao": "...",
  "dataSaida": "2026-12-04", "dataRetorno": "2026-12-08",
  "valor": 1890.00, "vagas": 40, "vagasDisponiveis": 39, "maxParcelas": 10,
  "imagens": [{ "id": 1, "url": "http://localhost:5002/uploads/3f2a...c1.jpg" }]
}
```

Formulário (POST/PUT): `titulo`, `destino`, `descricao`, `dataSaida`, `dataRetorno`, `valor` (ponto decimal: `1890.50`), `vagas`, `maxParcelas` (1–12), `imagens` (vários arquivos, mesma chave) e, no PUT, `imagensRemover` (vários ids).
Imagens: só JPG, PNG ou WEBP de verdade (o conteúdo é conferido), até 5 MB cada e 10 por viagem.

`/viagens/{id}/passageiros`:
```json
[{
  "compraId": 1,
  "passageiro": { "id": 2, "nome": "Maria", "email": "maria@x.com" },
  "metodo": "boleto", "numParcelas": 3, "parcelasPagas": 1,
  "valorTotal": 1890.00, "valorPago": 630.00,
  "status": "pendente", "criadoEm": "2026-11-01T16:50:11Z"
}]
```

---

## Pagamentos (Strategy)

| Método | Rota | Quem |
|---|---|---|
| GET | `/pagamentos/metodos` | público |
| GET | `/viagens/{id}/simular?metodo=cartao&parcelas=6` | público |
| POST | `/viagens/{id}/comprar` | Passageiro — `{ metodo, numParcelas }` |
| GET | `/compras/minhas` | Passageiro |
| POST | `/parcelas/{id}/pagar` | Passageiro dono — pagamento **simulado**, só a próxima parcela em aberto |

`metodo`: `"pix"` (à vista, 5% de desconto), `"cartao"` (até 3x sem juros, depois 2,5% a.m.) ou `"boleto"` (sem juros).

Simulação:
```json
{ "metodo": "cartao", "numParcelas": 6, "maxParcelas": 10, "total": 2191.82,
  "parcelas": [{ "numero": 1, "valor": 365.30, "vencimento": "2026-11-01" }] }
```

Compra (resposta de comprar e itens de `/compras/minhas`):
```json
{
  "id": 1, "metodo": "cartao", "numParcelas": 6, "valorTotal": 2191.82,
  "status": "pendente", "criadoEm": "2026-11-01T16:50:11Z",
  "viagem": { "id": 1, "titulo": "...", "destino": "...", "dataSaida": "2026-12-04", "imagem": "http://..." },
  "parcelas": [{ "id": 10, "numero": 1, "valor": 365.30, "vencimento": "2026-11-01", "status": "aberta", "pagoEm": null }]
}
```

`status` da compra: `pendente | pago | cancelado`. `status` da parcela: `aberta | paga`.
O valor **nunca** vem do front: a API busca a viagem no banco e calcula com a strategy.
