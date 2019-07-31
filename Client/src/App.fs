module App

open System
open System.Collections.Generic

open Fable.Core
open Fable.Core.JsInterop
open Fable.Pixi
open Fable.Pixi.PIXI
open Browser
open Browser.Types

open Shared
open Message
open MathUtil
open Graphics

type Player = {
    id : int32
    mutable posX : float
    mutable posY : float
    mutable lookingAngle : float
}

module Utils =
    let clamp (num : float, min : float, max : float) =
        if num <= min then
            min
        else if num >= max then
            max
        else
            num

    [<Emit("new DataView($0)")>]
    let createDataView (x: obj) : obj = jsNative

    [<Emit("new ArrayBuffer($0)")>]
    let createArrayBuffer (capacity: int) : obj = jsNative        

type App() =

    let _window = Dom.window
    let _webSocketProtocol = if _window.location.protocol = "https:"  then "wss:" else "ws:"
    let mutable _webSocketURI = _webSocketProtocol + "//" + _window.location.host + "/lobby";
    let mutable _playerId = -1
    let _players = Dictionary<int, Player>()
    let _keysStates = Dictionary<char, bool>()
    let mutable _mouseIsDown = false
    let _playerContainer = createPlayerContainer()
    let _ennemiesRoot = pixi.Container.Create()
    let _socket = WebSocket.Create(_webSocketURI)
    let _intervalMs = 8
    let _updateDelta = 2.0
    let mutable _intervalId = -1.0

    let _app = 
        let options : PIXI.ApplicationStaticOptions = unbox createObj []
        options.width <- Some Const.Width
        options.height <- Some Const.Height
        pixi.Application.Create(options)

    let _view = _app.view

    member private this.GetKeyState(key : char) =
        if _keysStates.ContainsKey key then
            _keysStates.[key]
        else
            false

    member private this.OnKeyUp(event : Event) =
       let keyCode : int = event?keyCode
       let key = char keyCode
       _keysStates.[key] <- false        

    member private this.OnKeyDown(event : Event) =
       let keyCode : int = event?keyCode
       let key = char keyCode
       _keysStates.[key] <- true

    member private this.OnMouseClick(event : Event) =
        _mouseIsDown <- true  

    member public this.Start() =
        _webSocketURI <- _webSocketURI.Replace("8080", "5000")

        document.body.appendChild _view |> ignore

        _app.renderer.plugins.interaction.cursorStyles.Item 
            "default" <- Some ("url('top-down-shooter/hud/cursor.png') 10 10 , auto" :> Object)

        _app.stage.addChild(_playerContainer) |> ignore
        _app.stage.addChild(_ennemiesRoot) |> ignore

        _socket.addEventListener_open(fun a -> this.OnSocketOpen(a))
        _socket.addEventListener_error(fun a -> this.OnSocketError(a))
        _socket.addEventListener_close(fun a -> this.OnSocketClose(a))
        _socket.addEventListener_message(fun a -> this.OnSocketMessage(a))

        _window.addEventListener("keyup", fun event -> this.OnKeyUp(event))
        _window.addEventListener("keydown", fun event -> this.OnKeyDown(event))
        _window.addEventListener("click", fun event -> this.OnMouseClick(event)) // or use _view to only grab click on canvas ?

        _intervalId <- _window.setInterval(
                fun event -> this.OnUpdate()
                , 
                _intervalMs
        )
        ()

    member this.OnSocketOpen(a : Event) =
        console.log("websocket open.")

    member this.OnSocketError(a : ErrorEvent) =
        console.log("error on websocket.")

    member this.OnSocketClose(a : CloseEvent) =
        console.log("websocket is closed.")

    member this.HandleMessage(message : ServerMessage) =
        match message with
            | ServerMessageNewClientId idMessage -> 
                _playerId <- idMessage.id
                let player = {
                        id = idMessage.id
                        posX = idMessage.posX
                        posY = idMessage.posY
                        lookingAngle = 0.0
                }
                _players.Add(idMessage.id, player)

            | ServerMessagePlayerTransformUpdate ppu ->
                let player = {
                        id = ppu.id
                        posX = ppu.posX
                        posY = ppu.posY
                        lookingAngle = ppu.orientation
                    }

                if ppu.id <> _playerId then
                    if _players.ContainsKey(ppu.id) then
                        _players.[ppu.id] <- player
                    else
                        _players.Add(ppu.id, player)                                        

            | ServerMessagePlayerDisconnected msg ->
                if _players.ContainsKey(msg.idOfDisconnectedPlayer) then
                    _players.Remove(msg.idOfDisconnectedPlayer) |> ignore                 

            | ServerMessage.UnknowMessage -> 
                console.log("unknow message received")

    member this.OnSocketMessage(a : MessageEvent) =
        let blob : Fable.Import.Browser.Blob = a?data
        let fileReader : Fable.Import.Browser.FileReader = Fable.Import.Browser.FileReader.Create()

        fileReader.onload <- fun a ->
            let mutable arrayBuffer = Utils.createArrayBuffer(0)
            arrayBuffer <- a?target?result
            let dv = Utils.createDataView arrayBuffer
            let length : int = dv?byteLength
            let array = Array.create length (byte 0)

            for i in 0..(length - 1) do
                let byte = dv?getUint8(i)
                array.[i] <- byte

            let message = Message.parseServerMessage array
            this.HandleMessage(message)

        fileReader.readAsArrayBuffer(blob)
        false    

    member this.GetMoveVectorAndMousePosition() =
        let moveVectorY = 
            if this.GetKeyState('W') then -1.0 else 0.0 
            + if this.GetKeyState('S') then 1.0 else 0.0

        let moveVectorX = 
            if this.GetKeyState('D') then 1.0 else 0.0 
            + if this.GetKeyState('A') then -1.0 else 0.0

        let moveVector = 
            { 
                X = moveVectorX
                Y = moveVectorY 
            }

        let mousePosition = _app.renderer.plugins.interaction.mouse.``global``

        let mousePos = {
            X = mousePosition.x
            Y = mousePosition.y
        }

        (moveVector, mousePos)

    member this.OnUpdate() =
        if _playerId >= 0 then
            let (moveVector, mousePos) = this.GetMoveVectorAndMousePosition()

            let myPos = {
                X = _players.[_playerId].posX
                Y = _players.[_playerId].posY
            }

            let dir = (mousePos - myPos).Normalized()
            let angle = atan2 dir.Y dir.X
            let oldAngle = _players.[_playerId].lookingAngle
            _players.[_playerId].lookingAngle <- angle

            let hasChanged = moveVector.X <> 0.0 || moveVector.Y <> 0.0 || oldAngle <> angle
            
            if moveVector.X <> 0.0 || moveVector.Y <> 0.0  then
                let currentPos = { X = _players.[_playerId].posX; Y = _players.[_playerId].posY }
                let newPos = currentPos + moveVector.Normalized() * _updateDelta
                
                _players.[_playerId].posX <- Utils.clamp(newPos.X, 0.0, Const.Width)
                _players.[_playerId].posY <- Utils.clamp(newPos.Y, 0.0, Const.Height)

            if hasChanged then
                let updateMsg : ClientMessagePlayerTransformUpdate =  {
                        id = _playerId
                        newPosX = _players.[_playerId].posX
                        newPosY = _players.[_playerId].posY 
                        orientation = angle
                    }

                _socket.send(updateMsg.ToByteArray())

            if _mouseIsDown then
                _mouseIsDown <- false

                let shootMsg : ClientMessagePlayerShoot =  {
                    id = _playerId
                    originX = _players.[_playerId].posX
                    originY = _players.[_playerId].posY 
                    angle = _players.[_playerId].lookingAngle
                }

                _socket.send(shootMsg.ToByteArray())

            this.DrawApp()

    member this.DrawApp () = 
        // TODO : avoid re-create ennemies each frame
        let count = _ennemiesRoot.children.Count

        for _ in 0..count - 1 do
            _ennemiesRoot.removeChildAt(0.0) |> ignore

        for kvp in _players do
            let player = kvp.Value 
            if kvp.Key = _playerId then
                _playerContainer.x <- player.posX
                _playerContainer.y <- player.posY
                _playerContainer.rotation <- player.lookingAngle - Math.PI / 2.0
            else
                let container = createEnnemyContainer()
                container.x <- player.posX
                container.y <- player.posY
                container.rotation <- player.lookingAngle - Math.PI / 2.0
                _ennemiesRoot.addChild(container) |> ignore
        ()                

let app = App()
app.Start()