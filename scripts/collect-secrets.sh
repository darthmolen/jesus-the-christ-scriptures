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
# Read a .env value by key. The `|| true` keeps a missing key from tripping `set -e`,
# and `tr -d '\r'` strips Windows line endings that would corrupt the value.
val()  { { grep -E "^$1=" "$ENV_FILE" || true; } | head -n1 | cut -d= -f2- | tr -d '\r'; }
# Normalize a Windows path (C:\..., C:/...) to an MSYS path Git Bash tools accept.
norm() { if command -v cygpath >/dev/null 2>&1; then cygpath -u "$1"; else printf '%s' "${1//\\//}"; fi; }

[ -f "$ENV_FILE" ] || fail ".env not found at $ENV_FILE (create it with at least 'keystore_password=...')."

PASSWORD="$(val keystore_password)"
[ -n "$PASSWORD" ] || fail "'keystore_password' missing from .env."

KEYSTORE_RAW="$(val keystore_path)"
KEYSTORE_PATH="$(norm "${KEYSTORE_RAW:-$HOME/keys/vozloop-upload.keystore}")"
[ -f "$KEYSTORE_PATH" ] || fail "keystore not found at '$KEYSTORE_PATH' (set 'keystore_path' in .env)."

SA_RAW="$(val service_account_json_path)"
SA_B64=""
if [ -n "$SA_RAW" ]; then
  SA_PATH="$(norm "$SA_RAW")"
  [ -f "$SA_PATH" ] || fail "'service_account_json_path' set but file not found: '$SA_PATH'."
  SA_B64="$(b64 "$SA_PATH")"
else
  echo "WARN: 'service_account_json_path' not set — the Play upload secret will be omitted." >&2
fi

umask 077
{
  echo "# Google Play release secrets — DO NOT COMMIT (gitignored). Managed by scripts/collect-secrets.sh."
  echo "keystore_password=$PASSWORD"
  [ -n "$KEYSTORE_RAW" ] && echo "keystore_path=$KEYSTORE_RAW"
  [ -n "$SA_RAW" ] && echo "service_account_json_path=$SA_RAW"
  echo "keystore_base64=$(b64 "$KEYSTORE_PATH")"
  [ -n "$SA_B64" ] && echo "service_account_json_base64=$SA_B64"
} > "$ENV_FILE"
chmod 600 "$ENV_FILE"

echo "Updated $ENV_FILE (values not shown). Service-account secret included: $([ -n "$SA_B64" ] && echo yes || echo NO)."
echo "Next: scripts/push-secrets.sh"
