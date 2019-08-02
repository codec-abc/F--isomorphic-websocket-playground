namespace Shared

open System
open Utils
open MessageIds

type public ServerMessageNewClientId = {
    id : int
    posX : float
    posY : float
} with

    member this.ToByteArray() : byte[] =

        let header = BitConverter.GetBytes(ServerMessageNewClientIdId)
       
        Array.concat [ 
            header
            BitConverter.GetBytes(this.id)
            BitConverter.GetBytes(this.posX)
            BitConverter.GetBytes(this.posY)
        ]

    static member public Parse (streamReader: StreamReader) =
        let result : ServerMessageNewClientId = {
            id = streamReader.ReadInt32()
            posX = streamReader.ReadDouble()
            posY = streamReader.ReadDouble()
        } 
        result