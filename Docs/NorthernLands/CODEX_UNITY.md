# Codex + Unity Editor

The project uses one Unity bridge: **MCP for Unity 9.7.1**. The package is pinned in
`Packages/manifest.json`, and the Codex endpoint is scoped to this trusted project in
`.codex/config.toml`.

## First local connection

1. Open this repository in Unity `6000.0.52f1` and wait for package import and script compilation.
2. Open `Window > MCP for Unity` and configure/start the local server for Codex.
3. Keep the endpoint bound to `127.0.0.1:8080`; do not expose it on `0.0.0.0`.
4. Restart Codex from this trusted project, then verify that the Unity tools and active instance appear.

The local editor bootstrap automatically generates Riverholm, the Dead World and the Tower of Gods,
then opens `Startup` so Play Mode begins at the main menu. Batch/cloud builds continue to use
`CloudBuildHooks.PreExport` and do not depend on the local MCP server.
