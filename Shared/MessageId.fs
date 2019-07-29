namespace Shared

open ClientIdMessage
open PlayerPositionUpdateMessage
open PlayerDisconnectedMessage
open PlayerMoveMessage
open System

module Message =

    type public ServerMessage =
        | ClientIdMessage of ClientIdMessage
        | PlayerPositionUpdateMessage of PlayerPositionUpdateMessage
        | PlayerDisconnectedMessage of PlayerDisconnectedMessage
        | UnknowMessage

    type public ClientMessage =
        | PlayerMoveMessage of PlayerMoveMessage
        | UnknowMessage

    let parseServerMessage (bytes : byte[]) : ServerMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with 
            | ClientIdMessageId -> ClientIdMessage.parse bytes |> ClientIdMessage
            | PlayerPositionUpdateMessageId -> PlayerPositionUpdateMessage.parse bytes |> PlayerPositionUpdateMessage
            | PlayerDisconnectedMessageId -> PlayerDisconnectedMessage.parse bytes |> PlayerDisconnectedMessage
            | _ -> ServerMessage.UnknowMessage

    let parseClientMessage (bytes : byte[]) : ClientMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with
            | PlayerMoveMessageId -> PlayerMoveMessage.parse bytes |> PlayerMoveMessage 
            | _ -> ClientMessage.UnknowMessage       