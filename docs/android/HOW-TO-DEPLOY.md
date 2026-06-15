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

Add three repository secrets (**Settings → Secrets and variables → Actions**):

| Secret | What it is | How to produce it |
|---|---|---|
| `PLAY_KEYSTORE_BASE64` | The upload keystore, base64-encoded | `base64 -w0 vozloop-upload.keystore` (or `certutil -encode` on Windows, stripped of headers) |
| `PLAY_KEYSTORE_PASSWORD` | The store/key password | from your password manager |
| `PLAY_SERVICE_ACCOUNT_JSON` | Play Developer API service-account key (full JSON) | Google Cloud Console → create a service account → grant it access in Play Console (**Users and permissions → Invite → grant "Releases"**) → download a JSON key |

The keystore itself is **never committed** — it lives outside the repo (locally at
`C:\Users\swmol\keys\vozloop-upload.keystore`, alias `vozloop-upload`) and reaches CI only
as the base64 secret. `*.keystore` / `*.jks` are gitignored.

> Google requires the **first** bundle for a brand-new app to be uploaded through the
> Console UI before the API will accept uploads. That was already done for 1.0, so the API
> path is open.

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
