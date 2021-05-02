[<RequireQualifiedAccess>]
module TodoMvu.Task

open System
open Elmish
open Bolero
open Bolero.Html

type Model =
    { Id: Guid
      Title: string
      Completed: bool }

module Model =

    let empty =
        { Id = Guid.Empty
          Title = String.Empty
          Completed = false }

    let create title =
        { Id = Guid.NewGuid ()
          Title = title
          Completed = false }


type Message =
    | Completed of bool
    | Destroy

let update message model =
    match message with
    | Completed completed ->
        { model with Completed = completed }, Cmd.none
    | Destroy ->
        model, Cmd.none

type Component () =
    inherit ElmishComponent<Model, Message> ()
    override _.View model dispatch =
        li [ attr.classes [ "todo"; if model.Completed then "completed" ] ] [
            div [ attr.``class`` "view" ] [
                input
                    [ bind.``checked`` model.Completed (Completed >> dispatch)
                      attr.``class`` "toggle"
                      attr.``type`` "checkbox" ]
                label [] [ text model.Title ]
                button
                    [ on.click (fun _ -> dispatch Destroy)
                      attr.``class`` "destroy" ]
                    []
            ]
            input [ attr.``class`` "edit"; attr.``type`` "text" ]
        ]

let view model dispatch =
    ecomp<Component,_,_> [] model dispatch
