namespace Shared

open System

module ClientIdMessage = 

    [<Literal>]
    let public ClientIdMessageId = 0

    type public ClientIdMessage = {
        id : int
        posX : float
        posY : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(ClientIdMessageId)
           
            Array.concat [ 
                header
                BitConverter.GetBytes(this.id)
                BitConverter.GetBytes(this.posX)
                BitConverter.GetBytes(this.posY)
            ]

    let public create(id : int, posX : float, posY : float) : ClientIdMessage =
        { 
            id = id
            posX = posX
            posY = posY
        }

    let public parse (bytes: byte[]) =
        let result : ClientIdMessage = {
            id = BitConverter.ToInt32(bytes, 4)
            posX = BitConverter.ToDouble(bytes, 8)
            posY = BitConverter.ToDouble(bytes, 16)
        } 
        result