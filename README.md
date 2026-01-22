# AlchemicShot

A Unity game about combining imbued bullets and slaughter.

## Quick Links
- **Game Design Doc:** [design.md](docs/design.md)
- **Project Board:** [Trello](trello.com)
- **Builds:** [Releases](https://github.com/Team-Alchemic-Shot/AlchemicShot/releases)
- **Team Chat:** [Discord](https://discord.gg/Yf42wfZmNd)
- **Trello Board:** https://trello.com/invite/b/69726493b75c0b2eb4e642e7/ATTI869c733c876a1f9208cd78528688a1dd7015677B/alchemicshot 

## Team
- **Producer / PM:** @TBD
- **Design:** @TBD
- **Programming:** @TBD
- **Art:** @TBD
- **Audio:** @TBD

## Status
- **Current milestone:** Alpha (Due: 2026-2-12)
- **Latest playable build:** None
- **Known top issues:** [Trello](trello.com)

## Controls
- **Move:** 
- **Aim:** 
- **Shoot / Primary:** 
- **Interact:** 
- **Pause:** 

## How to Run (Dev)
**Unity Version:** 2022.3.62f3

1. Install Unity Hub + the required Unity version.
2. Clone the repo:
	- `git clone <repo-url>`
3. Open the project in Unity Hub.
4. Press Play in the editor.

## Branching & PRs
- **Main branches:** `main` (stable), `dev` (integration)
- **Feature branches:** `feature/<short-name>`
- **PR rules:** small PRs, screenshots for gameplay changes, link issue, request review from 1–2 teammates.

## Trello Rules
We use Trello as the source of truth for work tracking (required for class).

- **All coding/editor tasks & bugfixes must have a linked GitHub Issue.**
  - The Trello card must include the GitHub Issue link.
  - The GitHub Issue should link back to the Trello card (or reference it in the description).
- **Every GitHub Issue gets its own branch.**
  - Branch from `dev` using: `issue/<short-name>` (example: `issue/input-rebinds`)
- **PRs must link the Issue** (which links the Trello card) and target `dev`.
- Move cards as you work: **To Do → In Progress → In Review → Done**.

## Project Conventions
- **Scenes:** `Assets/Scenes/` (keep one “Main” scene entry point)
- **Scripts:** `Assets/Scripts/` (namespace: `AlchemicShot`)
- **Art/Audio:** `Assets/Art/`, `Assets/Audio/`
- **Prefabs:** `Assets/Prefabs/`
- **Naming:** `PascalCase` for scripts/classes, `camelCase` for locals
- **Docs**: `docs/`

## Tech Notes
- **Input System:** New
- **Target platform(s):** Windows/macOS/Linux

## How to Build
- **Windows/macOS/Linux:** File → Build Settings → <platform> → Build
- **Build Storage:** If you are building for testing, place in [builds](Builds/), if building a deliverable, create a release on [Github](https://github.com/Team-Alchemic-Shot/AlchemicShot/releases)

## Assets & Credits
- **Third-party assets:** [Unity Store List](https://assetstore.unity.com/lists/alchemic-shot-2475922618331)
- **Third-party credits:** [CREDITS](CREDITS.md)
- **Licenses:** see [LICENSE](LICENSE)

## Contact / Help
If you’re new to the project: good luck!
