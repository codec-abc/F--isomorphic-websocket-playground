namespace MathUtil

open System

// TODO : clean and refactor

type Circle = {
    center : Vector2
    radius : float
}
and Line = {
    anchor : Vector2
    direction : Vector2
} with
    member this.Intersect(circle : Circle) =
        let pointB = this.anchor
        let pointA = this.anchor + this.direction
        let center = circle.center
        let radius = circle.radius

        let baX = pointB.X - pointA.X
        let baY = pointB.Y - pointA.Y
        let caX = center.X - pointA.X
        let caY = center.Y - pointA.Y

        let a = baX * baX + baY * baY
        let bBy2 = baX * caX + baY * caY
        let c = caX * caX + caY * caY - radius * radius

        let pBy2 = bBy2 / a
        let q = c / a

        let disc = pBy2 * pBy2 - q

        if disc < 0.0 then
            [||]
        else
            let tmpSqrt = Math.Sqrt(disc)
            let abScalingFactor1 = -pBy2 + tmpSqrt
            let abScalingFactor2 = -pBy2 - tmpSqrt

            let p1 : Vector2 = {
                    X = pointA.X - baX * abScalingFactor1
                    Y = pointA.Y - baY * abScalingFactor1
            }

            if disc = 0.0 then
                [| p1 |]
            else
                let p2 : Vector2 = {
                    X = pointA.X - baX * abScalingFactor2
                    Y = pointA.Y - baY * abScalingFactor2
                }
                
                [|p1; p2|]

and LineSegment = {
    start : Vector2
    stop : Vector2
} with 
    member this.Intersect(circle : Circle) =
        let line : Line = {
            anchor = this.start
            direction = this.stop - this.start
        }

        let intersections = line.Intersect(circle)

        intersections |> 
            Array.where (fun point -> 
                let k = 
                    if Math.Abs(line.direction.X) > Math.Abs(line.direction.Y) then
                        (point.X - line.anchor.X) / line.direction.X
                    else
                        (point.Y - line.anchor.Y) / line.direction.Y
                k >= 0.0 && k <= 1.0
            ) |>
            Array.sortWith (fun point1 point2 ->
                let d1 = (point1 - this.start).Magnitude()
                let d2 = (point2 - this.start).Magnitude()
                if d1 = d2 then
                    0
                else if d1 > d2 then
                    1
                else
                    -1   
            )                

and HalfOpenLineSegment = {
    start : Vector2
    direction : Vector2
} with
    member this.Intersect(circle : Circle) =
        let line : Line = {
            anchor = this.start
            direction = this.direction
        }

        let intersections = line.Intersect(circle)

        intersections |> 
            Array.where (fun point -> 
                let k = 
                    if Math.Abs(this.direction.X) > Math.Abs(this.direction.Y) then
                        (point.X - this.start.X) / this.direction.X
                    else
                        (point.Y - this.start.Y) / this.direction.Y
                k >= 0.0
            ) |>
            Array.sortWith (fun point1 point2 ->
                let d1 = (point1 - this.start).Magnitude()
                let d2 = (point2 - this.start).Magnitude()
                if d1 = d2 then
                    0
                else if d1 > d2 then
                    1
                else
                    -1                                                     
            )