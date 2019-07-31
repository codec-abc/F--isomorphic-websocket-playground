namespace MathUtil

open System

type Vector2 = {
    mutable X : float
    mutable Y : float
} with
    member public this.Magnitude() =
        Math.Sqrt(this.X * this.X + this.Y * this.Y)

    member public this.Normalized() =
        this * (1.0 / this.Magnitude())

    member public this.Normalize() =
        let normalized = this.Normalized()
        this.X <- normalized.X
        this.Y <- normalized.Y

    static member public (+) (a : Vector2, other : Vector2) =
        {
            X = a.X + other.X
            Y = a.Y + other.Y
        }

    static member public (-) (a : Vector2, other : Vector2) =
        {
            X = a.X - other.X
            Y = a.Y - other.Y
        }    

    static member public (*) (a : Vector2, scale : float) =
        {
            X = a.X * scale
            Y = a.Y * scale
        }

    static member public (*) (a : Vector2, b : Vector2) =
        a.X * b.Y + a.Y * b.X   