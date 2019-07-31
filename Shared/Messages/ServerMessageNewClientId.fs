namespace Shared

open System
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

    static member public Parse (bytes: byte[]) =
        let result : ServerMessageNewClientId = {
            id = BitConverter.ToInt32(bytes, 4)
            posX = BitConverter.ToDouble(bytes, 8)
            posY = BitConverter.ToDouble(bytes, 16)
        } 
        result