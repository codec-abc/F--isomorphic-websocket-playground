namespace Server

open System
open System.Timers
open System.Threading
open System.Threading.Tasks
open System.Net.WebSockets
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

open Shared
open Message
open Client
open Const
open MathUtil

type ServerEvent =
    | Tick
    | NewClient of ClientData
    | ReceivedMessage of ClientMessage * ClientData
    | DisconnectClient of ClientData

type Startup() =

    let mutable _webSocketIdentifier : int = 0
    let _clients : Dictionary<int, ClientData> = Dictionary<int, ClientData>()
    let _random = Random()
    let _timer = new Timers.Timer()
    let _serverEvents = ConcurrentQueue<ServerEvent>()

    member this.SendMessage(clientToSendTo : ClientData, msgContent : byte[]) =
        try
            clientToSendTo.socket.SendAsync(
                    buffer = new ArraySegment<byte>(msgContent),
                    messageType = WebSocketMessageType.Binary, 
                    endOfMessage = true, 
                    cancellationToken = CancellationToken.None
                ) |> ignore
            ()            
        with
            | ex ->
                Console.WriteLine("Exception happened during send: " + ex.Message)
                if clientToSendTo.socket.State <> WebSocketState.Open then
                    if _clients.ContainsKey(clientToSendTo.id) then
                        _clients.Remove(clientToSendTo.id) |> ignore
                        _serverEvents.Enqueue(DisconnectClient clientToSendTo)
                        Console.WriteLine("[1]-Websocket has closed. Removing it client : " + id.ToString())

    member this.Tick() =
        try
            _serverEvents.Enqueue(ServerEvent.Tick)
            let initialCount : int = _serverEvents.Count
            for _ in 0..(initialCount - 1) do
                let hasEvent, event = _serverEvents.TryDequeue()
                if hasEvent then
                    this.ProcessEvent(event) 
        with 
            | ex -> Console.WriteLine("error in tick " + ex.Message + " " + ex.StackTrace)

    member this.HandleNewClient(newClient : ClientData) =
        let newClientMsg : ServerMessageNewClientId = {
            id = newClient.id
            posX = newClient.posX
            posY = newClient.posY
        }        

        let msg = newClientMsg.ToByteArray()
        
        this.SendMessage(newClient, msg)
      
        for otherClientVP in _clients do
            let otherClient = otherClientVP.Value

            let playerTransformUpdateMsg : ServerMessagePlayerTransformUpdate = {
                id = otherClient.id
                posX = otherClient.posX
                posY = otherClient.posY
                orientation = otherClient.orientation
            }            

            let msg = playerTransformUpdateMsg.ToByteArray()
            this.SendMessage(newClient, msg)

        _clients.Add(newClient.id, newClient)

    member this.HandleClientMessage(msg : ClientMessage, sender : ClientData) =
        // TODO : we should not trust blindly clients' messages.

        match msg with
            | ClientMessagePlayerTransformUpdate mvMsg ->
                if _clients.ContainsKey(sender.id) then 
                    _clients.[sender.id].posX <- mvMsg.newPosX
                    _clients.[sender.id].posY <- mvMsg.newPosY
                    _clients.[sender.id].orientation <- mvMsg.orientation
            | ClientMessagePlayerShoot shootMsg ->

                let hitscanShootLine : HalfOpenLineSegment = {
                    start = {
                        X = shootMsg.originX
                        Y = shootMsg.originY
                    }
                    direction = {
                        X = Math.Cos(shootMsg.angle)
                        Y = Math.Sin(shootMsg.angle)
                    }
                }

                for clientVP in _clients do
                    let client = clientVP.Value
                    if client.id <> sender.id then
                        let clientHitCircle : Circle = {
                            center = {
                                X = client.posX
                                Y = client.posY
                            }
                            radius = PlayerRadius
                        }
                        let intersectResult = hitscanShootLine.Intersect(clientHitCircle)

                        if intersectResult.Length > 0 then
                            Console.WriteLine("client has been hit")
                            // TODO
                ()
            | UnknowMessage -> 
                Console.WriteLine("Unknown message received.")

    member this.HandleDisconnectClient(disconnectedClient : ClientData) =
        let playerDisconnectMsg : ServerMessagePlayerDisconnected = {
            idOfDisconnectedPlayer = disconnectedClient.id
        }

        let msg = playerDisconnectMsg.ToByteArray()

        for otherClient in _clients do
            this.SendMessage(otherClient.Value, msg)

    member this.ProcessEvent(event : ServerEvent) =
        match event with
            | NewClient newClient -> this.HandleNewClient(newClient)
            | ReceivedMessage(msg, client) -> this.HandleClientMessage(msg, client)
            | DisconnectClient client -> this.HandleDisconnectClient(client)
            | Tick ->
                for clientVP in _clients do
                    let client = clientVP.Value

                    let playerTransformUpdateMsg : ServerMessagePlayerTransformUpdate = {
                        id = client.id
                        posX = client.posX
                        posY = client.posY
                        orientation = client.orientation
                    }

                    let msg = playerTransformUpdateMsg.ToByteArray()

                    for otherClient in _clients do
                        this.SendMessage(otherClient.Value, msg)

    member this.RunClientSocket(webSocket : WebSocket) : Async<unit> =
        async {

            let id = _webSocketIdentifier
            Console.WriteLine("web socket accepted. Id = " + id.ToString())
            _webSocketIdentifier <- _webSocketIdentifier + 1

            let currentClient : ClientData = {
                socket = webSocket
                id = id
                posX = _random.NextDouble() * Width
                posY = _random.NextDouble() * Height
                orientation = 0.0
            }

            _serverEvents.Enqueue(ServerEvent.NewClient currentClient)
            let buffer = Array.create 2048 (byte 0)

            let mutable shouldContinue = true
            while shouldContinue do 
               try
                    let! result = 
                            webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None) |>
                            Async.AwaitTask

                    if result.EndOfMessage then
                            let msg = parseClientMessage buffer
                            _serverEvents.Enqueue(ServerEvent.ReceivedMessage(msg, currentClient))
                    with 
                        | ex ->
                            if webSocket.State <> WebSocketState.Open then
                                if _clients.ContainsKey(currentClient.id) then
                                    _clients.Remove(currentClient.id) |> ignore
                                    _serverEvents.Enqueue(DisconnectClient currentClient)
                                    Console.WriteLine("[2]- Websocket has closed. Removing it client : " + id.ToString())
                                shouldContinue <- false

            Console.WriteLine("client " + id.ToString() + " is ended.")
        }

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseDefaultFiles() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseFileServer(true)  |> ignore
        app.UseWebSockets()  |> ignore // Only for Kestrel 

        app.UseMvc(fun routes ->
            routes.MapRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}"
            ) |> ignore
        ) |> ignore

        app.Map(PathString("/lobby"), fun builder ->
            builder.Use(
                fun context next ->
                        let result : Task =
                            if context.WebSockets.IsWebSocketRequest then
                                let task = async { 
                                    let! websocket = Async.AwaitTask(context.WebSockets.AcceptWebSocketAsync())
                                    let! result = this.RunClientSocket(websocket)
                                    result
                                }
                                Task.Factory.StartNew(fun () -> Async.RunSynchronously task)
                                
                            else
                                next.Invoke()
                        result
            ) |> ignore
        ) |> ignore

        System.Threading.Tasks.Task.Run(
            fun () ->
                _timer.Interval <- 16.0

                let timerHandler : ElapsedEventHandler = 
                    ElapsedEventHandler(
                        fun a b -> 
                            this.Tick()
                )

                _timer.Elapsed.AddHandler(timerHandler)
                _timer.Start()
        ) |> ignore