namespace Shared

open ClientIdMessage
open System

module Message =

    type Message =
        | ClientIdMessage of ClientIdMessage
        | None

    let parse (bytes : byte[]) : Message =
        let messageId = BitConverter.ToInt32(bytes, 0)
        match messageId with 
            | ClientIdMessageId -> ClientIdMessage.parse bytes |> ClientIdMessage
            | _ -> None