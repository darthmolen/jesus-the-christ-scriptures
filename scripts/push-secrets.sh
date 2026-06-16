#!/usr/bin/env bash
#
# push-secrets.sh — upload the release secrets from ./.env to GitHub Actions
# (repo-level) for the tag-triggered release workflow. Run collect-secrets.sh
# first. Secret VALUES are piped to `gh secret set` and never printed.
# See docs/android/HOW-TO-DEPLOY.md.
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"

[ -f "$ENV_FILE" ] || { echo "ERROR: $ENV_FILE not found — run scripts/collect-secrets.sh first." >&2; exit 1; }
command -v gh >/dev/null || { echo "ERROR: gh CLI not found on PATH." >&2; exit 1; }

# Run gh from inside the checkout so repo detection works regardless of the caller's cwd.
cd "$REPO_ROOT"
REPO="$(gh repo view --json nameWithOwner -q .nameWithOwner)"
# `|| true` keeps a missing key from tripping `set -e`; `tr -d '\r'` strips CRLF.
val() { { grep -E "^$1=" "$ENV_FILE" || true; } | head -n1 | cut -d= -f2- | tr -d '\r'; }

KEYSTORE_B64="$(val keystore_base64)"
KEYSTORE_PW="$(val keystore_password)"
SA_B64="$(val service_account_json_base64)"

[ -n "$KEYSTORE_B64" ] || { echo "ERROR: keystore_base64 missing — run scripts/collect-secrets.sh." >&2; exit 1; }
[ -n "$KEYSTORE_PW" ]  || { echo "ERROR: keystore_password missing from .env." >&2; exit 1; }

printf '%s' "$KEYSTORE_B64" | gh secret set PLAY_KEYSTORE_BASE64   --repo "$REPO"
printf '%s' "$KEYSTORE_PW"  | gh secret set PLAY_KEYSTORE_PASSWORD --repo "$REPO"

if [ -n "$SA_B64" ]; then
  # The workflow wants the raw JSON in PLAY_SERVICE_ACCOUNT_JSON; decode on the way in.
  printf '%s' "$SA_B64" | base64 -d | gh secret set PLAY_SERVICE_ACCOUNT_JSON --repo "$REPO"
  echo "Pushed 3 secrets to $REPO (values not shown)."
else
  echo "Pushed 2 secrets to $REPO. PLAY_SERVICE_ACCOUNT_JSON skipped — not in .env yet;"
  echo "the release workflow's upload step will fail until it is set."
fi
