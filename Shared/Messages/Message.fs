namespace Shared

open System

module Message =

    type public ServerMessage =
        | ServerMessageNewClientId of ServerMessageNewClientId
        | ServerMessagePlayerTransformUpdate of ServerMessagePlayerTransformUpdate
        | ServerMessagePlayerDisconnected of ServerMessagePlayerDisconnected
        | UnknowMessage

    type public ClientMessage =
        | ClientMessagePlayerTransformUpdate of ClientMessagePlayerTransformUpdate
        | UnknowMessage

    let parseServerMessage (bytes : byte[]) : ServerMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with 
            | MessageIds.ServerMessageNewClientIdId -> ServerMessageNewClientId.Parse bytes |> ServerMessageNewClientId
            | MessageIds.ServerMessagePlayerTransformUpdateId -> ServerMessagePlayerTransformUpdate.Parse bytes |> ServerMessagePlayerTransformUpdate
            | MessageIds.ServerMessagePlayerDisconnectedId -> ServerMessagePlayerDisconnected.Parse bytes |> ServerMessagePlayerDisconnected
            | _ -> ServerMessage.UnknowMessage

    let parseClientMessage (bytes : byte[]) : ClientMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with
            | MessageIds.ClientMessagePlayerTransformUpdateId -> ClientMessagePlayerTransformUpdate.Parse bytes |> ClientMessagePlayerTransformUpdate 
            | _ -> ClientMessage.UnknowMessage       