#!/usr/bin/env bash
#
# collect-secrets.sh — bundle the Google Play release secrets into ./.env
# (gitignored) by reading the keystore and service-account files from disk. Secret
# VALUES are never printed. Pair with push-secrets.sh. See docs/android/HOW-TO-DEPLOY.md.
#
# .env keys:
#   keystore_password            raw  — you set this
#   keystore_path                raw  — optional; default ~/keys/vozloop-upload.keystore
#   service_account_json_path    raw  — you set this once the Play service account exists
#   keystore_base64              built by this script from keystore_path
#   service_account_json_base64  built by this script from service_account_json_path
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"

fail() { echo "ERROR: $*" >&2; exit 1; }
b64()  { base64 "$1" | tr -d '\n'; }
val()  { grep -E "^$1=" "$ENV_FILE" | head -n1 | cut -d= -f2-; }

[ -f "$ENV_FILE" ] || fail ".env not found at $ENV_FILE (create it with at least 'keystore_password=...')."

PASSWORD="$(val keystore_password)"
[ -n "$PASSWORD" ] || fail "'keystore_password' missing from .env."

KEYSTORE_PATH="$(val keystore_path)"
KEYSTORE_PATH="${KEYSTORE_PATH:-$HOME/keys/vozloop-upload.keystore}"
[ -f "$KEYSTORE_PATH" ] || fail "keystore not found at '$KEYSTORE_PATH' (set 'keystore_path' in .env)."

SA_PATH="$(val service_account_json_path)"
SA_B64=""
if [ -n "$SA_PATH" ]; then
  [ -f "$SA_PATH" ] || fail "'service_account_json_path' set but file not found: '$SA_PATH'."
  SA_B64="$(b64 "$SA_PATH")"
else
  echo "WARN: 'service_account_json_path' not set — the Play upload secret will be omitted." >&2
fi

umask 077
{
  echo "# Google Play release secrets — DO NOT COMMIT (gitignored). Managed by scripts/collect-secrets.sh."
  echo "keystore_password=$PASSWORD"
  [ -n "$(val keystore_path)" ] && echo "keystore_path=$(val keystore_path)"
  [ -n "$SA_PATH" ] && echo "service_account_json_path=$SA_PATH"
  echo "keystore_base64=$(b64 "$KEYSTORE_PATH")"
  [ -n "$SA_B64" ] && echo "service_account_json_base64=$SA_B64"
} > "$ENV_FILE"

echo "Updated $ENV_FILE (values not shown). Service-account secret included: $([ -n "$SA_B64" ] && echo yes || echo NO)."
echo "Next: scripts/push-secrets.sh"
