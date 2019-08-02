namespace Utils

open System

type StreamReader(stream : byte[], offset : int) =
    let mutable _stream = stream
    let mutable _initialOffset = offset
    let mutable _currentPos = 0

    member this.ReadBoolean() =
        let result = BitConverter.ToBoolean(_stream, _initialOffset + _currentPos)
        _currentPos <- _currentPos + 1
        result

    member this.ReadChar() =
            let result = BitConverter.ToChar(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 1
            result

    member this.ReadDouble() =
            let result = BitConverter.ToDouble(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 8
            result

    member this.ReadInt16() =
            let result = BitConverter.ToInt16(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 2
            result

    member this.ReadInt32() =
            let result = BitConverter.ToInt32(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 4
            result

    member this.ReadInt64() =
            let result = BitConverter.ToInt64(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 8
            result

    member this.ReadSingle() =
            let result = BitConverter.ToSingle(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 4
            result

    member this.ReadUInt16() =
            let result = BitConverter.ToUInt16(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 2
            result

    member this.ReadUInt32() =
            let result = BitConverter.ToUInt32(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 4
            result

    member this.ReadUInt64() =
            let result = BitConverter.ToUInt64(_stream, _initialOffset + _currentPos)
            _currentPos <- _currentPos + 8
            result



