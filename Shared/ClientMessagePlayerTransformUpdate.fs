namespace Shared

open System

module ClientMessagePlayerTransformUpdate = 

    [<Literal>]
    let public ClientMessagePlayerTransformUpdateId = 3

    type public ClientMessagePlayerTransformUpdate = {
        id : int
        newPosX : float
        newPosY : float
        orientation : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(ClientMessagePlayerTransformUpdateId)

            Array.concat [ 
                header
                BitConverter.GetBytes(this.id)
                BitConverter.GetBytes(this.newPosX)
                BitConverter.GetBytes(this.newPosY)
                BitConverter.GetBytes(this.orientation)
            ]

    let public create(id : int, posX : float, posY : float, orientation : float) : ClientMessagePlayerTransformUpdate =
        { 
            id = id
            newPosX = posX
            newPosY = posY
            orientation = orientation
        }

    let public parse (bytes: byte[]) =
        let result : ClientMessagePlayerTransformUpdate = {
            id = BitConverter.ToInt32(bytes, 4)
            newPosX = BitConverter.ToDouble(bytes, 8)
            newPosY = BitConverter.ToDouble(bytes, 16)
            orientation = BitConverter.ToDouble(bytes, 24)
        } 
        result