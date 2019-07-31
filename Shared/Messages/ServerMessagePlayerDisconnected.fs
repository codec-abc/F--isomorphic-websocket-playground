namespace Shared

open System
open MessageIds

module ServerMessagePlayerDisconnected = 

    type public ServerMessagePlayerDisconnected = {
        idOfDisconnectedPlayer : int
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(ServerMessagePlayerDisconnectedId)

            Array.concat [ 
                header ; 
                BitConverter.GetBytes(this.idOfDisconnectedPlayer);
            ]

    let public create(id : int) : ServerMessagePlayerDisconnected =
        { 
            idOfDisconnectedPlayer = id
        }

    let public parse (bytes: byte[]) =
        let result : ServerMessagePlayerDisconnected = {
            idOfDisconnectedPlayer = BitConverter.ToInt32(bytes, 4)
        } 
        result