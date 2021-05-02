﻿[<RequireQualifiedAccess>]
module TodoMvu.App

open System
open Elmish
open Bolero
open Bolero.Html

type Model =
    { NewTask: string
      Tasks: Task.Model list }

module Model =
    let empty =
        { NewTask = String.Empty
          Tasks = List.empty }

type Message =
    | SetNew of string
    | Add
    | Remove of Task.Model
    | WrapTask of Task.Model * Task.Message

let update message model =
    match message with
    | SetNew title ->
        { model with NewTask = title }
        , Cmd.none
    | Add ->
        { model with
            NewTask = String.Empty
            Tasks = Task.Model.create model.NewTask :: model.Tasks }
        , Cmd.none
    | Remove task ->
        { model with Tasks = model.Tasks |> List.filter (fun t -> t.Id <> task.Id) }
        , Cmd.none
    | WrapTask (task, taskMsg) ->
        let task, taskCmd = Task.update taskMsg task
        { model with Tasks = model.Tasks |> List.map (fun t -> if t.Id = task.Id then task else t) }
        , Cmd.batch [
            Cmd.map WrapTask taskCmd
            if taskMsg = Task.Destroy then Cmd.ofMsg (Remove task)
        ]

let headerView model dispatch =
    header [ attr.``class`` "header" ] [
        h1 [] [ text "todos" ]
        input [
            on.keydown (fun e -> if e.Key = "Enter" then dispatch Add)
            bind.input.string model.NewTask (SetNew >> dispatch)
            attr.``class`` "new-todo"
            attr.placeholder "What needs to be done?"
            attr.autocomplete false
            attr.autofocus true
        ]
    ]

let mainView model dispatch =
    section [ attr.``class`` "main" ] [
        input [ attr.id "toggle-all"; attr.``class`` "toggle-all"; attr.``type`` "checkbox" ]
        label [ attr.``for`` "toggle-all" ] [ text "Mark all as complete" ]
        ul [ attr.``class`` "todo-list" ] [
            forEach model.Tasks <| fun task ->
                Task.view task (fun m -> WrapTask (task, m) |> dispatch)
        ]
    ]

let footerView model dispatch =
    footer [ attr.``class`` "footer" ] [
        span [ attr.``class`` "todo-count" ] []
        ul [ attr.``class`` "filters" ] [
            li [] [ a [] [ text "All" ] ]
            li [] [ a [] [ text "Active" ] ]
            li [] [ a [] [ text "Completed" ] ]
        ]
        button [ attr.``class`` "clear-completed" ] [ text "Clear completed" ]
    ]

let view model dispatch =
    section [ attr.``class`` "todoapp" ] [
        headerView model dispatch
        cond model.Tasks.IsEmpty <| function
        | true -> Empty
        | false ->
            concat [
                mainView model dispatch
                footerView model dispatch
            ]
    ]

open Microsoft.JSInterop

type Component () =
    inherit ProgramComponent<Model, Message> ()
    override this.Program =
        Program.mkProgram (fun _ -> Model.empty, Cmd.none) update view
#if DEBUG
        |> Program.withTrace (fun msg mdl ->
            this.JSRuntime.InvokeVoidAsync("console.log", string msg, mdl) |> ignore)
#endif
