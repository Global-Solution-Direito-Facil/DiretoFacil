# DireitoFácil API — Global Solution FIAP 2026/2

> API RESTful de documentos e pedidos de direitos sociais do cidadão (CitizenTech), desenvolvida em ASP.NET Core (.NET 10) com Oracle Database e publicada no Azure por CI/CD com Azure DevOps.

Disciplinas: **Advanced Business Development with .NET** e **DevOps Tools & Cloud Computing**.

# Link do vídeo 
| Recurso | URL |
|---|---|
| Youtube | https://youtu.be/obbbRXlLFy8

## Links da aplicação

| Recurso | URL |
|---|---|
| Swagger (API em produção) | [https://direitofacil-rm561378.azurewebsites.net/index.html](https://direitofacil-rm561378.azurewebsites.net/index.html) |
| Health check | [https://direitofacil-rm561378.azurewebsites.net/health](https://direitofacil-rm561378.azurewebsites.net/health) |
| Liveness | [https://direitofacil-rm561378.azurewebsites.net/health/live](https://direitofacil-rm561378.azurewebsites.net/health/live) |
| Banco de dados (Oracle FIAP, em nuvem) | `oracle.fiap.com.br:1521/orcl` |

---

## Membros

| RM | Nome completo | Turma | GitHub | LinkedIn |
|---|---|---|---|---|
| RM565594 | João Vitor Biribilli Ravelli | 2TDSPG | [biribillidev](https://github.com/biribillidev) | [LinkedIn](https://www.linkedin.com/in/jo%C3%A3o-vitor-biribilli-ravelli-b9b447367/) |
| RM564093 | Gabriel Neris Losano | 2TDSPG | [GNLosano](https://github.com/GNLosano) | [LinkedIn](https://www.linkedin.com/in/gabriellosano/) |
| RM565554 | Alexander Dennis Isidro Mamani | 2TDSPG | [alex-isidro](https://github.com/alex-isidro) | [LinkedIn](https://www.linkedin.com/in/alexander-isidro/) |
| RM563748 | Kelson Zhang | 2TDSPG | [KelsonZh0](https://github.com/KelsonZh0) | [LinkedIn](https://www.linkedin.com/in/kelson-zhang-211456323/) |
| RM561378 | Pietro Paranhos Wilhelm | 2TDSPG | [PietroWilhelm](https://github.com/PietroWilhelm) | [LinkedIn](https://www.linkedin.com/in/pietrowilhelm/) |

## Sumário

- [Links da aplicação](#links-da-aplicação)
- [Membros](#membros)
- [Descrição do Projeto](#descrição-do-projeto)
- [Benefícios para o Cidadão e para a Gestão Pública](#benefícios-para-o-cidadão-e-para-a-gestão-pública)
- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Arquitetura do projeto](#arquitetura-do-projeto)
- [Entidades principais](#entidades-principais)
- [Relacionamentos](#relacionamentos)
- [Requisitos da disciplina e onde estão](#requisitos-da-disciplina-e-onde-estão)
- [Configuração do banco Oracle](#configuração-do-banco-oracle)
- [Migrations e script do banco](#migrations-e-script-do-banco)
- [Observabilidade e Monitoramento](#observabilidade-e-monitoramento)
- [Autenticação (JWT) e papéis](#autenticação-jwt-e-papéis)
- [Idempotência, rate limiting e LGPD](#idempotência-rate-limiting-e-lgpd)
- [Integração com a API Java (RabbitMQ)](#integração-com-a-api-java-rabbitmq)
- [Testes automatizados](#testes-automatizados)
- [Como executar](#como-executar)
- [Deploy em Nuvem (CI/CD com Azure DevOps)](#deploy-em-nuvem-cicd-com-azure-devops)
- [Portas e serviços](#portas-e-serviços)
- [Variáveis de ambiente](#variáveis-de-ambiente)
- [Endpoints disponíveis](#endpoints-disponíveis)
- [Ordem recomendada para testar](#ordem-recomendada-para-testar)
- [JSONs para teste](#jsons-para-teste)
- [Testes de erro](#testes-de-erro)
- [CRUD direto no Oracle (comprovação no banco em nuvem)](#crud-direto-no-oracle-comprovação-no-banco-em-nuvem)
- [Códigos HTTP utilizados](#códigos-http-utilizados)
- [Observações importantes](#observações-importantes)
- [Evidências de funcionamento](#evidências-de-funcionamento)
- [Uso de IA generativa](#uso-de-ia-generativa)

---

## Descrição do Projeto

Famílias de pessoas com autismo, com deficiência ou idosas de baixa renda muitas vezes não sabem a quais direitos um laudo médico dá acesso, nem quais documentos precisam levar para pedi-los. O resultado são viagens perdidas a órgãos públicos, pedidos indeferidos por falta de papel e direitos que nunca chegam a quem tem direito.

O **DireitoFácil** lê o laudo, indica os direitos e acompanha cada pedido até a concessão. Esta API .NET cuida da parte de **documentos e envios**:

1. O cidadão envia o laudo e os demais documentos pelo app (upload seguro, com consentimento explícito para dado de saúde).
2. A API grava o arquivo em storage privado e publica o evento `DocumentoRecebido` (padrão outbox) para o RabbitMQ.
3. A API Java (Spring AI) consome o evento, extrai os dados do laudo e devolve o status do processamento (`PATCH /api/documentos/{id}/status`).
4. O cidadão monta um **envio** para um direito (ex.: `CIPTEA`, `BPC_PCD`) e o submete. A API confere se todos os documentos obrigatórios daquele direito estão presentes.
5. O assistente social (papel `GESTOR`) aprova o envio ou o devolve com uma observação do que corrigir.

A arquitetura segue os princípios de **Clean Architecture**, com separação entre domínio, aplicação, infraestrutura e apresentação. O banco de dados é o **Oracle** (Oracle FIAP, em nuvem), com o schema gerenciado por **Entity Framework Core Migrations**. A API é publicada no **Azure App Service** por uma esteira **CI/CD no Azure DevOps**.

ODS: 3 (Saúde e bem-estar), 10 (Redução das desigualdades) e 11 (Cidades e comunidades sustentáveis).

---

## Benefícios para o Cidadão e para a Gestão Pública

| Benefício | Descrição |
|---|---|
| **Checklist automático de documentos** | Cada direito tem a sua lista de documentos obrigatórios. A família descobre o que falta antes de ir ao órgão, e não depois de uma viagem perdida. |
| **Menos pedidos indeferidos** | O envio só é submetido quando está completo, o que reduz retrabalho para a família e para o assistente social. |
| **Fila de análise organizada** | O assistente social trabalha uma fila única (`status=Enviado`), aprova ou devolve com observação, e o histórico fica registrado. |
| **Privacidade (LGPD)** | Consentimento explícito para dado de saúde, exclusão de documentos pelo próprio cidadão e eventos sem nome, CPF ou conteúdo do arquivo. |
| **Funciona com internet instável** | A idempotência permite que o app reenvie a mesma requisição sem criar documentos ou envios duplicados. |
| **Escalável e desacoplada** | O processamento por IA roda em outra API, ligada por fila. Se a IA estiver fora do ar, os eventos ficam guardados e são publicados depois. |

---

## Tecnologias utilizadas

* C# / .NET 10
* ASP.NET Core Web API (Controllers)
* Entity Framework Core + Oracle.EntityFrameworkCore
* Oracle Database (Oracle FIAP)
* EF Core Migrations
* Swagger / OpenAPI (Swashbuckle)
* Autenticação JWT (Microsoft.AspNetCore.Authentication.JwtBearer)
* Rate Limiting nativo do ASP.NET Core
* Microsoft.Extensions.Diagnostics.HealthChecks
* RabbitMQ (RabbitMQ.Client) com padrão outbox — broker gerenciado no CloudAMQP (RabbitMQ 4.x)
* Azure Blob Storage (opcional) ou pasta local para os arquivos
* xUnit + SQLite em memória + WebApplicationFactory (testes)
* Rider / Visual Studio
* Azure CLI / Microsoft Azure (App Service)
* Azure DevOps (Azure Boards e Azure Pipelines — CI/CD)

---

## Arquitetura do projeto

### Diagrama da solução

![Arquitetura e fluxo CI/CD do DireitoFácil](docs/arquitetura-cicd.png)

O diagrama mostra todos os componentes da solução — personas (desenvolvedor, cidadão, assistente social), GitHub, Azure DevOps (Boards e pipelines CI/CD), Azure App Service, Oracle FIAP e a integração com a API Java via RabbitMQ — e o fluxo numerado da esteira, detalhado em [Deploy em Nuvem](#deploy-em-nuvem-cicd-com-azure-devops). O arquivo editável está em [`docs/arquitetura-cicd.drawio`](docs/arquitetura-cicd.drawio) (abrir no [draw.io](https://app.diagrams.net)).

### Fluxo da aplicação

```mermaid
flowchart LR
    App[App Mobile<br/>React Native] -->|JWT| Api[DireitoFacil.Api<br/>Controllers]
    Api --> App2[DireitoFacil.Application<br/>Services, DTOs, validações]
    App2 --> Dom[DireitoFacil.Domain<br/>Entidades e regras]
    App2 --> Inf[DireitoFacil.Infrastructure<br/>EF Core, Storage, Outbox]
    Inf --> Ora[(Oracle FIAP)]
    Inf --> Blob[(Storage privado<br/>Local ou Azure Blob)]
    Inf -->|Outbox Worker| Mq[[RabbitMQ<br/>documentos.recebidos]]
    Mq --> Java[API Java<br/>Spring AI]
    Java -->|PATCH status<br/>papel SERVICO| Api
```

### Camadas

A solução foi organizada em quatro projetos principais, mais os projetos de teste correspondentes:

```text
DireitoFacil
├── DireitoFacil.Api
├── DireitoFacil.Application
├── DireitoFacil.Domain
├── DireitoFacil.Infrastructure
├── DireitoFacil.Api.Tests
├── DireitoFacil.Application.Tests
├── DireitoFacil.Domain.Tests
├── DireitoFacil.Infrastructure.Tests
├── scripts/            (infra-direitofacil.sh, script-bd.sql, crud-demo.sql)
├── docs/               (diagrama da arquitetura e evidências de funcionamento)
└── azure-pipeline.yml
```

### DireitoFacil.Api

Camada de apresentação: expõe os endpoints REST.

* Controllers (`AuthController`, `DocumentoController`, `EnvioController`)
* `Program.cs` e extensões de configuração (DI, Swagger, JWT, rate limiting, health checks)
* `GlobalExceptionHandler` (erros em `application/problem+json`)
* Filtro de idempotência (`IdempotenciaFilter` + `[Idempotente]`)
* Health checks (Oracle, storage, RabbitMQ e API Java)

### DireitoFacil.Application

Casos de uso e contratos.

* Services (`AuthService`, `DocumentoService`, `EnvioService`)
* DTOs de request e response com DataAnnotations
* Interfaces de repositório, storage, eventos e token
* Validação de arquivo (tipo real pelos *magic bytes*, tamanho) e de paginação

### DireitoFacil.Domain

Regras de negócio, sem dependência de framework.

* Entidades e `BaseEntity`
* Enums
* Máquinas de estado do documento e do envio
* Exceções de domínio (`DomainException`, `RegraDeNegocioException`)

### DireitoFacil.Infrastructure

Persistência e integrações.

* `DireitoFacilContext` (EF Core + Oracle), configurations e migrations
* Repositórios e Unit of Work
* Hash de senha (PBKDF2)
* Storage local ou Azure Blob
* Outbox + `OutboxWorker` + publicação no RabbitMQ
* Seed da conta GESTOR (e, opcionalmente, SERVICO) e do catálogo de documentos obrigatórios

---

## Entidades principais

### Usuario

Pessoa que acessa a API: cidadão, assistente social ou o serviço da API Java.

* Id
* Nome
* Email (único)
* SenhaHash
* Papel (`Cidadao`, `Gestor` ou `Servico`)
* CriadoEm / AtualizadoEm

### Documento

Arquivo enviado pelo cidadão (laudo, receita, RG, comprovantes).

* Id
* CidadaoId
* Tipo (`Laudo`, `Receita`, `Rg`, `Cpf`, `ComprovanteRenda`, `ComprovanteResidencia`, `Outro`)
* Descricao
* NomeArquivo, ContentType, TamanhoArquivo
* BlobPath (caminho no storage privado)
* HashSha256 (impede o mesmo arquivo duas vezes para o mesmo cidadão)
* Status (`Recebido`, `Processando`, `Extraido`, `Falha`)
* ConsentimentoSaude / ConsentimentoSaudeEm
* CriadoEm / AtualizadoEm

### Envio

Pedido de um direito: o pacote de documentos analisado pelo assistente social.

* Id
* CidadaoId
* CasoId (caso aberto na API Java)
* DireitoCodigo (ex.: `CIPTEA`, `BPC_PCD`)
* Status (`Rascunho`, `Enviado`, `Aprovado`, `Devolvido`, `Cancelado`)
* Observacao
* GestorId
* EnviadoEm / AnalisadoEm
* CriadoEm / AtualizadoEm

### EnvioDocumento

Associação N:N entre envio e documento.

* EnvioId
* DocumentoId

### DocumentoObrigatorio

Catálogo de documentos exigidos por direito (o checklist), criado pelo seed.

* DireitoCodigo
* TipoDocumento

| Direito | Documentos obrigatórios |
|---|---|
| `CIPTEA` | Laudo, RG, CPF |
| `ACOMPANHANTE_ESCOLAR_TEA` | Laudo |
| `BPC_PCD` | Laudo, RG, CPF, comprovante de renda, comprovante de residência |
| `BPC_IDOSO` | RG, CPF, comprovante de renda, comprovante de residência |
| `CADUNICO` | RG, CPF, comprovante de renda, comprovante de residência |
| `TARIFA_SOCIAL` | CPF, comprovante de residência |
| `FARMACIA_POPULAR` | Receita, RG, CPF |
| `PASSE_LIVRE_PCD` | Laudo, RG, CPF, comprovante de renda |

### Tabelas de apoio

| Tabela | Conteúdo |
|---|---|
| `DF_IDEMPOTENCIA` | Chaves `Idempotency-Key` com o status e a resposta guardados |
| `DF_OUTBOX` | Eventos pendentes de publicação no RabbitMQ |

---

## Relacionamentos

```text
Usuario (cidadão) 1:N Documento
Usuario (cidadão) 1:N Envio
Envio N:N Documento   (via DF_ENVIO_DOCUMENTO)
DocumentoObrigatorio  (catálogo por DireitoCodigo, usado ao submeter o envio)
```

Ou seja:

* Um cidadão pode ter vários documentos e vários envios.
* Um envio reúne um ou mais documentos do mesmo cidadão.
* Um documento pode participar de mais de um envio (ex.: o mesmo RG em dois pedidos).
* Ao submeter, o envio é conferido contra o catálogo de documentos obrigatórios do direito.

Tabelas no Oracle (prefixo `DF_`): `DF_USUARIO`, `DF_DOCUMENTO`, `DF_ENVIO`, `DF_ENVIO_DOCUMENTO`, `DF_DOCUMENTO_OBRIGATORIO`, `DF_IDEMPOTENCIA` e `DF_OUTBOX`.

O banco desta API é independente do banco da API Java. A ligação entre as duas é feita só pelo evento `DocumentoRecebido` e pelo id do documento.

---

## Requisitos da disciplina e onde estão

| Requisito | Implementação |
|---|---|
| API RESTful com validações e códigos HTTP | Controllers em `DireitoFacil.Api/Controllers`; DataAnnotations nos DTOs; erros em `application/problem+json` pelo `GlobalExceptionHandler` (400, 401, 403, 404, 409, 422, 429, 503) |
| OpenAPI e Swagger | Swagger UI na raiz (`/`), com comentários XML, contratos de entrada e saída e respostas de cada endpoint |
| Requisições repetidas (idempotência) | `IdempotenciaFilter` + atributo `[Idempotente]` nos POST de criação |
| Limitação de requisições | 4 políticas em `RateLimitServiceCollectionExtensions`, com HTTP 429 e header `Retry-After` |
| JWT com controle de acesso | `AuthController` emite o token; policies `SomenteCidadao`, `SomenteGestor` e `SomenteServico` |
| Health checks | `/health` (Oracle, storage, RabbitMQ e API Java) e `/health/live` |
| Testes unitários e de integração com xUnit | 4 projetos de teste; os de integração sobem a API em memória com `WebApplicationFactory` e SQLite |

---

## Configuração do banco Oracle

A connection string **não fica versionada** no `appsettings.json`: senhas e chaves nunca vão para o GitHub. A API lê a chave `ConnectionStrings:DireitoFacilOracle`, que vem de um destes lugares conforme a forma de execução:

| Onde a API roda | De onde vem a connection string |
|---|---|
| Local, com `dotnet run` | **User Secrets** do .NET — arquivo fora do repositório, só na sua máquina |
| Nuvem (Azure) | App Setting `ConnectionStrings__DireitoFacilOracle`, gravado pelo deploy a partir da variável **secreta** `OracleConnection` da pipeline |

Formato da connection string (Oracle FIAP):

```text
Data Source=oracle.fiap.com.br:1521/orcl;User ID=SEU_RM;Password=SUA_SENHA;
```

---

## Migrations e script do banco

A API usa **EF Core Migrations**. Na inicialização ela aplica as migrations pendentes e roda o seed (conta GESTOR, conta SERVICO se configurada e catálogo de documentos obrigatórios). Para desligar, use `Database:AplicarMigrations=false`.

### Criar migration

```powershell
dotnet ef migrations add NomeDaMigration -p DireitoFacil.Infrastructure -s DireitoFacil.Api -o Persistence/Migrations
```

### Aplicar migration no Oracle

```powershell
dotnet ef database update -p DireitoFacil.Infrastructure -s DireitoFacil.Api
```

### Gerar o `scripts/script-bd.sql` (DDL usado pela pipeline)

```powershell
dotnet ef migrations script --idempotent -p DireitoFacil.Infrastructure -s DireitoFacil.Api -o scripts/script-bd.sql
```

O script é **idempotente**: verifica a tabela `__EFMigrationsHistory` e só cria o que ainda não existe. É ele que o `scripts/infra-direitofacil.sh` executa no Oracle durante o CI.

### Remover a última migration, se necessário

```powershell
dotnet ef migrations remove -p DireitoFacil.Infrastructure -s DireitoFacil.Api
```

---

## Observabilidade e Monitoramento

### Health Check

```http
GET /health
GET /health/live
```

| Rota | O que verifica | Status HTTP |
|---|---|---|
| `/health` | processo, Oracle, storage, RabbitMQ e API Java | 200 se `Healthy` ou `Degraded`; 503 se `Unhealthy` |
| `/health/live` | só o processo (usado para saber se o app precisa ser reiniciado) | 200 |

RabbitMQ e API Java fora do ar deixam a API `Degraded`, e não `Unhealthy`: os eventos ficam guardados no outbox e são publicados quando o serviço voltar.

Exemplo de resposta:

```json
{
  "status": "Healthy",
  "totalDurationMs": 412.7,
  "checks": [
    { "name": "self", "status": "Healthy", "description": "O processo da API está no ar.", "durationMs": 0.1, "error": null },
    { "name": "oracle", "status": "Healthy", "description": "…", "durationMs": 380.2, "error": null },
    { "name": "storage", "status": "Healthy", "description": "…", "durationMs": 2.4, "error": null },
    { "name": "rabbitmq", "status": "Healthy", "description": "RabbitMQ acessível.", "durationMs": 0.3, "error": null },
    { "name": "api-java", "status": "Healthy", "description": "API Java não configurada para monitoramento.", "durationMs": 0.2, "error": null }
  ]
}
```

> Exemplo resumido (descrições do Oracle e do storage abreviadas). Checks: `self` (único usado pelo `/health/live`), `oracle`, `storage`, `rabbitmq` e `api-java`. O detalhe da exceção (`error`) só é exibido em Development, para não vazar informação em produção.

### Logs

Logging padrão do ASP.NET Core. Na nuvem, os logs aparecem no **Log stream** do App Service (habilitado pelo `infra-direitofacil.sh`).

### Tratamento de exceções

O `GlobalExceptionHandler` converte qualquer exceção em `application/problem+json`:

| Exceção | Status |
|---|---|
| Validação de DTO (DataAnnotations) / `DomainException` | 400 |
| Credenciais inválidas | 401 |
| Acesso negado | 403 |
| Recurso não encontrado | 404 |
| `RegraDeNegocioException` / registro duplicado | 409 |
| Banco indisponível | 503 |
| Erro inesperado | 500 |

---

## Autenticação (JWT) e papéis

### Fluxo

1. O cidadão se cadastra em `POST /api/auth/registrar` (sempre com o papel `CIDADAO`).
2. Faz login em `POST /api/auth/login` e recebe o `accessToken`.
3. Envia `Authorization: Bearer {accessToken}` em todas as demais chamadas.

A conta **GESTOR** (assistente social) não se cadastra pela API: é criada pelo seed a partir de `Seed:Gestor`. A API Java não precisa de conta: ela gera o próprio token de papel **SERVICO** (validade de 5 minutos), assinado com a mesma chave JWT. A conta `Seed:Servico` é opcional, para testar esse papel pelo Swagger.

### Papéis

| Papel | Quem é | O que pode fazer |
|---|---|---|
| `CIDADAO` | Família | Enviar, corrigir e excluir os próprios documentos; criar, submeter e cancelar envios |
| `GESTOR` | Assistente social | Ver a fila de envios e aprovar ou devolver |
| `SERVICO` | API Java | Atualizar o status de processamento do documento |

> No token (claim `role`) o papel aparece em maiúsculas (`CIDADAO`, `GESTOR`, `SERVICO`); no JSON de resposta e no banco (`DF_USUARIO.Papel`) aparece como o nome do enum (`Cidadao`, `Gestor`, `Servico`).

### Configuração

| Chave | Valor |
|---|---|
| `Jwt:Issuer` | `direitofacil` |
| `Jwt:Audience` | `direitofacil-app` |
| `Jwt:Chave` | Segredo (mín. 32 caracteres), o mesmo nas APIs .NET e Java |
| `Jwt:ExpiracaoMinutos` | `480` |

O token é HS256 com `sub` = e-mail do usuário e a claim `role` com o papel.

---

## Idempotência, rate limiting e LGPD

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

> No Swagger, se o campo `Idempotency-Key` ficar vazio, a própria tela gera um UUID novo. Para testar a repetição, digite a mesma chave duas vezes. Fora do Swagger o header continua obrigatório.

### Rate limiting

| Política | Onde | Limite |
|---|---|---|
| `autenticacao` | login e cadastro | 10 por minuto por IP (contra força bruta) |
| `upload` | envio de documento | 5 seguidos, recarregando 1 a cada 2 minutos por usuário (cada upload aciona a IA) |
| `escrita` | demais POST, PUT e DELETE | 30 por minuto por usuário |
| `leitura` | GET | 120 por minuto por usuário |

Ao estourar o limite, a API responde **429** com o header `Retry-After`.

### Segurança e LGPD

- Senhas com hash PBKDF2 (`PasswordHasher` do ASP.NET Core).
- Chave do JWT, connection string e senhas das contas de seed ficam em user-secrets (local) ou em variáveis secretas da pipeline (Azure). Nada disso fica no código nem no `appsettings.json`.
- O upload de laudo exige consentimento explícito para dado de saúde (LGPD, art. 11), e a data do consentimento fica registrada.
- O tipo real do arquivo é conferido pelos bytes iniciais (*magic bytes*): um executável renomeado para `.pdf` é recusado. Aceita PDF, JPEG e PNG de até 10 MB.
- O mesmo arquivo não é aceito duas vezes para o mesmo cidadão (hash SHA-256).
- O cidadão só vê os próprios documentos. Um documento de outra pessoa responde 404, e não 403, para não revelar que o id existe.
- O cidadão pode excluir documento e arquivo (direito de exclusão, LGPD art. 18), exceto se o documento estiver em um envio ativo.
- O evento `DocumentoRecebido` não carrega nome, CPF nem o conteúdo do arquivo: só identificadores e uma URL temporária de leitura.

---

## Integração com a API Java (RabbitMQ)

| Item | Valor |
|---|---|
| Fila | `documentos.recebidos` (exchange padrão) |
| Evento | `DocumentoRecebido`, serializado em camelCase |
| Publicação | Padrão **outbox**: o evento é gravado na `DF_OUTBOX` na mesma transação do documento, e o `OutboxWorker` publica a cada 5 segundos |
| Retorno da IA | A API Java chama `PATCH /api/documentos/{id}/status` com um token de papel `SERVICO` |

Campos do evento:

```json
{
  "eventId": "1b9d6bcd-bbfd-4b2d-9b5d-ab8dfbbd4bed",
  "documentoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cidadaoId": "maria@email.com",
  "tipoDocumento": "Laudo",
  "contentType": "application/pdf",
  "blobPath": "maria@email.com/3fa85f64.pdf",
  "criadoEm": "2026-11-15T22:00:00Z",
  "arquivoUrl": "https://…/arquivo-temporario"
}
```

Na nuvem, o broker é um RabbitMQ gerenciado no **CloudAMQP** (plano gratuito Little Lemur), acessível pelas duas APIs pela internet via `amqps://`. A URL fica na variável secreta `RabbitMqUri` da pipeline.

O `arquivoUrl` é uma URL temporária de leitura (validade de 60 minutos, `RabbitMq:ValidadeUrlMinutos`) gerada pelo `OutboxWorker` no momento da publicação, para não expirar enquanto o evento espera no outbox. Com o storage local, a API Java baixa o arquivo pelo `GET /api/documentos/{id}/arquivo` com o token SERVICO.

Sem `RabbitMq:Uri` configurado, a API continua funcionando normalmente: os eventos ficam acumulados no outbox e o `/health` fica `Degraded`.

---

## Testes automatizados

A solução tem testes em **xUnit**, organizados por camada, espelhando a Clean Architecture. Nenhum teste precisa de Oracle nem de RabbitMQ: a persistência usa **SQLite em memória**.

| Projeto de teste | Camada testada | O que cobre |
|---|---|---|
| `DireitoFacil.Domain.Tests` | Domínio | Regras das entidades `Usuario`, `Documento`, `Envio` e `DocumentoObrigatorio`: validações, consentimento de saúde e as máquinas de estado do documento e do envio |
| `DireitoFacil.Application.Tests` | Application | `AuthService`, `DocumentoService` e `EnvioService` (cadastro, login, upload, checklist de obrigatórios, análise do gestor) e a validação de arquivo |
| `DireitoFacil.Infrastructure.Tests` | Infraestrutura | `DireitoFacilContext`, mapeamentos, repositórios, hash de senha, storage local e publicação no outbox, com SQLite em memória |
| `DireitoFacil.Api.Tests` | API (integração) | Sobe a API real em memória com `WebApplicationFactory`: autenticação, documentos, envios, segurança por papel, idempotência, rate limiting e health checks |

### Executando os testes

Rodar toda a suíte (168 testes):

```powershell
dotnet test
```

Rodar um projeto específico:

```powershell
dotnet test DireitoFacil.Domain.Tests
dotnet test DireitoFacil.Application.Tests
dotnet test DireitoFacil.Infrastructure.Tests
dotnet test DireitoFacil.Api.Tests
```

Na pipeline de CI os testes rodam automaticamente, e os resultados e a cobertura de código são publicados no Azure DevOps (abas **Tests** e **Code Coverage**).

---

## Como executar

### Localmente com .NET (requer Oracle externo)

**Pré-requisitos:** .NET SDK 10 e acesso a um banco Oracle (o da FIAP ou um Oracle Free local). RabbitMQ é opcional.

**1. Clone o repositório**

```bash
git clone https://github.com/Global-Solution-Direito-Facil/DiretoFacil.git
cd DiretoFacil
```

**2. Configure os segredos locais (User Secrets)**

```powershell
cd DireitoFacil.Api
dotnet user-secrets set "ConnectionStrings:DireitoFacilOracle" "Data Source=oracle.fiap.com.br:1521/orcl;User ID=SEU_RM;Password=SUA_SENHA;"
dotnet user-secrets set "Jwt:Chave" "UMA-CHAVE-ALEATORIA-COM-PELO-MENOS-32-CARACTERES"
dotnet user-secrets set "Seed:Gestor:Email" "gestor@direitofacil.com"
dotnet user-secrets set "Seed:Gestor:Senha" "SENHA_DO_GESTOR"
cd ..
```

Opcionais:

```powershell
dotnet user-secrets set "Seed:Servico:Email" "servico@direitofacil.com" --project DireitoFacil.Api
dotnet user-secrets set "Seed:Servico:Senha" "SENHA_DO_SERVICO" --project DireitoFacil.Api
dotnet user-secrets set "RabbitMq:Uri" "amqp://usuario:senha@host:5672/" --project DireitoFacil.Api
dotnet user-secrets set "ServicosExternos:JavaApiHealthUrl" "https://API_JAVA/actuator/health" --project DireitoFacil.Api
```

Para conferir: `dotnet user-secrets list --project DireitoFacil.Api`.

**3. Execute a API**

```powershell
dotnet run --project DireitoFacil.Api
```

> As migrations e o seed rodam automaticamente na inicialização. Não é necessário rodar `dotnet ef database update`.

**4. Acesse**

```
http://localhost:5264          (Swagger)
http://localhost:5264/health
```

---

## Deploy em Nuvem (CI/CD com Azure DevOps)

A API é publicada no **Azure App Service (Web App)** por uma pipeline YAML do **Azure DevOps** (`azure-pipeline.yml`, na raiz). Cada merge na branch `main` dispara a pipeline, que cria a infraestrutura (se ainda não existir), compila, roda os testes automatizados e faz o deploy, sem nenhum passo manual.

As URLs de produção estão em [Links da aplicação](#links-da-aplicação).

### Fluxo CI/CD (numeração do diagrama)

Os números abaixo seguem o [diagrama da arquitetura](#diagrama-da-solução).

| # | Etapa | Onde atua |
|---|---|---|
| 1 | A tarefa é criada no Azure Boards; o desenvolvedor cria uma branch, faz o commit com `AB#<id>` e abre o PR para a `main` | Azure Boards / GitHub |
| 2 | O merge na `main` dispara automaticamente a pipeline (`trigger: main`) — Stage CI | Azure Pipelines |
| 3 | Job **Criar Infra Inicial**: `scripts/infra-direitofacil.sh` cria as tabelas do `scripts/script-bd.sql` no Oracle (só se não existirem) e cria/reaproveita Resource Group, Plano e Web App | Azure CLI → Oracle FIAP / Azure |
| 4 | Job **Build and Test**: restore, build (Release) e testes xUnit, com resultados e cobertura publicados | Azure Pipelines (Tests) |
| 5 | Publish da `DireitoFacil.Api` e publicação do artefato `app` (.zip) | Azure DevOps (Artifacts) |
| 6 | O novo artefato dispara o Stage CD (deployment no environment `producao`) | Azure Pipelines |
| 7 | Deploy do .zip no Web App (`AzureWebApp@1`) e gravação das App Settings a partir das variáveis secretas | Azure App Service |
| 8 | A API em produção persiste no Oracle FIAP (EF Core) e publica o evento `DocumentoRecebido` (outbox) para a API Java via RabbitMQ | Oracle FIAP / RabbitMQ |
| 9 | Cidadão (app mobile), assistente social e Swagger consomem a API via HTTPS com JWT | Web App |

> O **Oracle FIAP** é o banco de dados em nuvem da solução. A API, rodando no Azure, e as ferramentas de banco (Rider / SQL Developer) se conectam ao mesmo servidor `oracle.fiap.com.br` pela internet.

O stage **CD** só roda quando o **CI** termina com sucesso: se um teste falhar, nada é publicado.

### Fluxo de trabalho (Boards + Repos)

1. Criar a tarefa no **Azure Boards**.
2. Criar uma branch a partir da `main` (ex.: `feature/AB123-ajuste-validacao`).
3. Alterar o **código-fonte** e fazer o commit citando a tarefa: `git commit -m "Ajusta validação do upload AB#123"`.
4. Abrir o Pull Request para a `main` com `AB#123` na descrição. A branch `main` é protegida: exige PR, revisor obrigatório (revisor padrão: RM561378) e work item vinculado.
5. Aprovar e fazer o merge. A pipeline roda sozinha e publica a nova versão.
6. A tarefa no Boards mostra os links de branch, commits e PR.

### Recursos no Azure

| Recurso | Nome |
|---|---|
| Grupo de recursos | `rg-direitofacil` (South Africa North) |
| Plano do App Service | `plan-direitofacil` (Windows, SKU F1) |
| Web App | `direitofacil-rm561378` (runtime .NET 10) |
| Banco de dados | Oracle FIAP, usuário `rm561378`, 7 tabelas `DF_*` |

Os recursos são criados pelo `scripts/infra-direitofacil.sh` (Azure CLI), que é idempotente: em cada execução reaproveita o que já existe e nunca apaga nem altera dados do banco.

### Arquivos de DevOps

| Arquivo | Conteúdo |
|---|---|
| `azure-pipeline.yml` | Pipeline YAML: Stage CI (infra, build, testes, artefato) e Stage CD (deploy) |
| `scripts/infra-direitofacil.sh` | Script Azure CLI: tabelas no Oracle (SQLcl) + Resource Group, Plano e Web App |
| `scripts/script-bd.sql` | DDL dos objetos do banco (gerado por `dotnet ef migrations script --idempotent`) |
| `scripts/crud-demo.sql` | CRUD de todas as tabelas, executado direto no Oracle |
| `docs/arquitetura-cicd.drawio` / `.png` | Desenho macro da arquitetura |
| `docs/evidencias/` | Prints do Oracle em nuvem e da fila no RabbitMQ |

### Configuração no Azure DevOps

1. **Service connection** do tipo *Azure Resource Manager* chamada `DireitoFacilAzure` (usada pelas tarefas Azure CLI e Azure Web App).
2. **Environment** `producao` (criado no primeiro deploy), usado pelo stage CD.
3. **Variáveis secretas** (botão *Variables* da pipeline, com cadeado), nunca declaradas no YAML:

| Variável | Uso |
|---|---|
| `ORACLE_PASS` | Senha do Oracle, usada pelo `infra-direitofacil.sh` para criar as tabelas |
| `OracleConnection` | Connection string do Oracle → App Setting `ConnectionStrings__DireitoFacilOracle` |
| `JwtChave` | Chave de assinatura do JWT (a mesma da API Java) → App Setting `Jwt__Chave` |
| `SeedGestorEmail` | E-mail da conta GESTOR criada no seed → `Seed__Gestor__Email` |
| `SeedGestorSenha` | Senha da conta GESTOR → `Seed__Gestor__Senha` |
| `RabbitMqUri` | URL AMQPS do RabbitMQ gerenciado (CloudAMQP), o mesmo broker da API Java → `RabbitMq__Uri` |

As variáveis não secretas (host, porta, SID e usuário do Oracle, nome do Web App, região e pasta de arquivos) ficam no bloco `variables` do `azure-pipeline.yml`.

> No App Service Windows o `wwwroot` fica somente leitura quando o pacote roda direto do .zip. Por isso os arquivos enviados são gravados em `D:\home\data\arquivos` (`Storage__PastaLocal`), pasta gravável e persistente.

### Excluindo a infraestrutura ao final

```bash
az group delete --name "rg-direitofacil" --yes --no-wait
```

---

## Portas e serviços

| Serviço | Onde | URL |
|---|---|---|
| DireitoFácil API (local) | `dotnet run` | `http://localhost:5264/api/...` |
| Swagger UI (local) | `dotnet run` | `http://localhost:5264/index.html` |
| DireitoFácil API (nuvem) | Azure App Service | `https://direitofacil-rm561378.azurewebsites.net/api/...` |
| Swagger UI (nuvem) | Azure App Service | `https://direitofacil-rm561378.azurewebsites.net/index.html` |
| Oracle FIAP | Nuvem | `oracle.fiap.com.br:1521/orcl` |
| RabbitMQ (CloudAMQP) | Nuvem | `amqps://…rmq6.cloudamqp.com` (porta 5671, TLS) |

---

## Variáveis de ambiente

Localmente as chaves vêm do User Secrets (com `:`); no Azure, das App Settings (com `__`).

| Variável | Valor padrão | Descrição |
|---|---|---|
| `ConnectionStrings__DireitoFacilOracle` | _(obrigatória)_ | Connection string do Oracle |
| `Jwt__Chave` | _(obrigatória)_ | Chave de assinatura do JWT (mín. 32 caracteres), igual à da API Java |
| `Jwt__Issuer` / `Jwt__Audience` | `direitofacil` / `direitofacil-app` | Emissor e público-alvo do token |
| `Jwt__ExpiracaoMinutos` | `480` | Validade do token |
| `ASPNETCORE_ENVIRONMENT` | `Development` (local) / `Production` (nuvem) | Ambiente de execução |
| `Database__AplicarMigrations` | `true` | Aplica migrations e seed na inicialização |
| `Seed__Gestor__Email` / `Seed__Gestor__Senha` | _(vazio)_ | Conta GESTOR criada no seed |
| `Seed__Servico__Email` / `Seed__Servico__Senha` | _(vazio)_ | Conta SERVICO opcional (testes pelo Swagger; a API Java gera o próprio token) |
| `Storage__Provider` | `Local` | `Local` ou `AzureBlob` |
| `Storage__PastaLocal` | `arquivos` | Pasta dos arquivos (nuvem: `D:\home\data\arquivos`) |
| `Storage__AzureConnectionString` / `Storage__Container` | _(vazio)_ / `documentos` | Usados quando o provider é `AzureBlob` |
| `RabbitMq__Uri` | _(vazio)_ | URI do RabbitMQ; vazio = eventos ficam no outbox |
| `RabbitMq__FilaDocumentosRecebidos` | `documentos.recebidos` | Fila do evento |
| `ServicosExternos__JavaApiHealthUrl` | _(vazio)_ | Health da API Java, verificado no `/health` |

---

## Endpoints disponíveis

Todos usam o prefixo `/api`. Com exceção de `auth`, todos exigem `Authorization: Bearer {token}`.

### Autenticação

| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `/api/auth/registrar` | público | Cadastra um cidadão (senha com 8+ caracteres, letras e números) |
| POST | `/api/auth/login` | público | Devolve o JWT (`accessToken`) |

### Documentos

| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `/api/documentos` | CIDADAO | Upload (`multipart/form-data`: `Arquivo`, `Tipo`, `ConsentimentoSaude`, `Descricao`). Idempotente |
| GET | `/api/documentos` | todos | Lista paginada (`page`, `pageSize`, `tipo`, `status`; `cidadaoId` só para gestor) |
| GET | `/api/documentos/{id}` | todos | Detalhe |
| GET | `/api/documentos/{id}/arquivo` | todos | Download do arquivo |
| PUT | `/api/documentos/{id}` | CIDADAO | Corrige tipo e descrição (bloqueado enquanto a IA processa) |
| PATCH | `/api/documentos/{id}/status` | SERVICO | A API Java informa `Processando`, `Extraido` ou `Falha` |
| DELETE | `/api/documentos/{id}` | CIDADAO | Exclui documento e arquivo |

### Envios

| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `/api/envios` | CIDADAO | Cria um envio em rascunho (`casoId`, `direitoCodigo`, `documentoIds`). Idempotente |
| GET | `/api/envios` | todos | Lista paginada; o gestor usa `status=Enviado` como fila de análise |
| GET | `/api/envios/{id}` | todos | Detalhe |
| PUT | `/api/envios/{id}` | CIDADAO | Troca os documentos (só em `Rascunho` ou `Devolvido`) |
| POST | `/api/envios/{id}/submeter` | CIDADAO | Envia para análise e confere os documentos obrigatórios. Idempotente |
| POST | `/api/envios/{id}/analise` | GESTOR | `Aprovar` ou `Devolver` (devolver exige observação) |
| DELETE | `/api/envios/{id}` | CIDADAO | Cancela (só em `Rascunho` ou `Devolvido`) |

Ciclo do envio:

```
Rascunho ──submeter──▶ Enviado ──aprovar──▶ Aprovado
    ▲                     │
    └──── corrigir ◀── Devolvido (observação obrigatória)
Rascunho / Devolvido ──cancelar──▶ Cancelado
```

Ciclo do documento: `Recebido → Processando → Extraido` ou `Falha`. Um documento com `Falha` impede a submissão do envio até ser reenviado.

### Health

`GET /health` e `GET /health/live` — detalhes em [Observabilidade e Monitoramento](#observabilidade-e-monitoramento).

---

## Ordem recomendada para testar

```text
1. Cadastrar cidadão        POST /api/auth/registrar
2. Login do cidadão          POST /api/auth/login  → copiar o accessToken
3. Authorize no Swagger      colar o accessToken
4. Enviar um laudo           POST /api/documentos  → copiar o id
5. Listar / consultar        GET  /api/documentos, GET /api/documentos/{id}
6. Corrigir a descrição      PUT  /api/documentos/{id}
7. Criar envio               POST /api/envios      → copiar o id
8. Submeter o envio          POST /api/envios/{id}/submeter
9. Login do gestor           POST /api/auth/login (conta do seed) → Authorize
10. Fila de análise          GET  /api/envios?status=Enviado
11. Aprovar ou devolver      POST /api/envios/{id}/analise
12. Cidadão cancela/exclui   DELETE /api/envios/{id} (rascunho/devolvido), DELETE /api/documentos/{id}
```

> Importante: ao testar pelo Swagger, sempre copie os ids retornados pela própria API. No Oracle os ids `Guid` ficam como `RAW(16)`, em formato hexadecimal diferente do usado no JSON.

---

## JSONs para teste

### 1. Cadastrar cidadão

```http
POST /api/auth/registrar
```

```json
{
  "nome": "Maria Souza",
  "email": "maria.souza@email.com",
  "senha": "Senha12345"
}
```

Resultado esperado: **201 Created**

```json
{
  "id": "6f1c2a9e-0b7d-4c1e-9a55-1f2e3d4c5b6a",
  "nome": "Maria Souza",
  "email": "maria.souza@email.com",
  "papel": "Cidadao"
}
```

---

### 2. Login

```http
POST /api/auth/login
```

```json
{
  "email": "maria.souza@email.com",
  "senha": "Senha12345"
}
```

Resultado esperado: **200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiraEm": "2026-11-16T06:00:00Z",
  "usuario": {
    "id": "6f1c2a9e-0b7d-4c1e-9a55-1f2e3d4c5b6a",
    "nome": "Maria Souza",
    "email": "maria.souza@email.com",
    "papel": "Cidadao"
  },
  "tokenType": "Bearer"
}
```

Copie o `accessToken` e informe-o no botão **Authorize** do Swagger.

---

### 3. Enviar documento (upload)

```http
POST /api/documentos
Idempotency-Key: 5d0f3c1a-8e2b-4f6a-9c7d-1a2b3c4d5e6f
Content-Type: multipart/form-data
```

| Campo | Valor |
|---|---|
| `Arquivo` | um PDF, JPEG ou PNG de até 10 MB |
| `Tipo` | `Laudo` |
| `ConsentimentoSaude` | `true` (obrigatório para `Laudo`) |
| `Descricao` | `Laudo neurológico` |

Resultado esperado: **201 Created**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cidadaoId": "maria.souza@email.com",
  "tipo": "Laudo",
  "descricao": "Laudo neurológico",
  "nomeArquivo": "laudo.pdf",
  "contentType": "application/pdf",
  "tamanhoArquivo": 102400,
  "status": "Recebido",
  "consentimentoSaude": true,
  "consentimentoSaudeEm": "2026-11-15T22:00:00Z",
  "criadoEm": "2026-11-15T22:00:00Z",
  "atualizadoEm": null
}
```

Com `curl`:

```bash
curl -X POST "https://direitofacil-rm561378.azurewebsites.net/api/documentos" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Idempotency-Key: 5d0f3c1a-8e2b-4f6a-9c7d-1a2b3c4d5e6f" \
  -F "Arquivo=@laudo.pdf;type=application/pdf" \
  -F "Tipo=Laudo" -F "ConsentimentoSaude=true" -F "Descricao=Laudo neurológico"
```

---

### 4. Listar documentos (paginação e filtro)

```http
GET /api/documentos?page=1&pageSize=10&tipo=Laudo&status=Recebido
```

Resultado esperado: **200 OK**

```json
{
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1,
  "items": [ { "id": "3fa85f64-...", "tipo": "Laudo", "status": "Recebido" } ],
  "hasPrevious": false,
  "hasNext": false
}
```

---

### 5. Corrigir documento

```http
PUT /api/documentos/{id}
```

```json
{
  "tipo": "Laudo",
  "descricao": "Laudo neurológico - CID F84.0",
  "consentimentoSaude": true
}
```

Resultado esperado: **200 OK**

---

### 6. Criar envio

```http
POST /api/envios
Idempotency-Key: 9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d
```

```json
{
  "casoId": 1,
  "direitoCodigo": "ACOMPANHANTE_ESCOLAR_TEA",
  "documentoIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ]
}
```

Resultado esperado: **201 Created**, com `"status": "Rascunho"`.

---

### 7. Trocar os documentos do envio

```http
PUT /api/envios/{id}
```

```json
{
  "documentoIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ]
}
```

Resultado esperado: **200 OK** (só em `Rascunho` ou `Devolvido`).

---

### 8. Submeter o envio

```http
POST /api/envios/{id}/submeter
Idempotency-Key: 0c1d2e3f-4a5b-4c6d-8e7f-9a0b1c2d3e4f
```

Sem body. Resultado esperado: **200 OK**, com `"status": "Enviado"`. Se faltar algum documento obrigatório do direito, a resposta é **409** com a lista do que falta.

---

### 9. Análise do gestor

Faça login com a conta do seed (`Seed:Gestor`) e use o token dela.

```http
GET /api/envios?status=Enviado
POST /api/envios/{id}/analise
```

Aprovar:

```json
{
  "decisao": "Aprovar",
  "observacao": "Documentação completa"
}
```

Devolver (observação obrigatória):

```json
{
  "decisao": "Devolver",
  "observacao": "Envie o laudo com o CID legível"
}
```

Resultado esperado: **200 OK**, com `"status": "Aprovado"` ou `"Devolvido"`.

---

### 10. Atualizar status do documento (API Java, papel SERVICO)

```http
PATCH /api/documentos/{id}/status
```

```json
{
  "status": "Processando"
}
```

Resultado esperado: **204 No Content**. Valores aceitos: `Processando`, `Extraido`, `Falha`.

---

### 11. Cancelar envio e excluir documento

```http
DELETE /api/envios/{id}
DELETE /api/documentos/{id}
```

Resultado esperado: **204 No Content**. Documento que está em um envio ativo não pode ser excluído (**409**).

---

## Testes de erro

| Cenário | Requisição | Resultado esperado |
|---|---|---|
| Endpoint protegido sem token | `GET /api/documentos` sem `Authorization` | **401** |
| Login com senha errada | `POST /api/auth/login` com senha incorreta | **401** |
| Cadastro com e-mail repetido | `POST /api/auth/registrar` com e-mail já usado | **409** |
| Cadastro com senha fraca | `"senha": "abc"` | **400** |
| Cidadão tentando analisar envio | `POST /api/envios/{id}/analise` com token de cidadão | **403** |
| Documento de outro cidadão / id inexistente | `GET /api/documentos/00000000-0000-0000-0000-000000000000` | **404** |
| Upload sem `Idempotency-Key` (fora do Swagger) | `POST /api/documentos` sem o header | **400** |
| Mesma `Idempotency-Key` com outro arquivo | repetir o upload trocando o arquivo | **422** |
| Laudo sem consentimento | `Tipo=Laudo`, `ConsentimentoSaude=false` | **400** |
| Arquivo `.exe` renomeado para `.pdf` | upload do arquivo | **400** |
| Submeter envio incompleto | `direitoCodigo: "CIPTEA"` só com o laudo | **409** |
| Devolver sem observação | `{"decisao": "Devolver"}` | **400** |
| Mais de 10 logins em 1 minuto | repetir `POST /api/auth/login` | **429** com `Retry-After` |

---

## CRUD direto no Oracle (comprovação no banco em nuvem)

A persistência é comprovada direto no banco, com `SELECT` (e não com `GET` na API). O script [`scripts/crud-demo.sql`](scripts/crud-demo.sql) faz **INSERT → SELECT → UPDATE → SELECT** em cada uma das 7 tabelas e, no final, o **DELETE** de todos os registros de teste, com um `SELECT` que confirma a exclusão.

Para conferir a conexão e as tabelas criadas pela pipeline:

```sql
SELECT SYS_CONTEXT('USERENV','SERVER_HOST') AS servidor,
       SYS_CONTEXT('USERENV','IP_ADDRESS')  AS ip_cliente
FROM dual;

SELECT table_name FROM user_tables WHERE table_name LIKE 'DF\_%' ESCAPE '\' ORDER BY table_name;
```

Para conferir o que foi criado pela API:

```sql
SELECT RAWTOHEX("Id") AS ID, "Nome", "Email", "Papel", "CriadoEm" FROM "DF_USUARIO";
SELECT RAWTOHEX("Id") AS ID, "CidadaoId", "Tipo", "Status", "CriadoEm" FROM "DF_DOCUMENTO";
SELECT RAWTOHEX("Id") AS ID, "CidadaoId", "DireitoCodigo", "Status", "GestorId" FROM "DF_ENVIO";
SELECT RAWTOHEX("EnvioId") AS ENVIO, RAWTOHEX("DocumentoId") AS DOCUMENTO FROM "DF_ENVIO_DOCUMENTO";
SELECT * FROM "DF_DOCUMENTO_OBRIGATORIO" ORDER BY "DireitoCodigo";
SELECT RAWTOHEX("Chave") AS CHAVE, "UsuarioId", "Endpoint", "StatusCode", "ExpiraEm" FROM "DF_IDEMPOTENCIA";
SELECT RAWTOHEX("Id") AS ID, "Tipo", "Tentativas", "ProcessadoEm", "UltimoErro" FROM "DF_OUTBOX";
```

---

## Códigos HTTP utilizados

| Código | Significado | Uso na API |
|---|---|---|
| 200 | OK | Consulta, atualização, submissão, análise ou login |
| 201 | Created | Usuário, documento ou envio criado |
| 204 | No Content | Exclusão, cancelamento ou atualização de status |
| 400 | Bad Request | Dados inválidos, header `Idempotency-Key` ausente ou arquivo não permitido |
| 401 | Unauthorized | Token ausente, inválido ou expirado; login com credenciais incorretas |
| 403 | Forbidden | Papel sem permissão para o endpoint |
| 404 | Not Found | Registro inexistente ou de outro cidadão |
| 409 | Conflict | Regra de negócio no estado atual, registro duplicado ou mesma chave ainda em processamento |
| 422 | Unprocessable Entity | Mesma `Idempotency-Key` com conteúdo diferente |
| 429 | Too Many Requests | Limite de requisições atingido (`Retry-After`) |
| 500 | Internal Server Error | Erro inesperado |
| 503 | Service Unavailable | Banco de dados indisponível; `/health` com status `Unhealthy` |

---

## Observações importantes

### GUID e Oracle RAW(16)

Os ids das entidades são `Guid` no C# e ficam no Oracle como `RAW(16)`. No banco, use `RAWTOHEX("Id")` para exibir e `HEXTORAW('...')` para filtrar. Para testar endpoints que dependem de id, use sempre o `id` retornado pela própria API.

### Nomes entre aspas no Oracle

As tabelas e colunas foram criadas pelo EF Core entre aspas, então os nomes diferenciam maiúsculas e minúsculas: use `"DF_USUARIO"."Nome"`, e não `df_usuario.nome`.

### Enums

Os enums são trafegados e gravados como **texto** (`"Laudo"`, `"Enviado"`, `"Cidadao"`), e não como número. Isso deixa o JSON e o banco legíveis e evita valores inválidos.

### Commit no Oracle

Em ferramentas com transação manual (Rider, SQL Developer), rode `COMMIT` depois de cada `INSERT`, `UPDATE` e `DELETE` para que a API e as outras sessões vejam a alteração.

---

## Evidências de funcionamento

### API em produção

A API publicada pela pipeline responde em [`/health`](https://direitofacil-rm561378.azurewebsites.net/health) com todos os checks `Healthy` (Oracle, storage e RabbitMQ) e o Swagger fica em [`/index.html`](https://direitofacil-rm561378.azurewebsites.net/index.html).

### Banco de dados em nuvem (Oracle FIAP)

Conexão ao servidor remoto `oracle.fiap.com.br` (servidor `oraclenew`, banco `ORCL`, usuário `RM561378`) — o IP do cliente foi ocultado:

![Conexão ao Oracle FIAP](docs/evidencias/oracle-conexao-nuvem.png)

As 7 tabelas `DF_*` criadas pelo `scripts/script-bd.sql` durante o CI:

![Tabelas DF_ no Oracle](docs/evidencias/oracle-tabelas-df.png)

### Mensageria (RabbitMQ no CloudAMQP)

Fila `documentos.recebidos` (durável) no RabbitMQ 4.3.6 gerenciado pelo CloudAMQP, com os eventos `DocumentoRecebido` publicados pelo `OutboxWorker` da API em produção, aguardando o consumo pela API Java:

![Fila documentos.recebidos no RabbitMQ](docs/evidencias/rabbitmq-fila-documentos-recebidos.png)

Para conferir no banco que os eventos foram publicados (`ProcessadoEm` preenchido e `UltimoErro` vazio):

```sql
SELECT RAWTOHEX("Id") AS ID, "Tipo", "Tentativas", "ProcessadoEm", "UltimoErro"
FROM "DF_OUTBOX" ORDER BY "CriadoEm" DESC;
```

---

## Uso de IA generativa

Ferramenta: **Claude (Anthropic)**, usada como apoio para planejar a estrutura da solução, revisar o código, sugerir testes, montar a pipeline de CI/CD e redigir este README. Todo o código foi revisado, compilado e testado pelos integrantes.

---

## Conclusão

O DireitoFácil transforma um laudo médico em um caminho claro até o direito: a família sabe quais documentos levar, envia tudo pelo app com segurança e acompanha o pedido até a decisão do assistente social. A API .NET garante as regras de negócio, a segurança e a privacidade dos dados, e a esteira CI/CD no Azure DevOps leva cada alteração do código até a nuvem de forma automática, testada e rastreável.
