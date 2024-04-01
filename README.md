# GallowsGame

[![main workflow badge](https://github.com/andrew1407/GallowsGame/actions/workflows/main.yml/badge.svg)](https://github.com/andrew1407/GallowsGame/actions)

<p align="center">
  <a href="https://github.com/andrew1407/GallowsGame/releases">
    <img src="./Assets/Art/icon.png" width="140" alt="DichBox Logo" />
  </a>
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
