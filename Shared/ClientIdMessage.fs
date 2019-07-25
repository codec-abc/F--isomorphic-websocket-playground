namespace Shared

open System

module ClientIdMessage = 

    [<Literal>]
    let public ClientIdMessageId = 0

    type public ClientIdMessage = {
        id : int
    } with
        member this.ToByteArray() : byte[] =

            let header = BitConverter.GetBytes(ClientIdMessageId)
            let content = BitConverter.GetBytes(this.id)

            Array.concat [ 
                header ; 
                content
            ]

    let public create(id : int) : ClientIdMessage =
        { id = id}

    let public parse (bytes: byte[]) =
        let id = BitConverter.ToInt32(bytes, 4)
        let result : ClientIdMessage = {
            id = id
        } 
        result