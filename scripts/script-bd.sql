DECLARE
V_COUNT INTEGER;
BEGIN
SELECT COUNT(TABLE_NAME) INTO V_COUNT from USER_TABLES where TABLE_NAME = '__EFMigrationsHistory';
IF V_COUNT = 0 THEN
Begin
BEGIN 
EXECUTE IMMEDIATE 'CREATE TABLE 
"__EFMigrationsHistory" (
    "MigrationId" NVARCHAR2(150) NOT NULL,
    "ProductVersion" NVARCHAR2(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
)';
END;

End;

END IF;
EXCEPTION
WHEN OTHERS THEN
    IF(SQLCODE != -942)THEN
        RAISE;
    END IF;
END;
/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_DOCUMENTO" (
        "Id" RAW(16) NOT NULL,
        "CidadaoId" NVARCHAR2(254) NOT NULL,
        "Tipo" NVARCHAR2(40) NOT NULL,
        "Descricao" NVARCHAR2(300),
        "NomeArquivo" NVARCHAR2(255) NOT NULL,
        "ContentType" NVARCHAR2(100) NOT NULL,
        "TamanhoArquivo" NUMBER(19) NOT NULL,
        "BlobPath" NVARCHAR2(400) NOT NULL,
        "HashSha256" NVARCHAR2(64) NOT NULL,
        "Status" NVARCHAR2(20) NOT NULL,
        "ConsentimentoSaude" NUMBER(1) NOT NULL,
        "ConsentimentoSaudeEm" TIMESTAMP(7),
        "CriadoEm" TIMESTAMP(7) NOT NULL,
        "AtualizadoEm" TIMESTAMP(7),
        CONSTRAINT "PK_DF_DOCUMENTO" PRIMARY KEY ("Id")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_DOCUMENTO_OBRIGATORIO" (
        "DireitoCodigo" NVARCHAR2(60) NOT NULL,
        "TipoDocumento" NVARCHAR2(40) NOT NULL,
        CONSTRAINT "PK_DF_DOCUMENTO_OBRIGATORIO" PRIMARY KEY ("DireitoCodigo", "TipoDocumento")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_ENVIO" (
        "Id" RAW(16) NOT NULL,
        "CidadaoId" NVARCHAR2(254) NOT NULL,
        "CasoId" NUMBER(19) NOT NULL,
        "DireitoCodigo" NVARCHAR2(60) NOT NULL,
        "Status" NVARCHAR2(20) NOT NULL,
        "Observacao" NVARCHAR2(1000),
        "GestorId" NVARCHAR2(254),
        "EnviadoEm" TIMESTAMP(7),
        "AnalisadoEm" TIMESTAMP(7),
        "CriadoEm" TIMESTAMP(7) NOT NULL,
        "AtualizadoEm" TIMESTAMP(7),
        CONSTRAINT "PK_DF_ENVIO" PRIMARY KEY ("Id")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_IDEMPOTENCIA" (
        "Chave" RAW(16) NOT NULL,
        "UsuarioId" NVARCHAR2(254) NOT NULL,
        "Endpoint" NVARCHAR2(200) NOT NULL,
        "Fingerprint" NVARCHAR2(64) NOT NULL,
        "StatusCode" NUMBER(10),
        "RespostaJson" CLOB,
        "CriadoEm" TIMESTAMP(7) NOT NULL,
        "ExpiraEm" TIMESTAMP(7) NOT NULL,
        CONSTRAINT "PK_DF_IDEMPOTENCIA" PRIMARY KEY ("Chave", "UsuarioId", "Endpoint")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_OUTBOX" (
        "Id" RAW(16) NOT NULL,
        "Tipo" NVARCHAR2(100) NOT NULL,
        "Payload" CLOB NOT NULL,
        "CriadoEm" TIMESTAMP(7) NOT NULL,
        "ProcessadoEm" TIMESTAMP(7),
        "Tentativas" NUMBER(10) NOT NULL,
        "UltimoErro" NVARCHAR2(1000),
        CONSTRAINT "PK_DF_OUTBOX" PRIMARY KEY ("Id")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_USUARIO" (
        "Id" RAW(16) NOT NULL,
        "Nome" NVARCHAR2(120) NOT NULL,
        "Email" NVARCHAR2(254) NOT NULL,
        "SenhaHash" NVARCHAR2(200) NOT NULL,
        "Papel" NVARCHAR2(20) NOT NULL,
        "CriadoEm" TIMESTAMP(7) NOT NULL,
        "AtualizadoEm" TIMESTAMP(7),
        CONSTRAINT "PK_DF_USUARIO" PRIMARY KEY ("Id")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DF_ENVIO_DOCUMENTO" (
        "EnvioId" RAW(16) NOT NULL,
        "DocumentoId" RAW(16) NOT NULL,
        CONSTRAINT "PK_DF_ENVIO_DOCUMENTO" PRIMARY KEY ("EnvioId", "DocumentoId"),
        CONSTRAINT "FK_DF_ENVIO_DOCUMENTO_DF_DOCUMENTO_DocumentoId" FOREIGN KEY ("DocumentoId") REFERENCES "DF_DOCUMENTO" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DF_ENVIO_DOCUMENTO_DF_ENVIO_EnvioId" FOREIGN KEY ("EnvioId") REFERENCES "DF_ENVIO" ("Id") ON DELETE CASCADE
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    BEGIN
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ACOMPANHANTE_ESCOLAR_TEA', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_IDOSO', N'ComprovanteRenda');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_IDOSO', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_IDOSO', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_IDOSO', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_PCD', N'ComprovanteRenda');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_PCD', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_PCD', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BPC_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CADUNICO', N'ComprovanteRenda');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CADUNICO', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CADUNICO', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CADUNICO', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CIPTEA', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CIPTEA', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CIPTEA', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'FARMACIA_POPULAR', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'FARMACIA_POPULAR', N'Receita');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'FARMACIA_POPULAR', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PASSE_LIVRE_PCD', N'ComprovanteRenda');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PASSE_LIVRE_PCD', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PASSE_LIVRE_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PASSE_LIVRE_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'TARIFA_SOCIAL', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'TARIFA_SOCIAL', N'Cpf');
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE INDEX "IX_DF_DOCUMENTO_CidadaoId_CriadoEm" ON "DF_DOCUMENTO" ("CidadaoId", "CriadoEm")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE UNIQUE INDEX "IX_DF_DOCUMENTO_CidadaoId_HashSha256" ON "DF_DOCUMENTO" ("CidadaoId", "HashSha256")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE INDEX "IX_DF_ENVIO_CidadaoId_CriadoEm" ON "DF_ENVIO" ("CidadaoId", "CriadoEm")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE INDEX "IX_DF_ENVIO_Status" ON "DF_ENVIO" ("Status")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE INDEX "IX_DF_ENVIO_DOCUMENTO_DocumentoId" ON "DF_ENVIO_DOCUMENTO" ("DocumentoId")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE INDEX "IX_DF_IDEMPOTENCIA_ExpiraEm" ON "DF_IDEMPOTENCIA" ("ExpiraEm")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE INDEX "IX_DF_OUTBOX_ProcessadoEm_CriadoEm" ON "DF_OUTBOX" ("ProcessadoEm", "CriadoEm")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'CREATE UNIQUE INDEX "IX_DF_USUARIO_Email" ON "DF_USUARIO" ("Email")'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261110235154_Inicial';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES (N''20261110235154_Inicial'', N''10.0.8'')'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'DELETE FROM "DF_DOCUMENTO_OBRIGATORIO"
    WHERE "DireitoCodigo" = N''FARMACIA_POPULAR'' AND "TipoDocumento" = N''Cpf'''
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'DELETE FROM "DF_DOCUMENTO_OBRIGATORIO"
    WHERE "DireitoCodigo" = N''FARMACIA_POPULAR'' AND "TipoDocumento" = N''Receita'''
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'DELETE FROM "DF_DOCUMENTO_OBRIGATORIO"
    WHERE "DireitoCodigo" = N''FARMACIA_POPULAR'' AND "TipoDocumento" = N''Rg'''
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'DELETE FROM "DF_DOCUMENTO_OBRIGATORIO"
    WHERE "DireitoCodigo" = N''TARIFA_SOCIAL'' AND "TipoDocumento" = N''ComprovanteResidencia'''
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'DELETE FROM "DF_DOCUMENTO_OBRIGATORIO"
    WHERE "DireitoCodigo" = N''TARIFA_SOCIAL'' AND "TipoDocumento" = N''Cpf'''
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    BEGIN
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BOLSA_FAMILIA', N'ComprovanteRenda');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BOLSA_FAMILIA', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BOLSA_FAMILIA', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'BOLSA_FAMILIA', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_CONCURSO', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_CONCURSO', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_CONCURSO', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_EMPREGO', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_EMPREGO', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_EMPREGO', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_ENSINO_SUPERIOR', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_ENSINO_SUPERIOR', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'COTA_PCD_ENSINO_SUPERIOR', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CREDENCIAL_IDOSO', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CREDENCIAL_IDOSO', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CREDENCIAL_PCD', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CREDENCIAL_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'CREDENCIAL_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IOF_VEICULO_PCD', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IOF_VEICULO_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IOF_VEICULO_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IPI_VEICULO_PCD', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IPI_VEICULO_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IPI_VEICULO_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IR_65MAIS', N'Cpf');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'ISENCAO_IR_65MAIS', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'MEIA_ENTRADA_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'MEIA_ENTRADA_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PASSE_IDOSO_INTERESTADUAL', N'ComprovanteRenda');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PASSE_IDOSO_INTERESTADUAL', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PRIORIDADE_IDOSO', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PRIORIDADE_PCD', N'Laudo');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'PRIORIDADE_PCD', N'Rg');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'TARIFA_SOCIAL_ENERGIA', N'ComprovanteResidencia');
    INSERT INTO "DF_DOCUMENTO_OBRIGATORIO" ("DireitoCodigo", "TipoDocumento")
    VALUES (N'TARIFA_SOCIAL_ENERGIA', N'Cpf');
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118012908_AtualizaCatalogoDireitos';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES (N''20261118012908_AtualizaCatalogoDireitos'', N''10.0.8'')'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118022224_DocumentoPessoaId';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'ALTER TABLE "DF_DOCUMENTO" ADD "PessoaId" NUMBER(19)'
    ;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20261118022224_DocumentoPessoaId';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE 
    'INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES (N''20261118022224_DocumentoPessoaId'', N''10.0.8'')'
    ;
 END IF;
END;

/

