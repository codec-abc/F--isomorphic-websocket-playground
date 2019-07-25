namespace Shared

open System

module PlayerPositionUpdateMessage = 

    [<Literal>]
    let public PlayerPositionUpdateMessageId = 1

    type public PlayerPositionUpdateMessage = {
        id : int
        posX : float
        posY : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(PlayerPositionUpdateMessageId)

            Array.concat [ 
                header ; 
                BitConverter.GetBytes(this.id);
                BitConverter.GetBytes(this.posX);
                BitConverter.GetBytes(this.posY);
            ]

    let public create(id : int, posX : float, posY : float) : PlayerPositionUpdateMessage =
        { 
            id = id
            posX = posX
            posY = posY
        }

    let public parse (bytes: byte[]) =
        let result : PlayerPositionUpdateMessage = {
            id = BitConverter.ToInt32(bytes, 4)
            posX = BitConverter.ToDouble(bytes, 8)
            posY = BitConverter.ToDouble(bytes, 16)
        } 
        result