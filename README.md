# Test Desktop

A C# desktop starter app built with .NET 10 and Avalonia. It opens a window where you can enter your name and click **Say hello**. Avalonia supports Windows, macOS, and Linux.

## Open in VS Code

1. Install the .NET SDK version in `global.json` (10.0.401, or a newer patch in the same feature band).
2. Open this repository folder in VS Code and install the recommended **C# Dev Kit** extension.
3. Press **F5** to build and debug the app, or run the **run desktop app** task from **Terminal → Run Task**.

You need a graphical desktop session to see the window. Linux also needs X11 and fontconfig libraries; a normal desktop installation typically provides them.

## Commands

From the repository root:

```sh
dotnet restore TestDesktop.slnx --locked-mode
dotnet build TestDesktop.slnx --no-restore
dotnet run --project src/TestDesktop --no-build
dotnet test TestDesktop.slnx --no-build --no-restore
```

The two UI tests create the real window using Avalonia's headless platform and exercise the greeting button. They do not require a display.

## GitHub Codespaces

The `.devcontainer` configuration installs the pinned .NET SDK, C# Dev Kit, and a browser-accessible Linux desktop. It restores dependencies, builds the app, and runs the UI tests when the container is created.

For an existing Codespace, pull the latest repository changes and run **Codespaces: Rebuild Container** from the command palette. After rebuilding:

1. Run `dotnet run --project src/TestDesktop`, or press **F5** to debug.
2. Open the **Ports** tab and open port **6080**, labelled **Desktop**, in your browser. Keep its visibility **Private**.
3. Connect to the desktop using the feature's default VNC password, `vscode`. The app window appears on that desktop.

The Codespaces container configuration has not been built in this cloud environment. The application build, rendering tests, and native Linux window startup have been verified here.

## Cloud environment

The cloud setup installs the SDK at `/workspace/.dotnet` and virtual-display tools at `/workspace/.desktop-tools`. Activate them in each new shell:

```sh
source /workspace/.cloud-setup/env.sh
cd /workspace/test
dotnet build TestDesktop.slnx --no-restore
dotnet test TestDesktop.slnx --no-build --no-restore
```

To run the native Linux app on a virtual display:

```sh
xvfb-run -a dotnet run --project src/TestDesktop --no-build
```

This checks native startup in the cloud; it does not expose a visible desktop in this chat. Use VS Code on a machine with a graphical desktop to interact with the window.

## Project layout

- `src/TestDesktop`: application, window layout, and button handler.
- `tests/TestDesktop.Tests`: headless UI tests.
- `.vscode`: extension recommendation and build, run, test, and debug configurations.

NuGet dependencies are pinned in the project files and package lockfiles. When intentionally changing dependencies, run `dotnet restore TestDesktop.slnx --force-evaluate` and review the lockfile changes.
