[<RequireQualifiedAccess>]
module TodoMvu.App

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client

type Model =
    { NewTask: string
      Tasks: Task.Model list }
    static member Empty =
        { NewTask = String.Empty
          Tasks = [] }

type Message =
    | SetNew of string
    | Add

let update message model =
    match message with
    | SetNew title ->
        { model with NewTask = title }, Cmd.none
    | Add ->
        { model with
            NewTask = String.Empty
            Tasks = Task.create model.NewTask :: model.Tasks }
        , Cmd.none

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
                li [ attr.``class`` "todo" ] [
                    div [ attr.``class`` "view" ] [
                        input [ attr.``class`` "toggle"; attr.``type`` "checkbox" ]
                        label [] [ text task.Title ]
                        button [ attr.``class`` "destroy" ] []
                    ]
                    input [ attr.``class`` "edit"; attr.``type`` "text" ]
                ]
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
    inherit ProgramComponent<Model, Message>()
    override this.Program =
        Program.mkProgram (fun _ -> Model.Empty, Cmd.none) update view
#if DEBUG
        |> Program.withTrace (fun msg mdl ->
            this.JSRuntime.InvokeVoidAsync("console.log", string msg, mdl) |> ignore)
#endif
