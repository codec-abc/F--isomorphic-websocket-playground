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
open ServerMessageNewClientId
open Client
open Const

type ServerEvent =
    | Tick
    | NewClient of Client
    | ReceivedMessage of ClientMessage * Client
    | DisconnectClient of Client

type Startup() =

    let mutable _webSocketIdentifier : int = 0
    let _clients : List<Client> = List<Client>()
    let _random = Random()
    let _timer = new Timers.Timer()
    let _serverEvents = ConcurrentQueue<ServerEvent>()

    member this.SendMessage(clientToSendTo : Client, msgContent : byte[]) =
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
                if clientToSendTo.socket.State <> WebSocketState.Open then
                    if _clients.Contains(clientToSendTo) then
                        _clients.Remove(clientToSendTo) |> ignore
                        _serverEvents.Enqueue(DisconnectClient clientToSendTo)
                        Console.WriteLine("[1]-Websocket has closed. Removing it client : " + id.ToString())  

    member this.Tick() =
        _serverEvents.Enqueue(ServerEvent.Tick)
        let initialCount : int = _serverEvents.Count
        for i in 0..(initialCount - 1) do
            let hasEvent, event = _serverEvents.TryDequeue()
            if hasEvent then
                this.ProcessEvent(event)                    

    member this.HandleNewClient(newClient : Client) =
        let msg = 
            ServerMessageNewClientId.create(
                newClient.id, 
                newClient.posX, 
                newClient.posY
            ).ToByteArray()
        
        this.SendMessage(newClient, msg)
      
        for otherClient in _clients do
            let msg = 
                ServerMessagePlayerTransformUpdate.create(
                    otherClient.id,
                    otherClient.posX,
                    otherClient.posY,
                    otherClient.orientation
                ).ToByteArray()

            this.SendMessage(newClient, msg)

        _clients.Add(newClient)        

    member this.HandleClientMessage(msg : ClientMessage, sender : Client) =
        match msg with
            | ClientMessagePlayerTransformUpdate mvMsg ->
                let msg = 
                    ServerMessagePlayerTransformUpdate.create(
                        sender.id,
                        mvMsg.newPosX,
                        mvMsg.newPosY,
                        mvMsg.orientation
                    ).ToByteArray()
              
                for otherClient in _clients do
                    this.SendMessage(otherClient, msg)
            | UnknowMessage -> 
                Console.WriteLine("Unknown message received.")

    member this.HandleDisconnectClient(disconnectedClient : Client) =
        let msg = 
            ServerMessagePlayerDisconnected.create(
                disconnectedClient.id
            ).ToByteArray()

        for otherClient in _clients do
            this.SendMessage(otherClient, msg)
        ()        

    member this.ProcessEvent(event : ServerEvent) =
        match event with
            | NewClient newClient -> this.HandleNewClient(newClient)
            | ReceivedMessage(msg, client) -> this.HandleClientMessage(msg, client)
            | DisconnectClient client -> this.HandleDisconnectClient(client)
            | Tick -> () // TODO

    member this.RunClientSocket(webSocket : WebSocket) : Async<unit> =
        async {

            let id = _webSocketIdentifier
            Console.WriteLine("web socket accepted. Id = " + id.ToString())
            _webSocketIdentifier <- _webSocketIdentifier + 1

            let currentClient : Client = {
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
                                if _clients.Contains(currentClient) then
                                    _clients.Remove(currentClient) |> ignore
                                    _serverEvents.Enqueue(DisconnectClient currentClient)
                                    Console.WriteLine("[2]- Websocket has closed. Removing it client : " + id.ToString())
                                shouldContinue <- false

            Console.WriteLine("client " + id.ToString() + " is ended.")
        }

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore
        ()    

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

        let thread = 
            System.Threading.Tasks.Task.Run(
                fun () ->
                    _timer.Interval <- 30.0

                    let timerHandler : ElapsedEventHandler = 
                        ElapsedEventHandler(
                            fun a b -> 
                                this.Tick()
                    )

                    _timer.Elapsed.AddHandler(timerHandler)
                    _timer.Start()
            )
        ()