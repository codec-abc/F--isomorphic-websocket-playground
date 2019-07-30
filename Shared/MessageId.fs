namespace Shared

open ServerMessageNewClientId
open ServerMessagePlayerTransformUpdate
open ServerMessagePlayerDisconnected
open ClientMessagePlayerTransformUpdate
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
            | ServerMessageNewClientIdId -> ServerMessageNewClientId.parse bytes |> ServerMessageNewClientId
            | ServerMessagePlayerTransformUpdateId -> ServerMessagePlayerTransformUpdate.parse bytes |> ServerMessagePlayerTransformUpdate
            | ServerMessagePlayerDisconnectedId -> ServerMessagePlayerDisconnected.parse bytes |> ServerMessagePlayerDisconnected
            | _ -> ServerMessage.UnknowMessage

    let parseClientMessage (bytes : byte[]) : ClientMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with
            | ClientMessagePlayerTransformUpdateId -> ClientMessagePlayerTransformUpdate.parse bytes |> ClientMessagePlayerTransformUpdate 
            | _ -> ClientMessage.UnknowMessage       