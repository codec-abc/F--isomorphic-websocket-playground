namespace Server

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Threading
open System.Threading.Tasks

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore
        ()

    member this.RunServer(webSocket : Net.WebSockets.WebSocket) : Async<unit> =
        async {
            let buffer = Array.create 2048 (byte 0)

            // Console.WriteLine("web socket accepted")
            let task = 
                webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None) |>
                Async.AwaitTask

            return ()
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