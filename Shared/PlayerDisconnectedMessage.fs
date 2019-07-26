namespace Shared

open System

module PlayerDisconnectedMessage = 

    [<Literal>]
    let public PlayerDisconnectedMessageId = 2

    type public PlayerDisconnectedMessage = {
        idOfDisconnectedPlayer : int
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(PlayerDisconnectedMessageId)

            Array.concat [ 
                header ; 
                BitConverter.GetBytes(this.idOfDisconnectedPlayer);
            ]

    let public create(id : int) : PlayerDisconnectedMessage =
        { 
            idOfDisconnectedPlayer = id
        }

    let public parse (bytes: byte[]) =
        let result : PlayerDisconnectedMessage = {
            idOfDisconnectedPlayer = BitConverter.ToInt32(bytes, 4)
        } 
        result