namespace Shared

open System
open Utils
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

    static member public Parse (streamReader: StreamReader) =
        let result : ServerMessagePlayerDisconnected = {
            idOfDisconnectedPlayer = streamReader.ReadInt32()
        } 
        result