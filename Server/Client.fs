namespace Server

open System.Net.WebSockets

module Client =
    type Client = {
        socket : WebSocket;
        id : int32;
        posX : float;
        posY : float;
    }