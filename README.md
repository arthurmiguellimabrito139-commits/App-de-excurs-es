# Embarque — app de excursões

Sistema de venda de excursões com **passageiros** e **administradores**. O admin publica viagens (valor, fotos, descrição, vagas, parcelas); o passageiro vê as viagens e compra com **Pix, cartão ou boleto**, parcelado.

- **Back-end:** C# / ASP.NET Core 10, Dapper (SQL puro), MySQL 8, JWT, BCrypt
- **Front-end:** React + Vite + styled-components (`frontend/`)
- **Segurança:** os 21 itens do checklist de publicação, cada um com teste automatizado — veja **[SEGURANCA.md](SEGURANCA.md)**

## Arquitetura

```
HTTP → Controller → Service → Model (regras) → Repository (SQL) → MySQL
```

| Pasta | O que tem |
|---|---|
| `Models/` | **Model rico**: as regras moram nas classes (`Viagem.Vender`, `Compra.PagarParcela`, `Admin.EditarViagem`). `Usuario` é abstrato; `Admin` e `Passageiro` herdam dele. |
| `Strategies/Pagamento/` | **Strategy** (Pix, Cartão, Boleto) + **Template Method** na classe base: cada forma de pagamento só implementa o cálculo do total. |
| `Repositories/` | Todo o SQL, com Dapper e parâmetros. Transação e controle de concorrência na compra. |
| `Services/` | Coordenam: buscam os objetos, chamam as regras do model, pedem para o repositório gravar. Permissões ficam aqui. |
| `Controllers/` | Só HTTP: pegam o usuário do token, chamam o service, devolvem o status. |
| `Configuracao/` | Montagem da API e toda a configuração de segurança num lugar só. |
| `Data/schema.sql` | Estrutura do banco. |

Uma compra, por dentro: o `Passageiro` confere se já comprou, a `Viagem` confere a vaga e pede à **strategy** o cálculo das parcelas, e o repositório grava tudo numa **transação** que também protege a última vaga contra duas compras simultâneas.

---

## Como rodar (Windows / PowerShell)

Pré-requisitos: **.NET 10 SDK**, **MySQL 8**, **Node 20+**.
Todos os comandos partem da pasta `Excursoes` (onde está o `Excursoes.slnx`).

### 1. Banco de dados

No MySQL Workbench, como **root**:

```sql
CREATE DATABASE excursoes CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- usuário da API: só lê e grava dados, não pode criar nem apagar tabelas
CREATE USER 'excursoes_app'@'localhost' IDENTIFIED BY 'TroqueEstaSenha2026';
GRANT SELECT, INSERT, UPDATE, DELETE ON excursoes.* TO 'excursoes_app'@'localhost';
FLUSH PRIVILEGES;
```

Depois crie as tabelas com o script (no Workbench: *File → Open SQL Script → `Excursoes.Api/Data/schema.sql`* e clique no raio), ou:

```powershell
Get-Content Excursoes.Api/Data/schema.sql | mysql -u root -p
```

### 2. Segredos (fora do código e fora do Git)

```powershell
cd Excursoes.Api
dotnet user-secrets set "ConnectionStrings:Padrao" "server=localhost;port=3306;database=excursoes;user=excursoes_app;password=TroqueEstaSenha2026"
dotnet user-secrets set "Jwt:Chave" "$(New-Guid)$(New-Guid)"
dotnet user-secrets set "Admin:Email" "seu-email@exemplo.com"
dotnet user-secrets set "Admin:Senha" "UmaSenhaForteDeAdmin123"
dotnet user-secrets set "Admin:Nome" "Seu Nome"
cd ..
```

- A `Jwt:Chave` precisa de 32+ caracteres (duas GUIDs aleatórias dão 72). Sem ela a API **não inicia**, de propósito.
- O admin é criado automaticamente na primeira execução (senha com 12+ caracteres). O cadastro público só cria passageiros.

### 3. API

```powershell
dotnet run --project Excursoes.Api
```

Deve aparecer `Conectado ao MySQL.` e `Administrador inicial criado`. A API fica em `http://localhost:5002`.

### 4. Front

```powershell
cd frontend
copy .env.example .env
npm install
npm run dev
```

Abra `http://localhost:5173`. Para ver o front sem o back, troque `VITE_USE_MOCK=true` no `.env`.

---

## Testes

```powershell
# 40 testes das regras (models e strategies) — não precisam de banco
dotnet test Excursoes.Tests

# 35 testes de ATAQUE: sobem a API de verdade com um banco temporário e tentam invadir
$env:EXCURSOES_TESTE_MYSQL = "server=localhost;user=root;password=SUA_SENHA_DO_ROOT"
dotnet test Excursoes.IntegrationTests
```

Os testes de ataque criam um banco `excursoes_teste_xxxx`, rodam e apagam no final (por isso precisam de um usuário que crie bancos, como o root local).

Antes de publicar, rode também:

```powershell
dotnet list package --vulnerable --include-transitive   # dependências com falha conhecida
gitleaks detect                                         # senha esquecida no histórico do Git
```

---

## Publicação (ex.: Render)

Configure como **variáveis de ambiente** (nunca no `appsettings.json`):

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Padrao=server=...;database=excursoes;user=excursoes_app;password=...
Jwt__Chave=<texto aleatório com 32+ caracteres>
Admin__Email=...
Admin__Senha=...
Cors__OrigensPermitidas__0=https://endereco-do-seu-front.vercel.app
Pagamentos__PermitirPagamentoSimulado=true
```

Em produção a API liga HTTPS obrigatório (HSTS) e lê o IP real do visitante através do proxy da hospedagem. No front, `VITE_API_URL` aponta para o endereço público da API.

> As fotos ficam na pasta `wwwroot/uploads` do servidor. Em hospedagens com disco temporário (como o plano grátis do Render), elas somem a cada deploy: o próximo passo é trocar o `ImagemLocalStorage` por um storage externo (Cloudinary, S3), implementando a mesma interface `IImagemStorage`.

## Próximos passos

- **Gateway de pagamento real** (Mercado Pago): uma interface `IGatewayPagamento` gera a cobrança e um **webhook** confirma o pagamento; o pagamento simulado (`Pagamentos:PermitirPagamentoSimulado`) é desligado.
- Token em **cookie httpOnly** em vez de `localStorage` (veja limitações no SEGURANCA.md).
- Dockerfile para o deploy.
