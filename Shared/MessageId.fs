namespace Shared

open ClientIdMessage
open PlayerPositionUpdateMessage
open System

module Message =

    type public Message =
        | ClientIdMessage of ClientIdMessage
        | PlayerPositionUpdateMessage of PlayerPositionUpdateMessage
        | UnknowMessage

    let parse (bytes : byte[]) : Message =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with 
            | ClientIdMessageId -> ClientIdMessage.parse bytes |> ClientIdMessage
            | PlayerPositionUpdateMessageId -> PlayerPositionUpdateMessage.parse bytes |> PlayerPositionUpdateMessage
            | _ -> UnknowMessage