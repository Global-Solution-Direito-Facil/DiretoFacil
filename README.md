# DireitoFácil — API .NET (documentos e envios)

Global Solution 2026/2 · CitizenTech · 2TDSPG
Disciplina: **Advanced Business Development with .NET**

API REST em ASP.NET Core (.NET 10) que recebe os documentos da família (laudos, receitas, comprovantes) e organiza os **envios**: o pacote de documentos de um pedido de direito, analisado por um assistente social. É a API consumida pelo app mobile junto com a API Java.

## Integrantes
# 👥 Integrantes da Equipe

                                  






| RM | Nome completo | Turma |  GitHub | LinkedIn         
|---|---|---| ---| ---|
| RM565594 | João Vitor Biribilli Ravelli | 2TDSPG | [biribillidev](https://github.com/biribillidev)   | —                                                           |
| RM564093 | Gabriel Neris Losano | 2TDSPG | [GNLosano](https://github.com/GNLosano)              | [LinkedIn](https://www.linkedin.com/in/gabriellosano/)           |
| RM565554 | Alexander Dennis Isidro Mamani | 2TDSPG | [alex-isidro](https://github.com/alex-isidro)     | [LinkedIn](https://www.linkedin.com/in/alexander-isidro/) |
| RM563748 | Kelson Zhang | 2TDSPG | [KelsonZh0](https://github.com/KelsonZh0)                    | [LinkedIn](https://www.linkedin.com/in/kelson-zhang-211456323/)  |
| RM561378 | Pietro Paranhos Wilhelm | 2TDSPG | [PietroWilhelm](https://github.com/PietroWilhelm) |  [LinkedIn](https://www.linkedin.com/in/pietrowilhelm/)          |

---

## 1. Problema e solução

Famílias de pessoas com autismo, deficiência ou idosas de baixa renda não sabem a quais direitos um laudo médico dá acesso, nem quais documentos levar. O DireitoFácil lê o laudo, indica os direitos e acompanha cada pedido até a concessão.

Esta API cuida da parte de **documentos**:

1. O cidadão envia o laudo e os demais documentos pelo app (upload seguro, com consentimento para dado de saúde).
2. A API grava o arquivo em storage privado e publica o evento `DocumentoRecebido` no RabbitMQ.
3. A API Java (Spring AI) consome o evento, extrai os dados do laudo e devolve o status do processamento (`PATCH /api/documentos/{id}/status`).
4. O cidadão monta um **envio** para um direito (ex.: `CIPTEA`) e o submete. A API confere se todos os documentos obrigatórios daquele direito estão presentes.
5. O assistente social (papel `GESTOR`) aprova o envio ou o devolve com uma observação do que corrigir.

**Funcionalidade que agrega valor ao cidadão:** o checklist automático de documentos obrigatórios por direito. A família descobre o que falta antes de ir ao órgão, e não depois de uma viagem perdida.

ODS: 3 (Saúde e bem-estar), 10 (Redução das desigualdades) e 11 (Cidades e comunidades sustentáveis).

## 2. Arquitetura

```mermaid
flowchart LR
    App[App Mobile<br/>React Native] -->|JWT| Api[DireitoFacil.Api<br/>Controllers]
    Api --> App2[DireitoFacil.Application<br/>Services, DTOs, validações]
    App2 --> Dom[DireitoFacil.Domain<br/>Entidades e regras]
    App2 --> Inf[DireitoFacil.Infrastructure<br/>EF Core, Storage, Outbox]
    Inf --> Ora[(Oracle)]
    Inf --> Blob[(Storage privado<br/>Local ou Azure Blob)]
    Inf -->|Outbox Worker| Mq[[RabbitMQ<br/>documentos.recebidos]]
    Mq --> Java[API Java<br/>Spring AI]
    Java -->|PATCH status<br/>papel SERVICO| Api
```

Camadas no padrão do projeto de referência da disciplina:

| Projeto | Responsabilidade |
|---|---|
| `DireitoFacil.Api` | Controllers, JWT, rate limiting, idempotência, health checks, Swagger e tratamento global de erros |
| `DireitoFacil.Application` | Services, DTOs com DataAnnotations, interfaces de repositório e validação de arquivo |
| `DireitoFacil.Domain` | Entidades (`Usuario`, `Documento`, `Envio`, `EnvioDocumento`, `DocumentoObrigatorio`), enums e máquinas de estado |
| `DireitoFacil.Infrastructure` | `DbContext` (Oracle), migrations, repositórios, hash de senha, storage, outbox e publicação no RabbitMQ |
| `*.Tests` | Testes xUnit: Domain, Application, Infrastructure e Api (integração) |

### Tabelas (Oracle, criadas pela migration `Inicial`)

| Tabela | Conteúdo |
|---|---|
| `DF_USUARIO` | Login: nome, e-mail, hash da senha e papel (`CIDADAO`, `GESTOR` ou `SERVICO`) |
| `DF_DOCUMENTO` | Metadados do arquivo: tipo, status, hash SHA-256, caminho no storage e data do consentimento |
| `DF_ENVIO` | Pedido de um direito: caso, código do direito, status, observação e gestor |
| `DF_ENVIO_DOCUMENTO` | Associação N:N entre envio e documento |
| `DF_DOCUMENTO_OBRIGATORIO` | Documentos exigidos por direito (o checklist) |
| `DF_IDEMPOTENCIA` | Chaves `Idempotency-Key` e respostas guardadas |
| `DF_OUTBOX` | Eventos pendentes de publicação no RabbitMQ |

O banco desta API é independente do banco da API Java. A ligação entre as duas é feita só pelo evento `DocumentoRecebido` e pelo id do documento.

## 3. Requisitos da disciplina e onde estão

| Requisito | Implementação |
|---|---|
| API RESTful com validações e códigos HTTP | Controllers em `DireitoFacil.Api/Controllers`; DataAnnotations nos DTOs; erros em `application/problem+json` pelo `GlobalExceptionHandler` (400, 401, 403, 404, 409, 422, 429, 503) |
| OpenAPI e Swagger | Swagger UI na raiz (`/`), com comentários XML, contratos de entrada e saída e respostas de cada endpoint |
| Requisições repetidas (idempotência) | `IdempotenciaFilter` + atributo `[Idempotente]` nos POST de criação |
| Limitação de requisições | 4 políticas em `RateLimitServiceCollectionExtensions`, com HTTP 429 e header `Retry-After` |
| JWT com controle de acesso | `AuthController` emite o token; policies `SomenteCidadao`, `SomenteGestor` e `SomenteServico` |
| Health checks | `/health` (Oracle, storage, RabbitMQ e API Java) e `/health/live` |
| Testes unitários e de integração com xUnit | 4 projetos de teste; os de integração sobem a API em memória com `WebApplicationFactory` e SQLite |

### Idempotência

Os endpoints `POST /api/documentos`, `POST /api/envios` e `POST /api/envios/{id}/submeter` exigem o header `Idempotency-Key` (um UUID gerado pelo app para cada ação). Com conexão instável, o app reenvia a mesma requisição sem criar documentos ou envios duplicados.

| Situação | Resposta |
|---|---|
| Header ausente ou inválido | 400 |
| Chave nova | Executa e guarda status e corpo da resposta |
| Mesma chave e mesmo conteúdo | Devolve a resposta guardada, sem executar de novo (header `Idempotent-Replayed: true`) |
| Mesma chave com conteúdo diferente | 422 |
| Mesma chave ainda em processamento | 409 |

A chave vale por usuário e endpoint e expira em 24 horas. Se a action falhar com erro 5xx, a chave é liberada para nova tentativa. No upload, o arquivo entra na comparação pelo hash SHA-256 do conteúdo.

### Rate limiting

| Política | Onde | Limite |
|---|---|---|
| `autenticacao` | login e cadastro | 10 por minuto por IP (contra força bruta) |
| `upload` | envio de documento | 5 seguidos, recarregando 1 a cada 2 minutos por usuário (cada upload aciona a IA) |
| `escrita` | demais POST, PUT e DELETE | 30 por minuto por usuário |
| `leitura` | GET | 120 por minuto por usuário |

### Segurança e LGPD

- Senhas com hash PBKDF2 (`PasswordHasher` do ASP.NET Core).
- Chave do JWT, connection string e senhas das contas de seed ficam em user-secrets (local) ou em variáveis de ambiente (Azure). Nada disso fica no código nem no `appsettings.json`.
- O upload de laudo exige consentimento explícito para dado de saúde (LGPD, art. 11), e a data do consentimento fica registrada.
- O tipo real do arquivo é conferido pelos bytes iniciais (magic bytes): um executável renomeado para `.pdf` é recusado. Aceita PDF, JPEG e PNG de até 10 MB.
- O mesmo arquivo não é aceito duas vezes para o mesmo cidadão (hash SHA-256).
- O cidadão só vê os próprios documentos. Um documento de outra pessoa responde 404, e não 403, para não revelar que o id existe.
- O cidadão pode excluir documento e arquivo (direito de exclusão, LGPD art. 18), exceto se o documento estiver em um envio ativo.
- O evento `DocumentoRecebido` não carrega nome, CPF nem o conteúdo do arquivo: só identificadores e uma URL temporária de leitura.

## 4. Endpoints

Todos usam o prefixo `/api`. Com exceção de `auth`, todos exigem `Authorization: Bearer {token}`.

### Autenticação

| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `/auth/registrar` | público | Cadastra um cidadão (senha com 8+ caracteres, letras e números) |
| POST | `/auth/login` | público | Devolve o JWT (`accessToken`) |

### Documentos

| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `/documentos` | CIDADAO | Upload (`multipart/form-data`: `Arquivo`, `Tipo`, `ConsentimentoSaude`, `Descricao`). Idempotente |
| GET | `/documentos` | todos | Lista paginada (`page`, `pageSize`, `tipo`, `status`; `cidadaoId` só para gestor) |
| GET | `/documentos/{id}` | todos | Detalhe |
| GET | `/documentos/{id}/arquivo` | todos | Download do arquivo |
| PUT | `/documentos/{id}` | CIDADAO | Corrige tipo e descrição (bloqueado enquanto a IA processa) |
| PATCH | `/documentos/{id}/status` | SERVICO | A API Java informa `Processando`, `Extraido` ou `Falha` |
| DELETE | `/documentos/{id}` | CIDADAO | Exclui documento e arquivo |

Tipos de documento: `Laudo`, `Receita`, `Rg`, `Cpf`, `ComprovanteRenda`, `ComprovanteResidencia`, `Outro`.

### Envios

| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `/envios` | CIDADAO | Cria um envio em rascunho (`casoId`, `direitoCodigo`, `documentoIds`). Idempotente |
| GET | `/envios` | todos | Lista paginada; o gestor usa `status=Enviado` como fila de análise |
| GET | `/envios/{id}` | todos | Detalhe |
| PUT | `/envios/{id}` | CIDADAO | Troca os documentos (só em `Rascunho` ou `Devolvido`) |
| POST | `/envios/{id}/submeter` | CIDADAO | Envia para análise e confere os documentos obrigatórios. Idempotente |
| POST | `/envios/{id}/analise` | GESTOR | `Aprovar` ou `Devolver` (devolver exige observação) |
| DELETE | `/envios/{id}` | CIDADAO | Cancela (só em `Rascunho` ou `Devolvido`) |

Ciclo do envio:

```
Rascunho ──submeter──▶ Enviado ──aprovar──▶ Aprovado
    ▲                     │
    └──── corrigir ◀── Devolvido (observação obrigatória)
Rascunho / Devolvido ──cancelar──▶ Cancelado
```

Ciclo do documento: `Recebido → Processando → Extraido` ou `Falha`. Um documento com `Falha` impede a submissão do envio até ser reenviado.

### Health checks

| Rota | O que verifica | Status HTTP |
|---|---|---|
| `/health` | processo, Oracle, storage, RabbitMQ e API Java | 200 se `Healthy` ou `Degraded`; 503 se `Unhealthy` |
| `/health/live` | só o processo | 200 |

RabbitMQ e API Java fora do ar deixam a API `Degraded`, e não `Unhealthy`: os eventos ficam guardados no outbox e são publicados quando o serviço voltar.

## 5. Como executar

### Pré-requisitos

- .NET SDK 10
- Acesso a um banco Oracle (o da FIAP ou um Oracle Free local)
- Opcional: RabbitMQ (sem ele, os eventos ficam acumulados no outbox)

### Configuração (user-secrets)

Na pasta `DireitoFacil.Api`:

```bash
cd DireitoFacil.Api
dotnet user-secrets set "ConnectionStrings:DireitoFacilOracle" "Data Source=<host>:1521/<servico>;User ID=<usuario>;Password=<senha>;"
dotnet user-secrets set "Jwt:Chave" "<chave-aleatoria-com-pelo-menos-32-caracteres>"
dotnet user-secrets set "Seed:Gestor:Email" "gestor@direitofacil.com"
dotnet user-secrets set "Seed:Gestor:Senha" "<senha-do-gestor>"
```

Opcionais:

```bash
dotnet user-secrets set "Seed:Servico:Email" "servico@direitofacil.com"   # conta usada pela API Java
dotnet user-secrets set "Seed:Servico:Senha" "<senha-do-servico>"
dotnet user-secrets set "RabbitMq:Uri" "amqp://usuario:senha@host:5672/"
dotnet user-secrets set "ServicosExternos:JavaApiHealthUrl" "https://<api-java>/actuator/health"
```

No Azure, use as mesmas chaves como variáveis de ambiente, trocando `:` por `__` (ex.: `ConnectionStrings__DireitoFacilOracle`, `Jwt__Chave`). Para guardar os arquivos no Azure Blob, defina `Storage__Provider=AzureBlob` e `Storage__AzureConnectionString`.

### Rodar

```bash
dotnet run --project DireitoFacil.Api
```

Na primeira execução a API aplica as migrations no Oracle e cria as contas de seed (gestor e serviço). Para desligar isso, use `Database:AplicarMigrations=false`.

Acesse o Swagger em `http://localhost:5264`.

### Testar

```bash
dotnet test
```

Os testes não precisam de Oracle nem de RabbitMQ: os de integração usam SQLite em memória.

## 6. Uso de IA generativa

Ferramenta: **Claude (Anthropic)**, usada como apoio para planejar a estrutura da solução, revisar o código, sugerir testes e redigir este README. Todo o código foi revisado, compilado e testado pelos integrantes.
