# GallowsGame

![main workflow badge](https://github.com/andrew1407/GallowsGame/actions/workflows/main.yml/badge.svg)

**GallowsGame (Hangman)** is a 2D sprite game with:

+ 4 difficulty levels;
+ [offline](./Assets/Scripts/Gameplay/Offline/OfflineStrategy.cs) mode;
+ online mode, clients:
    1. [HTTP](./Assets/Scripts/Gameplay/Online/HttpClientStrategy.cs);
    2. [WebSocket](./Assets/Scripts/Gameplay/Online/WebSocketClientStrategy.cs);
    3. [UDP](./Assets/Scripts/Gameplay/Online/UdpClientStrategy.cs);
    4. [TCP](./Assets/Scripts/Gameplay/Online/TcpClientStrategy.cs);

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
