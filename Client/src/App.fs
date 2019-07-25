module App

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Browser
open Browser.WebSocket

open Shared

let window = Browser.Dom.window

// Get our canvas context 
// As we'll see later, myCanvas is mutable hence the use of the mutable keyword
// the unbox keyword allows to make an unsafe cast. Here we assume that getElementById will return an HTMLCanvasElement 

let mutable myCanvas : Browser.Types.HTMLCanvasElement = unbox window.document.getElementById "myCanvas"  // myCanvas is defined in public/index.html

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

socket.addEventListener_message(
    fun a ->
        let blob : Browser.Blob = a?data
        let fileReader : Browser.FileReader = FileReader.Create()
        
        fileReader.onload <- fun a ->
            let mutable arrayBuffer = createArrayBuffer(0)
            arrayBuffer <- a?target?result
            let dv = createDataView arrayBuffer
            //let integer : int = dv?getInt32(0, isLittleEndian)
            let length : int = dv?byteLength
            let array = Array.create length (byte 0)
            for i in 0..length-1 do
                let byte = dv?getUint8(i)
                array.[i] <- byte
            let message = Message.parse array
            console.log(message.ToString())

        fileReader.readAsArrayBuffer(blob)
        ()
)

// Get the context
let ctx = myCanvas.getContext_2d()

// All these are immutables values
let w = myCanvas.width
let h = myCanvas.height
let steps = 20
let squareSize = 20

// gridWidth needs a float wo we cast tour int operation to a float using the float keyword
let gridWidth = float (steps * squareSize) 

// resize our canvas to the size of our grid
// the arrow <- indicates we're mutating a value. It's a special operator in F#.
myCanvas.width <- gridWidth
myCanvas.height <- gridWidth

// print the grid size to our debugger consoloe
printfn "%i" steps

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


