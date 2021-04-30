[<RequireQualifiedAccess>]
module TodoMvu.App

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client

type Model =
    { Init: unit }
    static member Empty =
        { Init = () }

type Message = unit

let update message model =
    model, Cmd.none

let view model dispatch =
    section [ attr.``class`` "todoapp" ] [
        header [ attr.``class`` "header" ] [
            h1 [] [ text "todos" ]
            input
                [ attr.``class`` "new-todo"
                  attr.placeholder "What needs to be done?"
                  attr.autocomplete false
                  attr.autofocus true ]
        ]
        section [ attr.``class`` "main" ] [
            input [ attr.id "toggle-all"; attr.``class`` "toggle-all"; attr.``type`` "checkbox" ]
            label [ attr.``for`` "toggle-all" ] [ text "Mark all as complete" ]
            ul [ attr.``class`` "todo-list" ] [
            ]
        ]
        footer [ attr.``class`` "footer" ] [
            span [ attr.``class`` "todo-count" ] []
            ul [ attr.``class`` "filters" ] [
                li [] [ a [] [ text "All" ] ]
                li [] [ a [] [ text "Active" ] ]
                li [] [ a [] [ text "Completed" ] ]
            ]
            button [ attr.``class`` "clear-completed" ] [ text "Clear completed" ]
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
