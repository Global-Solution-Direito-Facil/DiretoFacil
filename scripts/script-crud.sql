-- =====================================================================
-- DireitoFácil - Demonstração de CRUD no Oracle (nuvem: oracle.fiap.com.br)
-- =====================================================================

-- =====================================================================
-- 1) DF_USUARIO
-- =====================================================================
-- CREATE
INSERT INTO "DF_USUARIO" ("Id","Nome","Email","SenhaHash","Papel","CriadoEm")
VALUES (HEXTORAW('A0000000000000000000000000000001'), 'Maria Demo',
        'maria.demo@direitofacil.com', 'HASH_DEMO', 'CIDADAO', SYSTIMESTAMP);
COMMIT;

-- READ
SELECT RAWTOHEX("Id") AS ID, "Nome", "Email", "Papel", "CriadoEm", "AtualizadoEm"
FROM "DF_USUARIO" WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');

-- UPDATE
UPDATE "DF_USUARIO" SET "Nome" = 'Maria Demo Silva', "AtualizadoEm" = SYSTIMESTAMP
WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Nome", "AtualizadoEm"
FROM "DF_USUARIO" WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');

-- =====================================================================
-- 2) DF_DOCUMENTO
-- =====================================================================
-- CREATE
INSERT INTO "DF_DOCUMENTO" ("Id","CidadaoId","Tipo","Descricao","NomeArquivo","ContentType",
                            "TamanhoArquivo","BlobPath","HashSha256","Status","ConsentimentoSaude",
                            "ConsentimentoSaudeEm","CriadoEm")
VALUES (HEXTORAW('B0000000000000000000000000000001'), 'maria.demo@direitofacil.com',
        'Laudo', 'Laudo médico', 'laudo.pdf', 'application/pdf', 102400,
        'demo/laudo.pdf', RPAD('a', 64, 'a'), 'Recebido', 1, SYSTIMESTAMP, SYSTIMESTAMP);
COMMIT;

-- READ
SELECT RAWTOHEX("Id") AS ID, "CidadaoId", "Tipo", "Descricao", "Status", "CriadoEm"
FROM "DF_DOCUMENTO" WHERE "Id" = HEXTORAW('B0000000000000000000000000000001');

-- UPDATE (simula a IA concluindo a extração)
UPDATE "DF_DOCUMENTO" SET "Status" = 'Extraido', "Descricao" = 'Laudo TEA - CID F84.0',
                          "AtualizadoEm" = SYSTIMESTAMP
WHERE "Id" = HEXTORAW('B0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Status", "Descricao", "AtualizadoEm"
FROM "DF_DOCUMENTO" WHERE "Id" = HEXTORAW('B0000000000000000000000000000001');

-- =====================================================================
-- 3) DF_ENVIO
-- =====================================================================
-- CREATE
INSERT INTO "DF_ENVIO" ("Id","CidadaoId","CasoId","DireitoCodigo","Status","CriadoEm")
VALUES (HEXTORAW('C0000000000000000000000000000001'), 'maria.demo@direitofacil.com',
        1, 'ACOMPANHANTE_ESCOLAR_TEA', 'Rascunho', SYSTIMESTAMP);
COMMIT;

-- READ
SELECT RAWTOHEX("Id") AS ID, "CidadaoId", "DireitoCodigo", "Status", "CriadoEm"
FROM "DF_ENVIO" WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');

-- UPDATE (gestor aprova)
UPDATE "DF_ENVIO" SET "Status" = 'Aprovado', "EnviadoEm" = SYSTIMESTAMP,
                      "AnalisadoEm" = SYSTIMESTAMP, "GestorId" = 'gestor@direitofacil.com',
                      "Observacao" = 'Documentação completa', "AtualizadoEm" = SYSTIMESTAMP
WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Status", "GestorId", "Observacao", "AnalisadoEm"
FROM "DF_ENVIO" WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');

-- =====================================================================
-- 4) DF_ENVIO_DOCUMENTO (tabela de ligação Envio x Documento)
-- =====================================================================
-- CREATE
INSERT INTO "DF_ENVIO_DOCUMENTO" ("EnvioId","DocumentoId")
VALUES (HEXTORAW('C0000000000000000000000000000001'), HEXTORAW('B0000000000000000000000000000001'));
COMMIT;

-- READ (JOIN mostrando o relacionamento)
SELECT RAWTOHEX(e."Id") AS ENVIO, e."DireitoCodigo", e."Status",
       RAWTOHEX(d."Id") AS DOCUMENTO, d."Tipo", d."NomeArquivo"
FROM "DF_ENVIO" e
         JOIN "DF_ENVIO_DOCUMENTO" ed ON ed."EnvioId" = e."Id"
         JOIN "DF_DOCUMENTO" d        ON d."Id" = ed."DocumentoId"
WHERE e."Id" = HEXTORAW('C0000000000000000000000000000001');

-- UPDATE (tabela só de chaves: troca o documento vinculado)
INSERT INTO "DF_DOCUMENTO" ("Id","CidadaoId","Tipo","NomeArquivo","ContentType","TamanhoArquivo",
                            "BlobPath","HashSha256","Status","ConsentimentoSaude","CriadoEm")
VALUES (HEXTORAW('B0000000000000000000000000000002'), 'maria.demo@direitofacil.com',
        'Rg', 'rg.png', 'image/png', 50000, 'demo/rg.png', RPAD('b', 64, 'b'),
        'Recebido', 0, SYSTIMESTAMP);

UPDATE "DF_ENVIO_DOCUMENTO" SET "DocumentoId" = HEXTORAW('B0000000000000000000000000000002')
WHERE "EnvioId" = HEXTORAW('C0000000000000000000000000000001')
  AND "DocumentoId" = HEXTORAW('B0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX(ed."EnvioId") AS ENVIO, RAWTOHEX(ed."DocumentoId") AS DOCUMENTO, d."Tipo"
FROM "DF_ENVIO_DOCUMENTO" ed JOIN "DF_DOCUMENTO" d ON d."Id" = ed."DocumentoId"
WHERE ed."EnvioId" = HEXTORAW('C0000000000000000000000000000001');

-- =====================================================================
-- 5) DF_DOCUMENTO_OBRIGATORIO (regra: documentos exigidos por direito)
-- =====================================================================
-- CREATE
INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo","TipoDocumento")
VALUES ('DEMO_DIREITO', 'Laudo');
COMMIT;

-- READ
SELECT * FROM "DF_DOCUMENTO_OBRIGATORIO" WHERE "DireitoCodigo" = 'DEMO_DIREITO';

-- UPDATE
UPDATE "DF_DOCUMENTO_OBRIGATORIO" SET "TipoDocumento" = 'Receita'
WHERE "DireitoCodigo" = 'DEMO_DIREITO' AND "TipoDocumento" = 'Laudo';
COMMIT;

SELECT * FROM "DF_DOCUMENTO_OBRIGATORIO" WHERE "DireitoCodigo" = 'DEMO_DIREITO';

-- =====================================================================
-- 6) DF_IDEMPOTENCIA (chaves Idempotency-Key)
-- =====================================================================
-- CREATE
INSERT INTO "DF_IDEMPOTENCIA" ("Chave","UsuarioId","Endpoint","Fingerprint","StatusCode",
                               "RespostaJson","CriadoEm","ExpiraEm")
VALUES (HEXTORAW('D0000000000000000000000000000001'), 'maria.demo@direitofacil.com',
        'POST /api/documentos', RPAD('c', 64, 'c'), NULL, NULL,
        SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '1' DAY);
COMMIT;

-- READ
SELECT RAWTOHEX("Chave") AS CHAVE, "UsuarioId", "Endpoint", "StatusCode", "ExpiraEm"
FROM "DF_IDEMPOTENCIA" WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');

-- UPDATE (requisição concluída: guarda status e resposta)
UPDATE "DF_IDEMPOTENCIA" SET "StatusCode" = 201, "RespostaJson" = '{"id":"demo","status":"Recebido"}'
WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Chave") AS CHAVE, "StatusCode", "RespostaJson"
FROM "DF_IDEMPOTENCIA" WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');

-- =====================================================================
-- 7) DF_OUTBOX (eventos para o RabbitMQ)
--    Criado já com ProcessadoEm preenchido para o OutboxWorker da API ignorar
-- =====================================================================
-- CREATE
INSERT INTO "DF_OUTBOX" ("Id","Tipo","Payload","CriadoEm","ProcessadoEm","Tentativas")
VALUES (HEXTORAW('E0000000000000000000000000000001'), 'DocumentoRecebido',
        '{"documentoId":"demo"}', SYSTIMESTAMP, SYSTIMESTAMP, 0);
COMMIT;

-- READ
SELECT RAWTOHEX("Id") AS ID, "Tipo", "Payload", "Tentativas", "ProcessadoEm"
FROM "DF_OUTBOX" WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');

-- UPDATE
UPDATE "DF_OUTBOX" SET "Tentativas" = 1, "UltimoErro" = 'Teste de demonstração'
WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Tentativas", "UltimoErro"
FROM "DF_OUTBOX" WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');
