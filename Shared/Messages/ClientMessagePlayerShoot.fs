namespace Shared

open System
open Utils
open MessageIds

type public ClientMessagePlayerShoot = {
    id : int
    originX : float
    originY : float
    angle : float
} with

    member this.ToByteArray() : byte[] =
        let header = BitConverter.GetBytes(MessageIds.ClientMessagePlayerShootId)
        Array.concat [ 
            header
            BitConverter.GetBytes(this.id)
            BitConverter.GetBytes(this.originX)
            BitConverter.GetBytes(this.originY)
            BitConverter.GetBytes(this.angle)
            // BitConverter.GetBytes(this.directionX)
            // BitConverter.GetBytes(this.directionY)
        ]

    static member public Parse (streamReader: StreamReader) =
        let result : ClientMessagePlayerShoot = {
            id = streamReader.ReadInt32()
            originX = streamReader.ReadDouble()
            originY = streamReader.ReadDouble()
            angle = streamReader.ReadDouble()
            // directionX = streamReader.ReadDouble()
            // directionY = streamReader.ReadDouble()
        } 
        result