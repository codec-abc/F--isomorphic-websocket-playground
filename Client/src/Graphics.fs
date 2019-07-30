module Graphics

open System
open Fable.Pixi
open Fable.Pixi.PIXI
open Fable.Core
open Fable.Core.JsInterop

let playerSpriteBody = pixi.Sprite.from("top-down-shooter/characters/body/3.png")
let playerSpriteHead = pixi.Sprite.from("top-down-shooter/characters/head/2.png")

let ennemySpriteBody = pixi.Sprite.from("top-down-shooter/characters/body/1.png")
let ennemySpriteHead = pixi.Sprite.from("top-down-shooter/characters/head/1.png")

let createPlayerContainer () = 
    let playerContainer = pixi.Container.Create()
    let spriteHead = pixi.Sprite.Create(playerSpriteHead.texture)
    let spriteBody = pixi.Sprite.Create(playerSpriteBody.texture)
    spriteBody.x <- -11.0
    spriteBody.y <- -7.0
    spriteHead.x <- -11.0 + 3.0
    spriteHead.y <- -7.0 - 5.0
    playerContainer.addChild(spriteBody) |> ignore
    playerContainer.addChild(spriteHead) |> ignore
    playerContainer

let createEnnemyContainer () = 
    let ennemyContainer = pixi.Container.Create()
    let spriteHead = pixi.Sprite.Create(ennemySpriteHead.texture)
    let spriteBody = pixi.Sprite.Create(ennemySpriteBody.texture)
    spriteBody.x <- -11.0
    spriteBody.y <- -7.0
    spriteHead.x <- -11.0 + 3.0
    spriteHead.y <- -7.0 - 5.0
    ennemyContainer.addChild(spriteBody) |> ignore
    ennemyContainer.addChild(spriteHead) |> ignore
    ennemyContainer