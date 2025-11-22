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
}

let initState() =
    {
        NavigatorState = ShowMainMenu
        GameState = None
    }


let showMainMenu state =
    Console.Clear()
    Utils.displayMessageGigante 0 0 ConsoleColor.Green "Alien"
    Utils.displayMessageGigante 0 7 ConsoleColor.Red "Attack!"
    match MainMenu.mostrarMenu 20 15 with
    | GameCommand.NewGame ->
        {state with NavigatorState = ShowGame; GameState = None}
    | GameCommand.LoadGame ->
        //
        // La magia ocurre aqui para cargar
        // un juego grabado en disco
        //
        {state with NavigatorState=ShowGame}
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
        {state with NavigatorState = ShowGameOver; GameState = Some updatedGameState}



let showGameOver state =
    Console.Clear()
    Utils.displayMessageGigante 0 5 ConsoleColor.Red "Game Over"
    match GameOver.mostrarMenu 10 15 with
    | GameOverCommand.NewGame ->
        { state with NavigatorState = ShowGame; GameState = None }
    | GameOverCommand.Exit ->
        { state with NavigatorState=Terminated}


let showPause state =
    Console.Clear()
    Utils.displayMessageGigante 0 5 ConsoleColor.Magenta "Paused"
    match PauseMenu.mostrarMenu 10 15 with
    | PauseCommand.ContinueGame ->
    let restoredGameState =
        state.GameState
        |> Option.map (fun gs -> { gs with ProgramState = Game.Running })
    { state with NavigatorState = ShowGame; GameState = restoredGameState }
    | PauseCommand.SaveGame ->
        //
        // La magia ocurre aqui tambien
        //
        {state with NavigatorState=Terminated}
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
