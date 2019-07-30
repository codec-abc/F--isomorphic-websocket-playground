namespace Shared

open ClientIdMessage
open PlayerTransformUpdateMessage
open PlayerDisconnectedMessage
open PlayerMoveRotateMessage
open System

module Message =

    type public ServerMessage =
        | ClientIdMessage of ClientIdMessage
        | PlayerTransformUpdateMessage of PlayerTransformUpdateMessage
        | PlayerDisconnectedMessage of PlayerDisconnectedMessage
        | UnknowMessage

    type public ClientMessage =
        | PlayerMoveRotateMessage of PlayerMoveRotateMessage
        | UnknowMessage

    let parseServerMessage (bytes : byte[]) : ServerMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with 
            | ClientIdMessageId -> ClientIdMessage.parse bytes |> ClientIdMessage
            | PlayerTransformUpdateMessageId -> PlayerTransformUpdateMessage.parse bytes |> PlayerTransformUpdateMessage
            | PlayerDisconnectedMessageId -> PlayerDisconnectedMessage.parse bytes |> PlayerDisconnectedMessage
            | _ -> ServerMessage.UnknowMessage

    let parseClientMessage (bytes : byte[]) : ClientMessage =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with
            | PlayerMoveRotateMessageId -> PlayerMoveRotateMessage.parse bytes |> PlayerMoveRotateMessage 
            | _ -> ClientMessage.UnknowMessage       