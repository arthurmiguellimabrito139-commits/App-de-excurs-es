# Segurança

Checklist de 21 itens antes de publicar um site. Para cada item: **o que foi feito**, **onde está no código** e **qual teste prova**.
Os testes de ataque (`Excursoes.IntegrationTests/Ataques/`) sobem a API de verdade, com banco MySQL próprio, e tentam invadir.

| # | Item | O que foi feito | Onde | Teste que prova |
|---|---|---|---|---|
| 1 | Chave de API protegida | Chave do JWT e senha do banco ficam em `user-secrets` (dev) ou variáveis de ambiente (produção). A API **não inicia** se a chave do JWT faltar ou tiver menos de 32 caracteres. | `Services/TokenService.cs` (`JwtOpcoes.Ler`) | — (falha ao iniciar) |
| 2 | `.env` nunca exposto | `.env` do front no `.gitignore`. No front só entra o que é público (`VITE_API_URL`): tudo com `VITE_` vai para o navegador. | `.gitignore`, `frontend/.env.example` | — |
| 3 | Nada de senha no código | `appsettings.json` não tem nenhum segredo; o admin inicial também vem da configuração. | `appsettings.json`, `Configuracao/Inicializacao.cs` | — |
| 4 | Login de verdade | Senha com **BCrypt** (custo 12, limite de 72 bytes). **JWT** de 60 min validando assinatura, emissor, audiência, validade e algoritmo (só HS256: `alg: none` é recusado). Mesma mensagem e mesmo tempo de resposta para "e-mail não existe" e "senha errada". | `Services/AuthService.cs`, `Configuracao/Seguranca.cs` | `LoginEPermissaoTests`: token alterado, assinado com outra chave, vencido, `alg: none`, mensagem igual |
| 5 | Permissão no servidor | `[Authorize(Roles = "...")]` em toda rota que muda dados. A rota protegida do front só esconde a tela. | `Controllers/` | `Passageiro_NaoCriaViagem`, `Admin_NaoCompraPassagem` |
| 6 | Não confia no ID da tela | O id do usuário **sempre** vem do token assinado. O valor da compra **sempre** vem do banco; campos extras no JSON são ignorados (DTOs só têm o que é permitido). | `Controllers/ApiControllerBase.cs`, `DTOs/` | `Compra_IgnoraValorEIdMandadosPelaTela` |
| 7 | Cada um só vê o seu | "Minhas compras" filtra pelo token; parcela de outra pessoa e viagem de outro admin respondem **404** (não confirma que existem). | `Models/Admin.cs` (`GarantirQueEhDono`), `Services/CompraService.cs` | `CadaUmSoVeOSeuTests` (4 ataques) |
| 8 | Banco travado | A API usa um usuário MySQL com **só SELECT/INSERT/UPDATE/DELETE** no banco dela — não cria nem apaga tabelas. Chaves estrangeiras impedem apagar viagem com compra. | `README.md`, `Data/schema.sql` | `ViagemComVenda_NaoPodeSerExcluida` |
| 9 | Storage | Só a pasta `uploads` é pública, só com tipos de imagem, nomes aleatórios, sem listagem e sem sair da pasta (`../`). Arquivos removidos somem do disco. | `Configuracao/Inicializacao.cs` (`OpcoesDasImagens`) | `PastaDeUploads_NaoDeixaSairDela`, `ImagemRemovidaNaEdicao_SomeDoDisco` |
| 10 | Admin protegido | Cadastro público **sempre** cria Passageiro (o DTO nem tem campo de tipo). O admin vem da configuração, com senha de 12+ caracteres. | `DTOs/Auth/AuthDtos.cs`, `Services/AuthService.cs` | `Cadastro_PedindoParaSerAdmin_CriaPassageiroMesmoAssim` |
| 11 | Debug desligado | Sem Swagger, sem página de exceção, sem cabeçalho `Server`. Em produção: HTTPS obrigatório (HSTS). | `Configuracao/` | `NaoExistemTelasDeDebugOuDocumentacaoAbertas`, `Respostas_TemCabecalhosDeSeguranca...` |
| 12 | Erro sem detalhe | Toda exceção vira `{ "erro": "mensagem amigável" }`; o detalhe técnico vai só para o log do servidor. Erros de JSON malformado não mostram a estrutura interna. | `Middlewares/ErroMiddleware.cs`, `Seguranca.CriarRespostaDeValidacao` | `JsonQuebrado_DevolveErroSemDetalheTecnico`, `RotaInexistente_NaoMostraDetalhes` |
| 13 | Valida tudo no servidor | Duas camadas: atributos nos DTOs (tamanho, faixa, obrigatório) **e** regras nos models (datas, valor, vagas ≥ vendidas, parcelas). O objeto nunca fica inválido (`private set`). | `DTOs/`, `Models/` | `NumeroDeParcelasAbsurdo_Recusa`, `ViagemComVoltaAntesDaIda_Recusa`, 40 testes unitários |
| 14 | Limpa o que o usuário manda | `Trim()` e limite de tamanho em todo texto; e-mail normalizado; corpo das requisições limitado a 1 MB (uploads: 60 MB). O JSON de saída escapa HTML e o React escapa na tela. | `Models/`, `Seguranca.ConfigurarKestrel` | `CorpoGiganteForaDoUpload_EhRecusado`, `Email_EhGuardadoEmMinusculas...` |
| 15 | Upload protegido | Confere o **conteúdo** pelos primeiros bytes (JPG/PNG/WEBP), não a extensão; máx. 5 MB por foto e 10 por viagem; nome aleatório; `nosniff`. Se uma foto falhar, as anteriores são apagadas. | `Services/ImagemLocalStorage.cs` | `UploadTests` (6 ataques) e `ImagemTests` |
| 16 | Sem injeção de SQL | 100% das consultas com parâmetros (`@Id`) via Dapper; nenhum SQL montado com texto do usuário. | `Repositories/` | `Login_ComInjecaoDeSql_NaoEntra` |
| 17 | Limite de tentativas | Login e cadastro: 5 por minuto por IP. Geral: 300 por minuto por IP. Bloqueado = 429, até para a senha certa. | `Configuracao/Seguranca.cs` (`PoliticaDeLimites`) | `ForcaBrutaNoLogin_EhBloqueada` |
| 18 | Git sem senha vazada | `.gitignore` cobre `.env`, `bin/`, `obj/` e as fotos enviadas; segredos só em `user-secrets` / variáveis de ambiente. Rodar `gitleaks detect` antes de publicar. | `.gitignore` | — |
| 19 | Headers e CORS certos | CORS liberado **só** para o endereço do front (nunca `*`); `nosniff`, `X-Frame-Options: DENY`, `Content-Security-Policy`, `Referrer-Policy`, `Permissions-Policy`; HSTS em produção. | `Middlewares/CabecalhosSegurancaMiddleware.cs`, `Seguranca.AdicionarCors` | `Cors_SiteDesconhecido_NaoRecebePermissao`, `Cors_Front_RecebePermissao` |
| 20 | Testa como um estranho | 35 testes de ataque automatizados contra a API real. | `Excursoes.IntegrationTests/Ataques/` | todos |
| 21 | Auditoria completa com a IA | Revisão item a item deste documento, incluindo as limitações abaixo. | este arquivo | — |

## Além do checklist

- **Última vaga disputada:** a compra só grava se a viagem não mudou desde que foi lida (`UPDATE ... WHERE Versao = @Antiga`, dentro de uma transação). Num teste com 4 compradores simultâneos pela última vaga, repetido 15 vezes, **nenhuma viagem vendeu acima das vagas**. Teste: `UltimaVaga_DuasComprasAoMesmoTempo_SoUmaPassa`.
- **Valor sempre calculado no servidor:** o front só exibe a simulação; o total, os juros e as parcelas vêm do Strategy no C#.
- **Parcelas na ordem e sem pagar duas vezes:** regra dentro de `Compra` e `Parcela`.

## Limitações conhecidas (sendo honesto)

- **Token no `localStorage`:** se o front tivesse uma falha de XSS, o token poderia ser lido. O React escapa todo texto e o front não usa `dangerouslySetInnerHTML`, mas a proteção mais forte seria guardar o token em **cookie httpOnly** (com proteção contra CSRF).
- **Sem logout no servidor:** o token vale até expirar (60 min). Para revogar antes, seria preciso uma lista de tokens bloqueados ou tokens curtos com refresh token.
- **"E-mail já cadastrado" no cadastro** revela que o e-mail existe. É um compromisso comum de usabilidade; o limite de tentativas dificulta usar isso em massa.
- **Limite de tentativas em memória:** vale por instância da API. Com várias instâncias, seria preciso um contador compartilhado (ex.: Redis).
- **Pagamento simulado:** o botão "Pagar" é uma demonstração (liga/desliga em `Pagamentos:PermitirPagamentoSimulado`). Com um gateway real, quem confirma o pagamento é o webhook do gateway, validando a assinatura do aviso e consultando o pagamento na API do gateway.
- **Leitura do IP atrás de proxy** (`ForwardedHeaders`) só é ligada em produção, onde a hospedagem coloca um proxy na frente. Sem proxy, ela deixaria o visitante falsificar o próprio IP.
