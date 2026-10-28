// The browser's WebSocket for BrowserWebSocketConnection (Gameplay/Online/WebSocketConnections.cs):
// sockets are kept by id, and their events call back into C# with that id
var BrowserWebSocket = {
  $browserSockets: { next: 1, byId: {} },

  BrowserWebSocket_Connect: function (addressPtr, onOpen, onMessage, onClose) {
    var id = browserSockets.next++;
    var socket = new WebSocket(UTF8ToString(addressPtr));
    browserSockets.byId[id] = socket;
    socket.onopen = function () {
      {{{ makeDynCall('vi', 'onOpen') }}}(id);
    };
    socket.onmessage = function (event) {
      if (typeof event.data !== 'string') return;
      var messagePtr = stringToNewUTF8(event.data);
      {{{ makeDynCall('vii', 'onMessage') }}}(id, messagePtr);
      _free(messagePtr);
    };
    // A failed connection raises an error and then closes, so close alone covers both
    socket.onclose = function () {
      delete browserSockets.byId[id];
      {{{ makeDynCall('vi', 'onClose') }}}(id);
    };
    return id;
  },

  BrowserWebSocket_Send: function (id, messagePtr) {
    var socket = browserSockets.byId[id];
    if (socket && socket.readyState === WebSocket.OPEN) socket.send(UTF8ToString(messagePtr));
  },

  BrowserWebSocket_Close: function (id) {
    var socket = browserSockets.byId[id];
    if (socket) socket.close();
  },
};

autoAddDeps(BrowserWebSocket, '$browserSockets');
mergeInto(LibraryManager.library, BrowserWebSocket);
