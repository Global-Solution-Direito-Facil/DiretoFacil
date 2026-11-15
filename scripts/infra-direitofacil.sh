#!/usr/bin/env bash
# =============================================================
# infra-direitofacil.sh – Infraestrutura do DireitoFácil (Global Solution 2026/2)
# Disciplina: DevOps Tools & Cloud Computing
#
# Executado pela pipeline de CI no Azure DevOps, no job "Criar Infra Inicial"
# (tarefa Azure CLI), antes do job "Build and Test" — mesmo modelo do lab
# "Azure Pipelines API .NET com Oracle" e da Sprint 4 do Challenge (PetPulse).
#
# É idempotente: pode rodar a cada build sem efeito colateral.
#   1. Banco Oracle (FIAP): cria as tabelas do scripts/script-bd.sql SOMENTE se
#      elas ainda não existirem. Nunca apaga nem altera dados existentes.
#   2. Azure: cria (ou reaproveita) o Grupo de Recursos, o Plano de Serviço e o
#      Serviço de Aplicativo (Web App) onde o CD publica a API.
#
# Parâmetros POSICIONAIS (campo "Script Arguments" da tarefa Azure CLI):
#   $1 ORACLE_HOST   $2 ORACLE_PORT   $3 ORACLE_SID   $4 ORACLE_USER   $5 ORACLE_PASS
#   A senha chega como argumento porque variáveis SECRETAS da pipeline não são
#   expostas como variável de ambiente para scripts.
#
# Variáveis de ambiente (variáveis comuns da pipeline, expostas automaticamente):
#   NOME_WEBAPP – nome do Web App, único em todo o Azure (ex.: direitofacil-rm561378)
#   LOCATION    – região do Azure (ex.: southafricanorth)
#   RUNTIME     – opcional, runtime do Web App (padrão: dotnet:10)
# =============================================================
set -euo pipefail

DbHost="${1:?ORACLE_HOST ausente}"
DbPort="${2:?ORACLE_PORT ausente}"
DbSid="${3:?ORACLE_SID ausente}"
DbUser="${4:?ORACLE_USER ausente}"
DbPass="${5:?ORACLE_PASS ausente}"

app="${NOME_WEBAPP:?Variável NOME_WEBAPP não definida na pipeline}"
location="${LOCATION:?Variável LOCATION não definida na pipeline}"
runtime="${RUNTIME:-dotnet:10}"
rg="rg-direitofacil"
plan="plan-direitofacil"
sku="F1"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DDL_FILE="$SCRIPT_DIR/script-bd.sql"

# ===================================
# 1. Banco de Dados Oracle
# ===================================

if [ ! -f "$DDL_FILE" ]; then
  echo "ERRO: $DDL_FILE não encontrado (gere com 'dotnet ef migrations script --idempotent')."
  exit 1
fi

# SQLcl portátil (cliente de linha de comando do Oracle)
if [ ! -x "$HOME/sqlcl/bin/sql" ]; then
  echo "Baixando SQLcl..."
  curl -sSL -o "$HOME/sqlcl.zip" "https://download.oracle.com/otn_software/java/sqldeveloper/sqlcl-latest.zip"
  echo "Extraindo SQLcl..."
  unzip -q -o "$HOME/sqlcl.zip" -d "$HOME"
fi
SQLBIN="$HOME/sqlcl/bin/sql"
CONN="${DbUser}/${DbPass}@${DbHost}:${DbPort}/${DbSid}"

echo "Verificando se as tabelas do DireitoFácil já existem no Oracle..."
existe=$("$SQLBIN" -S "$CONN" <<'SQL' | tr -d '[:space:]'
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SELECT COUNT(*) FROM user_tables WHERE table_name = 'DF_USUARIO';
EXIT
SQL
) || { echo "ERRO: não foi possível consultar o Oracle (verifique host, usuário e senha)."; exit 1; }

if ! [[ "$existe" =~ ^[0-9]+$ ]]; then
  echo "ERRO: resposta inesperada do Oracle ao verificar as tabelas:"
  echo "$existe"
  exit 1
fi

if [ "$existe" = "0" ]; then
  echo "Tabelas não encontradas: criando a partir do script-bd.sql..."
  {
    echo "WHENEVER SQLERROR EXIT SQL.SQLCODE"
    tr -d '\r' < "$DDL_FILE"   # o script-bd.sql é gerado no Windows (quebras de linha CRLF)
    echo
    echo "EXIT"
  } > cria_objetos.sql
  "$SQLBIN" -S "$CONN" @cria_objetos.sql
  echo "[OK] Tabelas do DireitoFácil criadas"
else
  echo "[OK] Tabelas já existem — nada a fazer no banco (dados preservados)"
fi

# ===================================
# 2. Recursos da Azure
# ===================================

echo "Criando (ou reaproveitando) o Grupo de Recursos $rg..."
az group create --name "$rg" --location "$location" 1>/dev/null

# O plano precisa ser Windows (a API é publicada como Web App Windows).
plan_linux="$(az appservice plan show --resource-group "$rg" --name "$plan" --query reserved -o tsv 2>/dev/null || true)"
if [ "$plan_linux" = "true" ]; then
  echo "ERRO: o Plano de Serviço $plan já existe, mas é Linux — a API usa Web App Windows."
  echo "Exclua o plano (ou o grupo $rg) no Portal do Azure e rode a pipeline de novo."
  exit 1
fi

echo "Criando (ou reaproveitando) o Plano de Serviço $plan ($sku, Windows)..."
# A partir do Azure CLI 2.90 o padrão do "plan create" passou a ser Linux — por isso o
# --is-linux false explícito.
az appservice plan create --name "$plan" --resource-group "$rg" --location "$location" --sku "$sku" --is-linux false 1>/dev/null

if az webapp show --resource-group "$rg" --name "$app" &>/dev/null; then
  echo "Serviço de Aplicativo $app já existe — reaproveitando."
else
  echo "Criando o Serviço de Aplicativo $app (runtime $runtime)..."
  az webapp create --resource-group "$rg" --plan "$plan" --runtime "$runtime" --name "$app" 1>/dev/null
fi

# Logs do Serviço de Aplicativo (aplica só se ainda não estiverem como desejado)
app_logging="$(az webapp log show -g "$rg" -n "$app" --query 'applicationLogs.fileSystem.level' -o tsv 2>/dev/null || true)"
ws_logging="$(az webapp log show -g "$rg" -n "$app" --query 'httpLogs.fileSystem.enabled' -o tsv 2>/dev/null || true)"

if [ "$app_logging" != "Information" ] || [ "$ws_logging" != "true" ]; then
  echo "Habilitando logs do Serviço de Aplicativo..."
  az webapp log config \
    --resource-group "$rg" \
    --name "$app" \
    --application-logging filesystem \
    --web-server-logging filesystem \
    --level information \
    --detailed-error-messages true \
    --failed-request-tracing true 1>/dev/null
else
  echo "Logs já configurados."
fi

echo "[OK] Recursos da Azure prontos"
echo "API (após o primeiro deploy do CD): https://${app}.azurewebsites.net"
