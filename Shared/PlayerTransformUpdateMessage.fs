namespace Shared

open System

module PlayerTransformUpdateMessage = 

    [<Literal>]
    let public PlayerTransformUpdateMessageId = 1

    type public PlayerTransformUpdateMessage = {
        id : int
        posX : float
        posY : float
        orientation : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(PlayerTransformUpdateMessageId)

            Array.concat [ 
                header
                BitConverter.GetBytes(this.id)
                BitConverter.GetBytes(this.posX)
                BitConverter.GetBytes(this.posY)
                BitConverter.GetBytes(this.orientation)
            ]

    let public create(id : int, posX : float, posY : float, orientation : float) : PlayerTransformUpdateMessage =
        { 
            id = id
            posX = posX
            posY = posY
            orientation = orientation
        }

    let public parse (bytes: byte[]) =
        let result : PlayerTransformUpdateMessage = {
            id = BitConverter.ToInt32(bytes, 4)
            posX = BitConverter.ToDouble(bytes, 8)
            posY = BitConverter.ToDouble(bytes, 16)
            orientation = BitConverter.ToDouble(bytes, 24)
        } 
        result