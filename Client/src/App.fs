module App

open System
open System.Collections.Generic

open Fable.Core
open Fable.Core.JsInterop
open Fable.Pixi
open Browser
open Browser.Types

open Shared
open Message
open ClientIdMessage
open PlayerPositionUpdateMessage

let window = Dom.window

let mutable myCanvas : HTMLCanvasElement = unbox window.document.getElementById "myCanvas"  // myCanvas is defined in public/index.html

let webSocketProtocol = if window.location.protocol = "https:"  then "wss:" else "ws:"
let mutable webSocketURI = webSocketProtocol + "//" + window.location.host + "/lobby";

webSocketURI <- webSocketURI.Replace("8080", "5000")

let socket = WebSocket.Create(webSocketURI)

socket.addEventListener_open( 
    ignore
)

[<Emit("new DataView($0)")>]
let createDataView (x: obj) : obj = jsNative

[<Emit("new ArrayBuffer($0)")>]
let createArrayBuffer (capacity: int) : obj = jsNative

type Vector2 = {
    X : float
    Y : float
}

type Player = {
    id : int32
    posX : float
    posY : float
}

let mutable myId = -1
let players = Dictionary<int, Player>()

let PIXI = PIXI.pixi

let app = PIXI.Application.Create()
let graphics = PIXI.Graphics.Create()
app.stage.addChild(graphics) |> ignore
let view = app.view
document.body.appendChild view |> ignore

let keysStates = Dictionary<char, bool>()

let getKeyState (key : char) =
    if keysStates.ContainsKey key then
        keysStates.[key]
    else
        false

let intervalMs = 8
let updateDelta = 2.0

let intervalId = 
    window.setInterval(
        fun intervalEvent ->
            if myId >= 0 then
                let moveVectorY = 
                    if getKeyState('W') then -1.0 else 0.0 
                    + if getKeyState('S') then 1.0 else 0.0

                let moveVectorX = 
                    if getKeyState('D') then 1.0 else 0.0 
                    + if getKeyState('A') then -1.0 else 0.0

                let moveVector = { X = moveVectorX; Y = moveVectorY }

                if moveVector.X <> 0.0 || moveVector.Y <> 0.0 then

                    let currentPos = { X = players.[myId].posX; Y = players.[myId].posY }
                    let newPos = {
                        X = currentPos.X + moveVector.X * updateDelta
                        Y = currentPos.Y + moveVector.Y * updateDelta
                    }

                    let msg = PlayerMoveMessage.create(myId, newPos.X, newPos.Y).ToByteArray()

                    // console.log("sending update " + newPos.X.ToString() + " " + newPos.Y.ToString())

                    socket.send(
                        msg                    
                    )
        ,
        intervalMs
)    

window.addEventListener(
    "keyup",
    fun event -> 
        let keyCode : int = event?keyCode
        let key = char keyCode
        keysStates.[key] <- false
)

window.addEventListener(
    "keydown",
    fun event -> 
        let keyCode : int = event?keyCode
        let key = char keyCode
        keysStates.[key] <- true
)

socket.addEventListener_error(
    fun a ->
        console.log("error on websocket.")
)

socket.addEventListener_close(
    fun a ->
        console.log("websocket is closed.")
)

let drawApp () = 
    graphics.clear() |> ignore
    for kvp in players do
        let player = kvp.Value 
        if kvp.Key = myId then
            graphics.beginFill(float 0xDE3249) |> ignore
            graphics.drawRect(player.posX, player.posY, 4.0, 4.0) |> ignore
            graphics.endFill() |> ignore
        else
            graphics.beginFill(float 0x3500FA) |> ignore
            graphics.drawRect(player.posX, player.posY, 4.0, 4.0) |> ignore
            graphics.endFill() |> ignore
    ()

socket.addEventListener_message(
    fun a ->
        let blob : Fable.Import.Browser.Blob = a?data
        let fileReader : Fable.Import.Browser.FileReader = Fable.Import.Browser.FileReader.Create()

        // console.log("message received")
        
        fileReader.onload <- fun a ->
            let mutable arrayBuffer = createArrayBuffer(0)
            arrayBuffer <- a?target?result
            let dv = createDataView arrayBuffer
            let length : int = dv?byteLength
            let array = Array.create length (byte 0)

            for i in 0..length-1 do
                let byte = dv?getUint8(i)
                array.[i] <- byte

            let message = Message.parse array

            match message with
                | ClientIdMessage idMessage -> 
                    myId <- idMessage.id
                    let player = {
                            id = idMessage.id
                            posX = idMessage.posX
                            posY = idMessage.posY
                    }
                    players.Add(idMessage.id, player)

                | PlayerPositionUpdateMessage ppu -> 
                    let player = {
                            id = ppu.id
                            posX = ppu.posX
                            posY = ppu.posY
                        }

                    if players.ContainsKey(ppu.id) then
                        players.[ppu.id] <- player
                    else
                        players.Add(ppu.id, player)                                        

                    drawApp()

                | PlayerDisconnectedMessage msg ->
                    if players.ContainsKey(msg.idOfDisconnectedPlayer) then
                        players.Remove(msg.idOfDisconnectedPlayer) |> ignore
                        drawApp()

                | PlayerMoveMessage _ -> ()                    

                | UnknowMessage -> 
                    console.log("unknow message received")
                    ()

            // console.log(message.ToString())

        fileReader.readAsArrayBuffer(blob)
        false
)

printfn "done!"
