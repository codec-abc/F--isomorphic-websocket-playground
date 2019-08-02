namespace Shared

open System
open Utils

module Message =

    type public ServerMessage =
        | ServerMessageNewClientId of ServerMessageNewClientId
        | ServerMessagePlayerTransformUpdate of ServerMessagePlayerTransformUpdate
        | ServerMessagePlayerDisconnected of ServerMessagePlayerDisconnected
        | UnknowMessage

    type public ClientMessage =
        | ClientMessagePlayerTransformUpdate of ClientMessagePlayerTransformUpdate
        | ClientMessagePlayerShoot of ClientMessagePlayerShoot
        | UnknowMessage

    let parseServerMessage (bytes : byte[]) : ServerMessage =
        let initialOffset = 0
        let streamReader = StreamReader(bytes, initialOffset)
        let messageId = streamReader.ReadInt32()
        match messageId with 
            | MessageIds.ServerMessageNewClientIdId -> 
                ServerMessageNewClientId.Parse streamReader |> ServerMessageNewClientId

            | MessageIds.ServerMessagePlayerTransformUpdateId -> 
                ServerMessagePlayerTransformUpdate.Parse streamReader |> ServerMessagePlayerTransformUpdate

            | MessageIds.ServerMessagePlayerDisconnectedId -> 
                ServerMessagePlayerDisconnected.Parse streamReader |> ServerMessagePlayerDisconnected
            | _ -> 
                ServerMessage.UnknowMessage

    let parseClientMessage (bytes : byte[]) : ClientMessage =
        let initialOffset = 0
        let streamReader = StreamReader(bytes, initialOffset)
        let messageId = streamReader.ReadInt32()
        match messageId with
            | MessageIds.ClientMessagePlayerTransformUpdateId -> 
                ClientMessagePlayerTransformUpdate.Parse streamReader |> ClientMessagePlayerTransformUpdate

            | MessageIds.ClientMessagePlayerShootId -> 
                ClientMessagePlayerShoot.Parse streamReader |> ClientMessagePlayerShoot

            | _ -> ClientMessage.UnknowMessage