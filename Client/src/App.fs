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
let webSocketURI = webSocketProtocol + "//" + window.location.host + "/lobby";

let socket = WebSocket.Create(webSocketURI)

socket.addEventListener_open( 
    ignore
)

[<Emit("new DataView($0)")>]
let createDataView (x: obj) : obj = jsNative

[<Emit("new ArrayBuffer($0)")>]
let createArrayBuffer (capacity: int) : obj = jsNative

type Player = {
    id : int32
    posX : float
    posY : float
}

let mutable myId = 0
let players = Dictionary<int, Player>()

let PIXI = PIXI.pixi

let app = PIXI.Application.Create()
let graphics = PIXI.Graphics.Create()
app.stage.addChild(graphics) |> ignore
let view = app.view
document.body.appendChild view |> ignore

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

        console.log("message received")
        
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
                | UnknowMessage -> 
                    console.log("unknow message received")
                    ()

            console.log(message.ToString())

        fileReader.readAsArrayBuffer(blob)
        false
)

printfn "done!"
