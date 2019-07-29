namespace Shared

open ClientIdMessage
open PlayerPositionUpdateMessage
open PlayerDisconnectedMessage
open PlayerMoveMessage
open System

module Message =

    type public Message =
        | ClientIdMessage of ClientIdMessage
        | PlayerPositionUpdateMessage of PlayerPositionUpdateMessage
        | PlayerDisconnectedMessage of PlayerDisconnectedMessage
        | PlayerMoveMessage of PlayerMoveMessage
        | UnknowMessage

    let parse (bytes : byte[]) : Message =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with 
            | ClientIdMessageId -> ClientIdMessage.parse bytes |> ClientIdMessage
            | PlayerPositionUpdateMessageId -> PlayerPositionUpdateMessage.parse bytes |> PlayerPositionUpdateMessage
            | PlayerDisconnectedMessageId -> PlayerDisconnectedMessage.parse bytes |> PlayerDisconnectedMessage
            | PlayerMoveMessageId -> PlayerMoveMessage.parse bytes |> PlayerMoveMessage
            | _ -> UnknowMessage