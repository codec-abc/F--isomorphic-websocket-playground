module Graphics

open System
open Fable.Pixi
open Fable.Pixi.PIXI
open Fable.Core
open Fable.Core.JsInterop

let weaponIndexRocket = 4;
let weaponIndexSniper = 11;

let playerSpriteBody = pixi.Sprite.from("top-down-shooter/characters/body/3.png")
let playerSpriteHead = pixi.Sprite.from("top-down-shooter/characters/head/2.png")

let ennemySpriteBody = pixi.Sprite.from("top-down-shooter/characters/body/4.png")
let ennemySpriteHead = pixi.Sprite.from("top-down-shooter/characters/head/3.png")

let weaponTopSniper = 
    pixi.Sprite.from("top-down-shooter/weapons/attach-to-body/" + weaponIndexSniper.ToString() + ".png")

let createPlayer(textureHead, textureBody) =
    let playerContainer = pixi.Container.Create()
    let weapon = pixi.Sprite.Create(weaponTopSniper.texture)
    let spriteHead = pixi.Sprite.Create(textureHead)
    let spriteBody = pixi.Sprite.Create(textureBody)
    spriteBody.x <- -11.0
    spriteBody.y <- -7.0
    spriteHead.x <- -11.0 + 3.0
    spriteHead.y <- -7.0 - 5.0

    playerContainer.addChild(weapon) |> ignore
    playerContainer.addChild(spriteBody) |> ignore
    playerContainer.addChild(spriteHead) |> ignore
    playerContainer

let createPlayerContainer () = 
    createPlayer(playerSpriteHead.texture, playerSpriteBody.texture)

let createEnnemyContainer () = 
    createPlayer(ennemySpriteHead.texture, ennemySpriteBody.texture)