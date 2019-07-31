namespace Shared

open System
open MessageIds

type public ServerMessagePlayerTransformUpdate = {
    id : int
    posX : float
    posY : float
    orientation : float
} with

    member this.ToByteArray() : byte[] =
        let header = BitConverter.GetBytes(ServerMessagePlayerTransformUpdateId)
        Array.concat [ 
            header
            BitConverter.GetBytes(this.id)
            BitConverter.GetBytes(this.posX)
            BitConverter.GetBytes(this.posY)
            BitConverter.GetBytes(this.orientation)
        ]

    static member public Parse (bytes: byte[]) =
        let result : ServerMessagePlayerTransformUpdate = {
            id = BitConverter.ToInt32(bytes, 4)
            posX = BitConverter.ToDouble(bytes, 8)
            posY = BitConverter.ToDouble(bytes, 16)
            orientation = BitConverter.ToDouble(bytes, 24)
        } 
        result