module App

open System
open System.Collections.Generic

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Browser
open Browser.Types

open Shared
open Message
open ClientIdMessage
open PlayerPositionUpdateMessage

let window = Browser.Dom.window

// Get our canvas context 
// As we'll see later, myCanvas is mutable hence the use of the mutable keyword
// the unbox keyword allows to make an unsafe cast. Here we assume that getElementById will return an HTMLCanvasElement 

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

// Get the context
let ctx = myCanvas.getContext_2d()

// All these are immutables values
let w = myCanvas.width
let h = myCanvas.height

socket.addEventListener_error(
    fun a ->
        console.log("error on websocket.")
)

socket.addEventListener_close(
    fun a ->
        console.log("websocket is closed.")
)

let drawCanvas() = 
    ctx.clearRect(0.0, 0.0, w, h)
    for kvp in players do
    if kvp.Key = myId then
        ctx.fillStyle <- U3.Case1 "red"
    else
        ctx.fillStyle <- U3.Case1 "blue"
    let player = kvp.Value                        
    ctx.fillRect(player.posX, player.posY, 4.0, 4.0)

socket.addEventListener_message(
    fun a ->
        let blob : Browser.Blob = a?data
        let fileReader : Browser.FileReader = FileReader.Create()

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
                    
                    ctx.clearRect(0.0, 0.0, w, h)

                    drawCanvas()

                | PlayerDisconnectedMessage msg ->
                    if players.ContainsKey(msg.idOfDisconnectedPlayer) then
                        players.Remove(msg.idOfDisconnectedPlayer) |> ignore
                        drawCanvas()
                | UnknowMessage -> 
                    console.log("unknow message received")
                    ()

            console.log(message.ToString())

        fileReader.readAsArrayBuffer(blob)
        false
)


// prepare our canvas operations
// [0..steps] // this is a list
//   |> Seq.iter( fun x -> // we iter through the list using an anonymous function
//       let v = float ((x) * squareSize) 
//       ctx.moveTo(v, 0.)
//       ctx.lineTo(v, gridWidth)
//       ctx.moveTo(0., v)
//       ctx.lineTo(gridWidth, v)
//     ) 
// ctx.strokeStyle <- !^"#ddd" // color

// draw our grid
//ctx.stroke() 

// write Fable
//ctx.textAlign <- "center"
//ctx.fillText("Fable on Canvas", gridWidth * 0.5, gridWidth * 0.5)

printfn "done!"


