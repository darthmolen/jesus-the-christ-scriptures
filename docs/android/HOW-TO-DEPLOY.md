# How to deploy to Google Play

The app ships as a signed **Android App Bundle (`.aab`)** to the Google Play Console.

> **Preferred path: push a version tag and let CI ship it.** The
> [`release.yml`](../../.github/workflows/release.yml) workflow builds, signs, and uploads
> the bundle on any `v*` tag. The [manual build](#manual-fallback) below is the fallback for
> when CI is unavailable or you need a local artifact.

## Versioning (read this first)

The version lives in
[src/JesusTheChrist.App/JesusTheChrist.App.csproj](../../src/JesusTheChrist.App/JesusTheChrist.App.csproj),
in two fields that do different jobs:

| Field | Android name | Role |
|---|---|---|
| `ApplicationDisplayVersion` | `versionName` | Human-readable label (e.g. `1.0.1`). Shown to users. |
| `ApplicationVersion` | `versionCode` | **Integer Play uses to order/dedupe uploads.** Must strictly increase every upload, or Play rejects the bundle as a duplicate. |

A **git tag does not set the version** — the version is baked into the `.aab` from the
csproj at build time. The tag only *triggers* the release. So every release bumps the
csproj first.

Because the csproj is a tracked file and `main` is protected, the bump goes through a
**short PR** (see [Branch & Merge Policy in CLAUDE.md](../../CLAUDE.md)) — never a direct
commit to `main`.

## Preferred path: tag-triggered CI

### One-time setup

CI needs three repository secrets — `PLAY_KEYSTORE_BASE64`, `PLAY_KEYSTORE_PASSWORD`, and
`PLAY_SERVICE_ACCOUNT_JSON`. Don't add them by hand; the helper scripts read them from local
files and pipe them to GitHub without ever printing the values. But first you have to create
the **service-account JSON** (the credential CI uses to talk to the Play Developer API).

#### 1. Create the Play service-account JSON

> **The old way is gone.** Play Console's **Settings → API access** page no longer exists —
> `play.google.com/console/api-access` just redirects to the console home. Don't hunt for an
> "API access" menu item; it isn't there. The current flow is **Google-Cloud-first**, and you
> grant the account in Play Console afterward under *Users and permissions*.

**In Google Cloud — [console.cloud.google.com](https://console.cloud.google.com):**

1. Top bar: **create a project** (e.g. `jtc-play`) and select it.
2. **APIs & Services → Library** → search **"Google Play Android Developer API"** → **Enable**.
3. **APIs & Services → Credentials → + Create credentials → Help me choose**:
   - *Which API are you using?* **Google Play Android Developer API**
   - *What data will you be accessing?* **Application data** — this is the option that creates a
     **service account**. (*User data* makes an OAuth client, which is the wrong credential.)
   - **Next**.
4. Name it `play-publisher` → **Create and continue** → skip the optional roles → **Done**.
5. The wizard creates the account but **not** a key (Google split key creation out of it). Make
   the key separately: **IAM & Admin → Service Accounts** → click `play-publisher@…` →
   **Keys** tab → **Add key → Create new key → JSON → Create**. A `.json` downloads.
6. Save it next to the keystore — `C:\Users\swmol\keys\` — and keep it **out of the repo**.
7. Copy the account's **email**: `play-publisher@<project>.iam.gserviceaccount.com`.

**In Play Console — grant the account release access:**

8. Play Console → **Users and permissions → Invite new users**.
9. Paste the service-account **email** from step 7 (yes, a service account is invited exactly
   like a human user).
10. Under **App permissions**, add **Scriptures: Jesus The Christ** and grant **Release**
    permissions (release to testing tracks; add production if you'll auto-publish there later).
11. **Invite user.** Inviting the service account here *is* the link between Cloud and Play —
    there is no separate "link project" step in the new flow.

> New permission grants can take a few minutes to propagate. If the very first CI upload fails
> with a 403, wait a little and re-run. The app already has manual uploads on its tracks, so the
> "first bundle must be uploaded in the Console" requirement is already satisfied.

#### 2. Stage and push the secrets

The keystore lives outside the repo at `C:\Users\swmol\keys\vozloop-upload.keystore` (alias
`vozloop-upload`); `*.keystore`, `*.jks`, and `.env` are gitignored. Put the raw inputs in a
local `.env` at the repo root:

```
keystore_password=<your keystore store/key password>
service_account_json_path=C:/Users/swmol/keys/<the-service-account>.json
```

> Use **forward slashes** and **no quotes** in the path. Git Bash won't resolve `C:\Users\...`
> (backslashes) or a quoted `"C:\..."` — both forms fail silently. `C:/Users/...` or
> `/c/Users/...` both work.

Then run the two scripts (in order; values are never printed):

```bash
bash scripts/collect-secrets.sh   # base64-bundles the keystore + JSON into .env
bash scripts/push-secrets.sh      # uploads PLAY_KEYSTORE_BASE64, PLAY_KEYSTORE_PASSWORD,
                                  # and PLAY_SERVICE_ACCOUNT_JSON to GitHub Actions
```

`collect` honors an optional `keystore_path` in `.env` (defaults to
`~/keys/vozloop-upload.keystore`). To rotate a secret later, update the source file/value and
re-run both scripts.

### Releasing

1. **Bump the version** in the csproj via PR: `ApplicationDisplayVersion` → the new
   `x.y.z`, `ApplicationVersion` → the next integer. Merge it.
2. **Tag the merge commit** and push the tag:
   ```bash
   git checkout main && git pull
   git tag -a v1.0.2 -m "Release 1.0.2"
   git push origin v1.0.2
   ```
3. CI ([`release.yml`](../../.github/workflows/release.yml)) runs: it checks the tag matches
   the csproj `versionName`, builds + signs the `.aab`, and uploads it to the
   **`PLAY_TRACK`** track (default `internal`). Watch it under the repo's **Actions** tab.
4. If you publish to `internal`, **promote** to closed/production in the Console when ready
   (see [Tracks](#tracks)), or change `PLAY_TRACK` in the workflow to target a track
   directly.

The guard step fails the build if you tag `v1.0.2` while the csproj still says `1.0.1` —
that catches a forgotten bump before anything reaches Play.

## Tracks

| Track | Review | Use |
|---|---|---|
| **Internal testing** | none (live in minutes) | dogfooding; the CI default |
| **Closed testing** (e.g. "Alpha") | first setup reviewed (up to ~7 days) | the 20-tester × 14-day gate that unlocks production for personal accounts |
| **Production** | reviewed | public |

A bundle uploaded to one track lands in the app's **library**, so you can promote the same
`versionCode` across tracks without rebuilding (Console → the track → **Promote release**,
or **Create new release → Add from library**). Play always serves a tester the highest
`versionCode` available on a track it can see.

## Manual fallback

When CI isn't an option, build and upload by hand — this is exactly what CI automates.

1. Set the keystore env vars in your shell (the csproj signing block activates only when
   `JTC_KEYSTORE` is set):
   ```powershell
   setx JTC_KEYSTORE "C:\Users\swmol\keys\vozloop-upload.keystore"
   setx JTC_KEYSTORE_PASS "your-store-password"
   ```
   (`setx` persists for new shells; open a fresh terminal afterward.)
2. Build the signed bundle:
   ```bash
   dotnet publish src/JesusTheChrist.App/JesusTheChrist.App.csproj -c Release -f net10.0-android
   ```
   The signed `.aab` lands at
   `src/JesusTheChrist.App/bin/Release/net10.0-android/*-Signed.aab`.
3. In the Play Console, open the target track → **Create new release** → upload (or **Add
   from library**) → release name + notes → **Review release → Start rollout**.
4. Confirm the **Release summary** shows the new `versionCode` before rolling out.

## Listing & policy assets

- Store listing copy (title, descriptions, graphics checklist): [play-listing.md](play-listing.md)
- Privacy policy (host it and paste the URL in the Console): [privacy-policy.md](privacy-policy.md)
