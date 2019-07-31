namespace Server

open System.Net.WebSockets

module Client =
    type ClientData = {
        socket : WebSocket
        id : int32
        mutable posX : float
        mutable posY : float
        mutable orientation : float
    }