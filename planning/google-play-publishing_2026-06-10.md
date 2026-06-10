# Google Play Publishing — Plan & Recipe (V1)

**Goal:** ship V1 (full Standard Works) to Google Play as `com.vozloop.jesusthechristscriptures.full`,
published under **Molen Solutions LLC**, branded **Scriptures: Jesus The Christ**.

Legend: 🤖 = agent can do in-repo · 🧑 = you must do manually (account/secret/GUI).

---

## Already done ✅
- App icon (`appicon.png`) wired via `MauiIcon`; build verified (0/0), Android icon set regenerates.
- `ApplicationId` = `com.vozloop.jesusthechristscriptures.full` (permanent once published).
- Title `Scriptures: Jesus The Christ` (28 chars, under Play's 30 cap — no `(full)` in the visible title).
- Code builds clean; 73 Presentation tests green.

---

## The pipeline (in order)

### Phase A — Release signing config 🤖 (next branch, secret-free)
On the new `chore/play-release-setup` branch the agent will add:
1. A **Release-only** `PropertyGroup` in the csproj (Android), reading the keystore **path + passwords
   from environment variables** — nothing secret committed:
   ```xml
   <PropertyGroup Condition="'$(Configuration)'=='Release' and $(TargetFramework.Contains('-android'))">
     <AndroidKeyStore>true</AndroidKeyStore>
     <AndroidSigningKeyStore>$(JTC_KEYSTORE)</AndroidSigningKeyStore>
     <AndroidSigningKeyAlias>vozloop-upload</AndroidSigningKeyAlias>
     <AndroidSigningStorePass>$(JTC_KEYSTORE_PASS)</AndroidSigningStorePass>
     <AndroidSigningKeyPass>$(JTC_KEYSTORE_PASS)</AndroidSigningKeyPass>
     <AndroidPackageFormat>aab</AndroidPackageFormat>
   </PropertyGroup>
   ```
2. `.gitignore` entries: `*.keystore`, `*.jks`.
3. (Optional) a draft **privacy policy** (`docs/privacy-policy.md`) and **store listing copy** (`docs/play-listing.md`).

### Phase B — Generate the upload keystore 🧑 (one-time, you hold the password)
`keytool` ships with the JDK the Android workload uses. If it's not on PATH, find it under the JDK bin
(e.g. `C:\Program Files\Microsoft\jdk-<ver>\bin\keytool.exe`).

```bash
keytool -genkeypair -v -storetype PKCS12 \
  -keystore vozloop-upload.keystore \
  -alias vozloop-upload \
  -keyalg RSA -keysize 2048 -validity 10000
```
- Choose a **strong store password**; save it in your password manager. **Losing it is painful.**
- Identity prompts: name / org → "Molen Solutions".
- **Store the `.keystore` OUTSIDE the repo** (e.g. `C:\Users\swmol\keys\`). Back it up. Never commit it.
- Then set environment variables (PowerShell, persistent):
  ```powershell
  setx JTC_KEYSTORE "C:\Users\swmol\keys\vozloop-upload.keystore"
  setx JTC_KEYSTORE_PASS "<your-store-password>"
  ```
  (Open a fresh shell after `setx` so they're in scope.)

### Phase C — Build the signed app bundle 🤖/🧑
With the env vars set:
```bash
dotnet publish src/JesusTheChrist.App/JesusTheChrist.App.csproj -f net10.0-android -c Release
```
- Output: a signed **`.aab`** under `src/JesusTheChrist.App/bin/Release/net10.0-android/publish/`.
- Verify it exists and note its path. (Play requires `.aab`, not `.apk`, for new apps.)

### Phase D — Google Play Console 🧑 (GUI, your account)
1. **Create a Play Developer account** — **$25 one-time**. Register it under **Molen Solutions LLC**
   (business/organization account → liability + matches content-rights posture).
2. **Create app** → name `Scriptures: Jesus The Christ`, language, free, app (not game).
3. **Enroll in Play App Signing** (default for new apps) — you upload with the *upload* key; Google holds
   the real signing key. This is your safety net if the keystore is ever lost.
4. **Store listing assets** (gather/produce):
   - App icon **512×512** PNG ✅ (you have it — `appicon.png`).
   - **Feature graphic 1024×500** PNG/JPG (needs to be made — agent can draft from the tomb art).
   - **Phone screenshots** ×2–8 (min 320px, 16:9-ish). Capture from a device/emulator of: Home/feed,
     a reference card, the invitation, settings. (🧑 capture; agent can help stage.)
   - Short description (≤80 chars) + full description (≤4000). 🤖 can draft.
   - Category: **Books & Reference** (or Lifestyle). Contact email, website (optional).
5. **Privacy policy URL** — **required**. Host the policy (GitHub Pages, your site, etc.). 🤖 drafts the text.
6. **Data safety form** — your app stores notes/settings **locally** and collects ~nothing → declare
   accordingly (no data collected/shared). Easy, but must be filled.
7. **Content rating** questionnaire → likely "Everyone".
8. **Target audience / ads** → no ads; pick audience (all ages / adults as you prefer).

### Phase E — Testing → Production 🧑 ⚠️ *the timeline gate*
- **New personal dev accounts** must run **closed testing: 20 testers for 14 continuous days** before they
  can apply for production. **Organization (LLC) accounts may be exempt or differ — verify when you register;
  this can save two weeks.**
- Flow: **Internal testing** (instant, you + a few) → **Closed testing** (the 20×14 if required) →
  **apply for Production** → Google review (hours–days) → **live**.
- Upload the `.aab` to the chosen track, set release notes, roll out.

---

## Landmines (read before you click publish)
- **Package id is permanent.** `com.vozloop.jesusthechristscriptures.full` is final on first publish. ✅ chosen.
- **AAB, not APK**, for new apps. ✅ config handles it.
- **Keystore safety** — lose it without Play App Signing and you can't update. Back it up.
- **20 testers × 14 days** for new personal accounts — biggest schedule risk. LLC/org account may avoid it.
- **Privacy policy is mandatory**, even with zero data collection.
- **Content rights (Intellectual Reserve / the Church)** — the app bundles the Topical Guide, conference
  talks, and Liahona Spanish translations, all Church-copyrighted. Sort out permission/usage and **avoid
  implying official endorsement** before a *public* listing. Internal/closed testing is lower risk; public
  production is where this matters. This can block the listing — handle it deliberately.
- **targetSdk currency** — Play requires a recent target API each year; net10-android targets a current API,
  but confirm at publish time.

---

## Open decisions for you
1. **Developer account type:** personal vs **Molen Solutions LLC** (recommend LLC — liability + skips the
   20×14 gate if treated as org). 
2. **Privacy policy hosting:** where? (GitHub Pages off this repo is free and easy.)
3. **Feature graphic + screenshots:** agent drafts the 1024×500 from the tomb art; you capture device shots.
4. **Content-rights path:** proceed to *testing* now, resolve Church/IP permission before *public production*.

---

## Immediate next step
After PR #17 merges → agent pulls `main`, creates `chore/play-release-setup`, and does **Phase A**
(signing config + `.gitignore` + draft privacy policy + draft listing copy). Then you do **Phase B** (keystore).
