namespace Shared

open System
open MessageIds

type public ClientMessagePlayerTransformUpdate = {
    id : int
    newPosX : float
    newPosY : float
    orientation : float
} with
    member this.ToByteArray() : byte[] =

        let header = BitConverter.GetBytes(MessageIds.ClientMessagePlayerTransformUpdateId)
        Array.concat [ 
            header
            BitConverter.GetBytes(this.id)
            BitConverter.GetBytes(this.newPosX)
            BitConverter.GetBytes(this.newPosY)
            BitConverter.GetBytes(this.orientation)
        ]
        
    static member public Parse (bytes: byte[]) =
        let result : ClientMessagePlayerTransformUpdate = {
            id = BitConverter.ToInt32(bytes, 4)
            newPosX = BitConverter.ToDouble(bytes, 8)
            newPosY = BitConverter.ToDouble(bytes, 16)
            orientation = BitConverter.ToDouble(bytes, 24)
        } 
        result