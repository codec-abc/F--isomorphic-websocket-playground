namespace Shared

open System
open Utils
open MessageIds

type public ClientMessagePlayerTransformUpdate = {
    id : int
    newPosX : float
    newPosY : float
    orientation : float
} with
    member this.ToByteArray() : byte[] =

        let header = BitConverter.GetBytes(MessageIds.ClientMessagePlayerTransformUpdateId)
        Array.concat [ 
            header
            BitConverter.GetBytes(this.id)
            BitConverter.GetBytes(this.newPosX)
            BitConverter.GetBytes(this.newPosY)
            BitConverter.GetBytes(this.orientation)
        ]
        
    static member public Parse (streamReader: StreamReader) =
        let result : ClientMessagePlayerTransformUpdate = {
            id = streamReader.ReadInt32()
            newPosX = streamReader.ReadDouble()
            newPosY = streamReader.ReadDouble()
            orientation = streamReader.ReadDouble()
        } 
        result