namespace Shared

open System

module PlayerMoveMessage = 

    [<Literal>]
    let public PlayerMoveMessageId = 3

    type public PlayerMoveMessage = {
        id : int
        newPosX : float
        newPosY : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(PlayerMoveMessageId)

            Array.concat [ 
                header
                BitConverter.GetBytes(this.id)
                BitConverter.GetBytes(this.newPosX)
                BitConverter.GetBytes(this.newPosY)
            ]

    let public create(id : int, posX : float, posY : float) : PlayerMoveMessage =
        { 
            id = id
            newPosX = posX
            newPosY = posY
        }

    let public parse (bytes: byte[]) =
        let result : PlayerMoveMessage = {
            id = BitConverter.ToInt32(bytes, 4)
            newPosX = BitConverter.ToDouble(bytes, 8)
            newPosY = BitConverter.ToDouble(bytes, 16)
        } 
        result