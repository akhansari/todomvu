[<RequireQualifiedAccess>]
module TodoMvu.App

open System
open Elmish
open Bolero
open Bolero.Html

type Model =
    { NewTask: string
      Tasks: Task.Model list
      Filter: Task.State option
      FilteredTasks: Map<Task.State, Task.Model list> }

module Model =

    let empty =
        { NewTask = String.Empty
          Tasks = List.empty
          Filter = None
          FilteredTasks = Map.empty.Add(Task.Active, []).Add(Task.Completed, []) }

type Message =
    | SetNew of string
    | Add
    | Remove of Task.Model
    | ToggleStates
    | ClearCompleted
    | SetFilter of Task.State option
    | WrapTask of Task.Model * Task.Message

let private updateTasks model tasks =
    { model with
        Tasks = tasks
        FilteredTasks = Map.empty
            .Add(Task.Active, tasks |> List.filter (fun t -> t.State = Task.Active))
            .Add(Task.Completed, tasks |> List.filter (fun t -> t.State = Task.Completed)) }

let update message model =
    match message with
    | SetNew title ->
        { model with NewTask = title }
        , Cmd.none
    | Add ->
        Task.Model.addTo model.Tasks model.NewTask
        |> updateTasks { model with NewTask = String.Empty }
        , Cmd.none
    | Remove task ->
        Task.Model.removeFrom model.Tasks task
        |> updateTasks model
        , Cmd.none
    | ToggleStates ->
        if model.FilteredTasks.[Task.Active].IsEmpty then Task.Active else Task.Completed
        |> Task.Model.setState model.Tasks
        |> updateTasks model
        , Cmd.none
    | ClearCompleted ->
        Task.Model.cleanState model.Tasks Task.Completed
        |> updateTasks model
        , Cmd.none
    | SetFilter filter ->
        { model with Filter = filter }
        , Cmd.none
    | WrapTask (task, taskMsg) ->
        let task, taskCmd = Task.update taskMsg task
        Task.Model.updateIn model.Tasks task
        |> updateTasks model
        , Cmd.batch [
            Cmd.map (fun m -> WrapTask (task, m)) taskCmd
            if taskMsg = Task.Destroy then Cmd.ofMsg (Remove task)
        ]

let headerView newTask dispatch =
    header [ attr.``class`` "header" ] [
        h1 [] [ text "todos" ]
        input [
            on.keydown (fun e -> if e.Key = "Enter" then dispatch Add)
            bind.input.string newTask (SetNew >> dispatch)
            attr.``class`` "new-todo"
            attr.placeholder "What needs to be done?"
            attr.autocomplete false
            attr.autofocus true
        ]
    ]

let mainView tasks noActiveState dispatch =
    section [ attr.``class`` "main" ] [
        input
            [ bind.``checked`` noActiveState (fun _ -> dispatch ToggleStates)
              attr.id "toggle-all"
              attr.``class`` "toggle-all"
              attr.``type`` "checkbox" ]
        label [ attr.``for`` "toggle-all" ] [ text "Mark all as complete" ]
        ul [ attr.``class`` "todo-list" ] [
            forEach tasks <| fun task ->
                Task.view task (fun m -> WrapTask (task, m) |> dispatch)
        ]
    ]

let footerView remaining canClear filter dispatch =
    footer [ attr.``class`` "footer" ] [
        span
            [ attr.``class`` "todo-count" ]
            [ strong [] [ text (string remaining) ]
              text $""" item{if remaining > 1 then "s" else ""} left""" ]
        ul [ attr.``class`` "filters" ] [
            li [] [ a
                [ on.click (fun _ -> None |> SetFilter |> dispatch)
                  attr.classes [ if filter = None then "selected" ] ]
                [ text "All" ]
            ]
            li [] [ a
                [ on.click (fun _ -> Some Task.Active |> SetFilter |> dispatch)
                  attr.classes [ if filter = Some Task.Active then "selected" ] ]
                [ text "Active" ]
            ]
            li [] [ a
                [ on.click (fun _ -> Some Task.Completed |> SetFilter |> dispatch)
                  attr.classes [ if filter = Some Task.Completed then "selected" ] ]
                [ text "Completed" ]
            ]
        ]
        button
            [ on.click (fun _ -> dispatch ClearCompleted)
              attr.classes [ "clear-completed"; if not canClear then "hidden" ] ]
            [ text "Clear completed" ]
    ]

let view model dispatch =
    section [ attr.``class`` "todoapp" ] [
        headerView model.NewTask dispatch
        cond model.Tasks.IsEmpty <| function
        | true -> Empty
        | false ->
            let tasks =
                match model.Filter with
                | Some state -> model.FilteredTasks.[state]
                | None       -> model.Tasks
            let remaining =
                model.FilteredTasks.[Task.Active].Length
            let canClear =
                model.FilteredTasks.[Task.Completed].Length > 0
            let noActiveState =
                model.FilteredTasks.[Task.Active].IsEmpty
            concat [
                mainView tasks noActiveState dispatch
                footerView remaining canClear model.Filter dispatch
            ]
    ]

type Component () =
    inherit ProgramComponent<Model, Message> ()
    override this.Program =
        Program.mkProgram
            (fun _ -> Model.empty, Cmd.none)
            update
            view
