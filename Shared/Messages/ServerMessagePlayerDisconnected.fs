namespace Shared

open System
open MessageIds

type public ServerMessagePlayerDisconnected = {
    idOfDisconnectedPlayer : int
} with

    member this.ToByteArray() : byte[] =

        let header = BitConverter.GetBytes(ServerMessagePlayerDisconnectedId)

        Array.concat [ 
            header ; 
            BitConverter.GetBytes(this.idOfDisconnectedPlayer);
        ]

    static member public Parse (bytes: byte[]) =
        let result : ServerMessagePlayerDisconnected = {
            idOfDisconnectedPlayer = BitConverter.ToInt32(bytes, 4)
        } 
        result