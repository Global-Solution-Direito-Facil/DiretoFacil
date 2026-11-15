-- =====================================================================
-- crud-demo.sql - DireitoFácil (Global Solution 2026/2)
-- Demonstração de CRUD em todas as tabelas, direto no Oracle em nuvem
-- (oracle.fiap.com.br:1521/orcl). Execute um comando por vez e mostre
-- o resultado de cada SELECT. Os registros de teste são apagados no final.
-- =====================================================================

-- Conexão e tabelas criadas pela pipeline
SELECT SYS_CONTEXT('USERENV','SERVER_HOST') AS servidor,
       SYS_CONTEXT('USERENV','DB_NAME')     AS banco,
       SYS_CONTEXT('USERENV','IP_ADDRESS')  AS ip_cliente,
       SYS_CONTEXT('USERENV','SESSION_USER') AS usuario
FROM dual;

SELECT table_name FROM user_tables WHERE table_name LIKE 'DF\_%' ESCAPE '\' ORDER BY table_name;

-- =====================================================================
-- 1) DF_USUARIO
-- =====================================================================
INSERT INTO "DF_USUARIO" ("Id","Nome","Email","SenhaHash","Papel","CriadoEm")
VALUES (HEXTORAW('A0000000000000000000000000000001'), 'Maria Demo',
        'maria.demo@direitofacil.com', 'HASH_DEMO', 'Cidadao', SYSTIMESTAMP);
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Nome", "Email", "Papel", "CriadoEm", "AtualizadoEm"
FROM "DF_USUARIO" WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');

UPDATE "DF_USUARIO" SET "Nome" = 'Maria Demo Silva', "AtualizadoEm" = SYSTIMESTAMP
WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Nome", "AtualizadoEm"
FROM "DF_USUARIO" WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');

-- =====================================================================
-- 2) DF_DOCUMENTO
-- =====================================================================
INSERT INTO "DF_DOCUMENTO" ("Id","CidadaoId","Tipo","Descricao","NomeArquivo","ContentType",
       "TamanhoArquivo","BlobPath","HashSha256","Status","ConsentimentoSaude",
       "ConsentimentoSaudeEm","CriadoEm")
VALUES (HEXTORAW('B0000000000000000000000000000001'), 'maria.demo@direitofacil.com',
        'Laudo', 'Laudo médico', 'laudo.pdf', 'application/pdf', 102400,
        'demo/laudo.pdf', RPAD('a', 64, 'a'), 'Recebido', 1, SYSTIMESTAMP, SYSTIMESTAMP);
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "CidadaoId", "Tipo", "Descricao", "Status", "CriadoEm"
FROM "DF_DOCUMENTO" WHERE "Id" = HEXTORAW('B0000000000000000000000000000001');

-- simula a IA concluindo a extração
UPDATE "DF_DOCUMENTO" SET "Status" = 'Extraido', "Descricao" = 'Laudo TEA - CID F84.0',
       "AtualizadoEm" = SYSTIMESTAMP
WHERE "Id" = HEXTORAW('B0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Status", "Descricao", "AtualizadoEm"
FROM "DF_DOCUMENTO" WHERE "Id" = HEXTORAW('B0000000000000000000000000000001');

-- =====================================================================
-- 3) DF_ENVIO
-- =====================================================================
INSERT INTO "DF_ENVIO" ("Id","CidadaoId","CasoId","DireitoCodigo","Status","CriadoEm")
VALUES (HEXTORAW('C0000000000000000000000000000001'), 'maria.demo@direitofacil.com',
        1, 'ACOMPANHANTE_ESCOLAR_TEA', 'Rascunho', SYSTIMESTAMP);
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "CidadaoId", "DireitoCodigo", "Status", "CriadoEm"
FROM "DF_ENVIO" WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');

-- gestor aprova
UPDATE "DF_ENVIO" SET "Status" = 'Aprovado', "EnviadoEm" = SYSTIMESTAMP,
       "AnalisadoEm" = SYSTIMESTAMP, "GestorId" = 'gestor@direitofacil.com',
       "Observacao" = 'Documentação completa', "AtualizadoEm" = SYSTIMESTAMP
WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Status", "GestorId", "Observacao", "AnalisadoEm"
FROM "DF_ENVIO" WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');

-- =====================================================================
-- 4) DF_ENVIO_DOCUMENTO (ligação Envio x Documento)
-- =====================================================================
INSERT INTO "DF_ENVIO_DOCUMENTO" ("EnvioId","DocumentoId")
VALUES (HEXTORAW('C0000000000000000000000000000001'), HEXTORAW('B0000000000000000000000000000001'));
COMMIT;

SELECT RAWTOHEX(e."Id") AS ENVIO, e."DireitoCodigo", e."Status",
       RAWTOHEX(d."Id") AS DOCUMENTO, d."Tipo", d."NomeArquivo"
FROM "DF_ENVIO" e
JOIN "DF_ENVIO_DOCUMENTO" ed ON ed."EnvioId" = e."Id"
JOIN "DF_DOCUMENTO" d        ON d."Id" = ed."DocumentoId"
WHERE e."Id" = HEXTORAW('C0000000000000000000000000000001');

-- tabela só de chaves: o UPDATE troca o documento vinculado
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
-- 5) DF_DOCUMENTO_OBRIGATORIO (documentos exigidos por direito)
-- =====================================================================
INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo","TipoDocumento")
VALUES ('DEMO_DIREITO', 'Laudo');
COMMIT;

SELECT * FROM "DF_DOCUMENTO_OBRIGATORIO" WHERE "DireitoCodigo" = 'DEMO_DIREITO';

UPDATE "DF_DOCUMENTO_OBRIGATORIO" SET "TipoDocumento" = 'Receita'
WHERE "DireitoCodigo" = 'DEMO_DIREITO' AND "TipoDocumento" = 'Laudo';
COMMIT;

SELECT * FROM "DF_DOCUMENTO_OBRIGATORIO" WHERE "DireitoCodigo" = 'DEMO_DIREITO';

-- =====================================================================
-- 6) DF_IDEMPOTENCIA (chaves Idempotency-Key)
-- =====================================================================
INSERT INTO "DF_IDEMPOTENCIA" ("Chave","UsuarioId","Endpoint","Fingerprint","StatusCode",
       "RespostaJson","CriadoEm","ExpiraEm")
VALUES (HEXTORAW('D0000000000000000000000000000001'), 'maria.demo@direitofacil.com',
        'POST /api/documentos', RPAD('c', 64, 'c'), NULL, NULL,
        SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '1' DAY);
COMMIT;

SELECT RAWTOHEX("Chave") AS CHAVE, "UsuarioId", "Endpoint", "StatusCode", "ExpiraEm"
FROM "DF_IDEMPOTENCIA" WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');

-- requisição concluída: guarda status e resposta
UPDATE "DF_IDEMPOTENCIA" SET "StatusCode" = 201, "RespostaJson" = '{"id":"demo","status":"Recebido"}'
WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Chave") AS CHAVE, "StatusCode", "RespostaJson"
FROM "DF_IDEMPOTENCIA" WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');

-- =====================================================================
-- 7) DF_OUTBOX (eventos para o RabbitMQ)
--    Criado com ProcessadoEm preenchido para o OutboxWorker da API ignorar
-- =====================================================================
INSERT INTO "DF_OUTBOX" ("Id","Tipo","Payload","CriadoEm","ProcessadoEm","Tentativas")
VALUES (HEXTORAW('E0000000000000000000000000000001'), 'DocumentoRecebido',
        '{"documentoId":"demo"}', SYSTIMESTAMP, SYSTIMESTAMP, 0);
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Tipo", "Payload", "Tentativas", "ProcessadoEm"
FROM "DF_OUTBOX" WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');

UPDATE "DF_OUTBOX" SET "Tentativas" = 1, "UltimoErro" = 'Teste de demonstração'
WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');
COMMIT;

SELECT RAWTOHEX("Id") AS ID, "Tentativas", "UltimoErro"
FROM "DF_OUTBOX" WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');

-- =====================================================================
-- DELETE (filhos antes dos pais, por causa das FKs)
-- =====================================================================
DELETE FROM "DF_ENVIO_DOCUMENTO"       WHERE "EnvioId" = HEXTORAW('C0000000000000000000000000000001');
DELETE FROM "DF_ENVIO"                 WHERE "Id" = HEXTORAW('C0000000000000000000000000000001');
DELETE FROM "DF_DOCUMENTO"             WHERE "Id" IN (HEXTORAW('B0000000000000000000000000000001'),
                                                     HEXTORAW('B0000000000000000000000000000002'));
DELETE FROM "DF_USUARIO"               WHERE "Id" = HEXTORAW('A0000000000000000000000000000001');
DELETE FROM "DF_DOCUMENTO_OBRIGATORIO" WHERE "DireitoCodigo" = 'DEMO_DIREITO';
DELETE FROM "DF_IDEMPOTENCIA"          WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001');
DELETE FROM "DF_OUTBOX"                WHERE "Id" = HEXTORAW('E0000000000000000000000000000001');
COMMIT;

-- Comprova o DELETE: todas as colunas devem ser 0
SELECT
  (SELECT COUNT(*) FROM "DF_USUARIO"               WHERE "Id" = HEXTORAW('A0000000000000000000000000000001'))     AS USUARIO,
  (SELECT COUNT(*) FROM "DF_DOCUMENTO"             WHERE "CidadaoId" = 'maria.demo@direitofacil.com')           AS DOCUMENTO,
  (SELECT COUNT(*) FROM "DF_ENVIO"                 WHERE "Id" = HEXTORAW('C0000000000000000000000000000001'))     AS ENVIO,
  (SELECT COUNT(*) FROM "DF_ENVIO_DOCUMENTO"       WHERE "EnvioId" = HEXTORAW('C0000000000000000000000000000001')) AS ENVIO_DOC,
  (SELECT COUNT(*) FROM "DF_DOCUMENTO_OBRIGATORIO" WHERE "DireitoCodigo" = 'DEMO_DIREITO')                       AS DOC_OBRIG,
  (SELECT COUNT(*) FROM "DF_IDEMPOTENCIA"          WHERE "Chave" = HEXTORAW('D0000000000000000000000000000001'))  AS IDEMP,
  (SELECT COUNT(*) FROM "DF_OUTBOX"                WHERE "Id" = HEXTORAW('E0000000000000000000000000000001'))     AS OUTBOX
FROM dual;
