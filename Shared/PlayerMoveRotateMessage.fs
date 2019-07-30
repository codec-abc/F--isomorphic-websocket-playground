namespace Shared

open System

module PlayerMoveRotateMessage = 

    [<Literal>]
    let public PlayerMoveRotateMessageId = 3

    type public PlayerMoveRotateMessage = {
        id : int
        newPosX : float
        newPosY : float
        orientation : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(PlayerMoveRotateMessageId)

            Array.concat [ 
                header
                BitConverter.GetBytes(this.id)
                BitConverter.GetBytes(this.newPosX)
                BitConverter.GetBytes(this.newPosY)
                BitConverter.GetBytes(this.orientation)
            ]

    let public create(id : int, posX : float, posY : float, orientation : float) : PlayerMoveRotateMessage =
        { 
            id = id
            newPosX = posX
            newPosY = posY
            orientation = orientation
        }

    let public parse (bytes: byte[]) =
        let result : PlayerMoveRotateMessage = {
            id = BitConverter.ToInt32(bytes, 4)
            newPosX = BitConverter.ToDouble(bytes, 8)
            newPosY = BitConverter.ToDouble(bytes, 16)
            orientation = BitConverter.ToDouble(bytes, 24)
        } 
        result