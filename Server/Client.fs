namespace Server

open System.Net.WebSockets

module Client =
    type Client = {
        socket : WebSocket
        id : int32
        mutable posX : float
        mutable posY : float
    }