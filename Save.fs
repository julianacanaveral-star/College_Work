
module App.Save



open System.IO
open FSharp.Json
open App.Game


let private jsonConfiguration = JsonConfig.Default


let saveGame (state: Game.State) =
    let json = Json.serializeEx jsonConfiguration state
    File.WriteAllText("save.txt",json)

let loadGame () : Game.State option =
    if File.Exists("save.txt") then
        try
            let json = File.ReadAllText("save.txt")
            Some (Json.deserialize<Game.State> json)
        with
        | _ -> None
    else
        None

let deleteGame () =
    if File.Exists("save.txt") then
        File.Delete("save.txt")