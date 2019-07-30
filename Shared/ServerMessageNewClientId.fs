namespace Shared

open System

module ServerMessageNewClientId = 

    [<Literal>]
    let public ServerMessageNewClientIdId = 0

    type public ServerMessageNewClientId = {
        id : int
        posX : float
        posY : float
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(ServerMessageNewClientIdId)
           
            Array.concat [ 
                header
                BitConverter.GetBytes(this.id)
                BitConverter.GetBytes(this.posX)
                BitConverter.GetBytes(this.posY)
            ]

    let public create(id : int, posX : float, posY : float) : ServerMessageNewClientId =
        { 
            id = id
            posX = posX
            posY = posY
        }

    let public parse (bytes: byte[]) =
        let result : ServerMessageNewClientId = {
            id = BitConverter.ToInt32(bytes, 4)
            posX = BitConverter.ToDouble(bytes, 8)
            posY = BitConverter.ToDouble(bytes, 16)
        } 
        result