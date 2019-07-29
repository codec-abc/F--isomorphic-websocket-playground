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
open MathUtil

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

type Player = {
    id : int32
    posX : float
    posY : float
}

let mutable myId = -1
let players = Dictionary<int, Player>()

let PIXI = PIXI.pixi

let app = PIXI.Application.Create()
let view = app.view
document.body.appendChild view |> ignore

let playerSpriteBody = PIXI.Sprite.from("top-down-shooter/characters/body/3.png")
let playerSpriteHead = PIXI.Sprite.from("top-down-shooter/characters/head/2.png")

let ennemySpriteBody = PIXI.Sprite.from("top-down-shooter/characters/body/1.png")
let ennemySpriteHead = PIXI.Sprite.from("top-down-shooter/characters/head/1.png")

let createPlayerContainer () = 
    let playerContainer = PIXI.Container.Create()
    let spriteHead = PIXI.Sprite.Create(playerSpriteHead.texture)
    let spriteBody= PIXI.Sprite.Create(playerSpriteBody.texture)
    spriteBody.x <- -11.0
    spriteBody.y <- -7.0
    spriteHead.x <- -11.0 + 3.0
    spriteHead.y <- -7.0 - 5.0
    playerContainer.addChild(spriteBody) |> ignore
    playerContainer.addChild(spriteHead) |> ignore
    playerContainer

let createEnnemyContainer () = 
    let ennemyContainer = PIXI.Container.Create()
    let spriteHead = PIXI.Sprite.Create(ennemySpriteHead.texture)
    let spriteBody= PIXI.Sprite.Create(ennemySpriteBody.texture)
    spriteBody.x <- -11.0
    spriteBody.y <- -7.0
    spriteHead.x <- -11.0 + 3.0
    spriteHead.y <- -7.0 - 5.0
    ennemyContainer.addChild(spriteBody) |> ignore
    ennemyContainer.addChild(spriteHead) |> ignore
    ennemyContainer

let playerContainer = createPlayerContainer()

app.stage.addChild(playerContainer) |> ignore

let ennemiesRoot = PIXI.Container.Create()

app.stage.addChild(ennemiesRoot) |> ignore

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

                let moveVector = 
                    { 
                        X = moveVectorX
                        Y = moveVectorY 
                    }

                if moveVector.X <> 0.0 || moveVector.Y <> 0.0 then
                    let currentPos = { X = players.[myId].posX; Y = players.[myId].posY }
                    let newPos = currentPos + moveVector * updateDelta
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
    // TODO : avoid re-create ennemies each frame
    let count = ennemiesRoot.children.Count
    for i in 0..count - 1 do
        ennemiesRoot.removeChildAt(0.0) |> ignore
    for kvp in players do
        let player = kvp.Value 
        if kvp.Key = myId then
            playerContainer.x <- player.posX
            playerContainer.y <- player.posY
        else
            let container = createEnnemyContainer()
            container.x <- player.posX
            container.y <- player.posY
            ennemiesRoot.addChild(container) |> ignore

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

            let message = Message.parseServerMessage array

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

                | ServerMessage.UnknowMessage -> 
                    console.log("unknow message received")
                    ()

            // console.log(message.ToString())

        fileReader.readAsArrayBuffer(blob)
        false
)

printfn "done!"
