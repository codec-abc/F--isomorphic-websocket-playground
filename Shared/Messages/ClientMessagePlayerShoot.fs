namespace Shared

open System
open MessageIds

type public ClientMessagePlayerShoot = {
    id : int
    originX : float
    originY : float
    directionX : float
    directionY : float
} with

    member this.ToByteArray() : byte[] =
        let header = BitConverter.GetBytes(MessageIds.ClientMessagePlayerShootId)
        Array.concat [ 
            header
            BitConverter.GetBytes(this.id)
            BitConverter.GetBytes(this.originX)
            BitConverter.GetBytes(this.originY)
            BitConverter.GetBytes(this.directionX)
            BitConverter.GetBytes(this.directionY)
        ]

    static member public Parse (bytes: byte[]) =
        let result : ClientMessagePlayerShoot = {
            id = BitConverter.ToInt32(bytes, 4)
            originX = BitConverter.ToDouble(bytes, 8)
            originY = BitConverter.ToDouble(bytes, 16)
            directionX = BitConverter.ToDouble(bytes, 24)
            directionY = BitConverter.ToDouble(bytes, 32)
        } 
        result