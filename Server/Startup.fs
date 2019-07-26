namespace Server

open System
open System.Threading
open System.Threading.Tasks
open System.Net.WebSockets
open System.Collections.Generic

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

open Shared
open ClientIdMessage
open Client

type Startup() =

    let mutable webSocketIdentifier : int = 0
    let clients : List<Client> = List<Client>()
    let random = Random()
    let width = 600.0
    let height = 600.0

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore
        ()

    member this.RunServer(webSocket : WebSocket) : Async<unit> =
        async {
            let id = webSocketIdentifier
            Console.WriteLine("web socket accepted. Id = " + id.ToString())
            webSocketIdentifier <- webSocketIdentifier + 1

            let newClient : Client = {
                socket = webSocket
                id = id
                posX = random.NextDouble() * width
                posY = random.NextDouble() * height
            }

            webSocket.SendAsync(
                    buffer = new ArraySegment<byte>(ClientIdMessage.create(id).ToByteArray()),
                    messageType = WebSocketMessageType.Binary, 
                    endOfMessage = true, 
                    cancellationToken = CancellationToken.None
                ) 
                |> Async.AwaitTask 
                |> ignore

            let tasks = new List<Task>()            

            for client in clients do
                Console.WriteLine("sending info to client " + client.id.ToString())
                
                let msg = 
                    PlayerPositionUpdateMessage.create(
                        newClient.id,
                        newClient.posX,
                        newClient.posY
                    )

                client.socket.SendAsync(
                    buffer = new ArraySegment<byte>(msg.ToByteArray()),
                    messageType = WebSocketMessageType.Binary, 
                    endOfMessage = true, 
                    cancellationToken = CancellationToken.None
                ) 
                |> tasks.Add

            clients.Add(newClient)

            for client in clients do
                let msg = 
                    PlayerPositionUpdateMessage.create(
                        client.id,
                        client.posX,
                        client.posY
                    )

                webSocket.SendAsync(
                        buffer = new ArraySegment<byte>(msg.ToByteArray()),
                        messageType = WebSocketMessageType.Binary, 
                        endOfMessage = true, 
                        cancellationToken = CancellationToken.None
                    ) 
                    |> tasks.Add

            let waitAll = Task.WhenAll(tasks)

            Async.AwaitTask(waitAll) |> ignore

            try
                let buffer = Array.create 2048 (byte 0)
                while true do 
                    let! bytes = 
                        webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None) |>
                        Async.AwaitTask
                    ()
                return ()
            with 
                | ex ->
                    Console.WriteLine("Exception happened (for WebSocket with id ) " + id.ToString() + " in RunServer " + ex.ToString())
                    ()
                            
        }

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
                                    let! result = this.RunServer(websocket)
                                    result
                                }
                                Task.Factory.StartNew(fun () -> Async.RunSynchronously task)
                                
                            else
                                next.Invoke()
                        result
            ) |> ignore
        ) |> ignore