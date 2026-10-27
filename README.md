# GallowsGame

[![Build & test](https://github.com/andrew1407/GallowsGame/actions/workflows/main.yml/badge.svg)](https://github.com/andrew1407/GallowsGame/actions/workflows/main.yml)
[![GitHub Pages](https://github.com/andrew1407/GallowsGame/actions/workflows/pages.yml/badge.svg)](https://github.com/andrew1407/GallowsGame/actions/workflows/pages.yml)

<p align="center">
  <a href="https://andrew1407.github.io/GallowsGame/">
    <img src="./Assets/Art/icon.png" width="140" alt="GallowsGame Logo" />
  </a>
</p>

<p align="center">
  <b><a href="https://andrew1407.github.io/GallowsGame/">Play in the browser</a></b> · <a href="https://github.com/andrew1407/GallowsGame/releases/latest">Download for Windows, macOS, Linux</a>
</p>

**GallowsGame (Hangman)** is a 2D sprite game with:

+ 4 difficulty levels;
+ [offline](./Assets/Scripts/Gameplay/Offline/OfflineStrategy.cs) mode;
+ online mode, 4 clients (using **[gallows-remastered API](https://github.com/Andrew1407/gallows-remastered)**):
  + [HTTP](./Assets/Scripts/Gameplay/Online/HttpClientStrategy.cs).
  + [WebSocket](./Assets/Scripts/Gameplay/Online/WebSocketClientStrategy.cs).
  + [UDP](./Assets/Scripts/Gameplay/Online/UdpClientStrategy.cs).
  + [TCP](./Assets/Scripts/Gameplay/Online/TcpClientStrategy.cs).

## Gameplay

Play offline:

![Offline mode](./Doc/Resources/main-menu-offline.png)

Play online:

![Online mode](./Doc/Resources/main-menu-online.png)

Choose difficulty:

![Difficulties](./Doc/Resources/difficulties.png)

Main gameplay (wordguessing):

![wordguessing](./Doc/Resources/gameplay.png)

Loss cases:

![Loss 1](./Doc/Resources/loss-1.png)

![Loss 3](./Doc/Resources/loss-2.png)

Win case (guess a word):

![Win](./Doc/Resources/win.png)

## Downloads

Every push to `main` builds the game for Windows, macOS, Linux and WebGL (the archives are attached to the [workflow run](https://github.com/andrew1407/GallowsGame/actions/workflows/main.yml)). Pushing a version tag also publishes them as a [release](https://github.com/andrew1407/GallowsGame/releases):

```sh
git tag v1.0.0 && git push origin v1.0.0
```

| Platform | Archive | Run |
| --- | --- | --- |
| Windows | `GallowsGame-Windows.zip` | `GallowsGame.exe` |
| macOS | `GallowsGame-macOS.zip` | `GallowsGame.app` (unsigned: run `xattr -cr GallowsGame.app` once, or right-click → Open) |
| Linux | `GallowsGame-Linux.tar.gz` | `./GallowsGame` |
| WebGL | `GallowsGame-WebGL.zip` | serve the folder over HTTP, or just [play it online](https://andrew1407.github.io/GallowsGame/) |

The browser version renders with **WebGPU** where the browser supports it and falls back to **WebGL 2** everywhere else (the WebGL player's graphics APIs are `WebGPU, WebGL 2`, in that order; **Build → Configure WebGPU with WebGL 2 Fallback** restores them). It plays offline only: browsers allow neither the raw TCP/UDP sockets nor the `HttpClient` the online clients use.

## Tests

[EditMode](./Assets/Tests/EditMode) tests cover the game rules, the offline strategy, difficulty stats, payload (de)serialization, all four online clients (HTTP against a fake handler; TCP, UDP and WebSocket against loopback servers), the strategy factory, resources, the gameplay components and how lost tries map onto hanger and body parts (checked against the hanger prefab). [PlayMode](./Assets/Tests/PlayMode) tests cover the UI and the game flow: the game loop (stage chaining, input handling, restart), the start menu (offline start, online toggle, address validation), text tabs, stage coroutines and the view controllers. Run them from **Window → General → Test Runner** or headless:

```sh
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults playmode.xml
```

## Building locally

Use the **Build** menu in the editor, or [build.ps1](./build.ps1) to build with the project's Unity version in batch mode and zip the results into `Builds/` (close the project in the editor first):

```powershell
./build.ps1                                       # Windows + WebGL
./build.ps1 -Platforms Windows,Linux,macOS,WebGL  # every platform whose build support module is installed
```
