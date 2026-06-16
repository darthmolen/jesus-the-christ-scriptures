#!/usr/bin/env bash
#
# push-secrets.sh — upload the release secrets from ./.env to GitHub Actions
# (repo-level secrets) for the tag-triggered release workflow. Run
# collect-secrets.sh first. Secret VALUES are piped straight to `gh secret set`
# and are never printed. See docs/android/HOW-TO-DEPLOY.md.
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"

[ -f "$ENV_FILE" ] || { echo "ERROR: $ENV_FILE not found — run scripts/collect-secrets.sh first." >&2; exit 1; }
command -v gh >/dev/null || { echo "ERROR: gh CLI not found on PATH." >&2; exit 1; }

REPO="$(gh repo view --json nameWithOwner -q .nameWithOwner)"

# Read a single .env value by key (everything after the first '='), no sourcing.
val() { grep -E "^$1=" "$ENV_FILE" | head -n1 | cut -d= -f2-; }

printf '%s' "$(val PLAY_KEYSTORE_BASE64)"   | gh secret set PLAY_KEYSTORE_BASE64   --repo "$REPO"
printf '%s' "$(val PLAY_KEYSTORE_PASSWORD)" | gh secret set PLAY_KEYSTORE_PASSWORD --repo "$REPO"
# The workflow wants the raw JSON in PLAY_SERVICE_ACCOUNT_JSON; decode on the way in.
printf '%s' "$(val PLAY_SERVICE_ACCOUNT_JSON_BASE64)" | base64 -d | gh secret set PLAY_SERVICE_ACCOUNT_JSON --repo "$REPO"

echo "Pushed 3 secrets to $REPO (values not shown):"
echo "  PLAY_KEYSTORE_BASE64, PLAY_KEYSTORE_PASSWORD, PLAY_SERVICE_ACCOUNT_JSON"
