namespace Shared

open System
open ServerMessageNewClientId
open ServerMessagePlayerTransformUpdate
open ServerMessagePlayerDisconnected
open ClientMessagePlayerTransformUpdate

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
            | MessageIds.ServerMessageNewClientIdId -> ServerMessageNewClientId.parse bytes |> ServerMessageNewClientId
            | MessageIds.ServerMessagePlayerTransformUpdateId -> ServerMessagePlayerTransformUpdate.parse bytes |> ServerMessagePlayerTransformUpdate
            | MessageIds.ServerMessagePlayerDisconnectedId -> ServerMessagePlayerDisconnected.parse bytes |> ServerMessagePlayerDisconnected
            | _ -> ServerMessage.UnknowMessage

    let parseClientMessage (bytes : byte[]) : ClientMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with
            | MessageIds.ClientMessagePlayerTransformUpdateId -> ClientMessagePlayerTransformUpdate.parse bytes |> ClientMessagePlayerTransformUpdate 
            | _ -> ClientMessage.UnknowMessage       