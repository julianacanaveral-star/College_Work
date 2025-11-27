module App.Navigator

open System
open App.Types

type NavigatorState =
| ShowMainMenu
| ShowGame
| ShowPause
| ShowGameOver
| Terminated

type State = {
    NavigatorState: NavigatorState
    GameState: Game.State option
    ScreenX: int
    ScreenY: int
}

let initState() =
    {
        NavigatorState = ShowMainMenu
        GameState = None
        ScreenX= Console.BufferWidth 
        ScreenY= Console.BufferHeight
    }


let showMainMenu state =
    Console.Clear()
    Utils.displayMessageGigante (state.ScreenX/2-18) (state.ScreenY/2-10) ConsoleColor.Green "Alien"
    Utils.displayMessageGigante (state.ScreenX/2-22) (state.ScreenY/2-3) ConsoleColor.Red "Attack!"
    match MainMenu.mostrarMenu (state.ScreenX/2-5) (state.ScreenY/2+5) with
    | GameCommand.NewGame ->
        {state with NavigatorState = ShowGame; GameState = None}
    | GameCommand.LoadGame ->
        match Save.loadGame() with
        | Some savedState ->
            let restoredState = { savedState with ProgramState = Game.Running }
            { state with NavigatorState = ShowGame; GameState = Some restoredState }
        | None ->
            { state with NavigatorState = ShowGame; GameState = Some (Game.initState()) }
    | GameCommand.Exit ->
        {state with NavigatorState=Terminated}

let showGame state =
    Console.Clear()
    let gameState =
        match state.GameState with
        | Some s -> s
        | None -> Game.initState()
    let status, updatedGameState = Game.mostrarJuego gameState
    match status with
    | GameStatus.Paused ->
        {state with NavigatorState = ShowPause; GameState = Some updatedGameState}
    | GameStatus.GameOver ->
        Save.deleteGame() 
        {state with NavigatorState = ShowGameOver; GameState = Some updatedGameState}



let showGameOver state =
    Console.Clear()
    Utils.displayMessageGigante (state.ScreenX/2-30) (state.ScreenY/2-10) ConsoleColor.Red "Game Over"
    match GameOver.mostrarMenu (state.ScreenX/2-5) (state.ScreenY/2+2) with
    | GameOverCommand.NewGame ->
        { state with NavigatorState = ShowGame; GameState = None }
    | GameOverCommand.Exit ->
        { state with NavigatorState=Terminated}


let showPause state =
    Console.Clear()
    Utils.displayMessageGigante (state.ScreenX/2-18) (state.ScreenY/2-10) ConsoleColor.Magenta "Paused"
    match PauseMenu.mostrarMenu (state.ScreenX/2-5) (state.ScreenY/2+2) with
    | PauseCommand.ContinueGame ->
    let restoredGameState =
        state.GameState
        |> Option.map (fun gs -> { gs with ProgramState = Game.Running })
    { state with NavigatorState = ShowGame; GameState = restoredGameState }
    | PauseCommand.SaveGame ->
        match state.GameState with
        | Some gs -> Save.saveGame gs
        | None -> ()
    {state with NavigatorState = ShowPause}
    | PauseCommand.Exit ->
        {state with NavigatorState=Terminated}



let updateState state =
    match state.NavigatorState with
    | ShowMainMenu -> showMainMenu state
    | ShowGame -> showGame state
    | ShowPause -> showPause state
    | ShowGameOver -> showGameOver state
    | _ -> state


let rec mainLoop state =
    let newState = state |> updateState
    if newState.NavigatorState <> Terminated then
        mainLoop newState

let mostrarNavegador() =
    initState()
    |> mainLoop
