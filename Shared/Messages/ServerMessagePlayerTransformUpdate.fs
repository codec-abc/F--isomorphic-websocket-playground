namespace Shared

open System
open Utils
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

    static member public Parse (streamReader: StreamReader) =
        let result : ServerMessagePlayerTransformUpdate = {
            id = streamReader.ReadInt32()
            posX = streamReader.ReadDouble()
            posY = streamReader.ReadDouble()
            orientation = streamReader.ReadDouble()
        } 
        result