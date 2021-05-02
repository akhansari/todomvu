[<RequireQualifiedAccess>]
module TodoMvu.Task

open System
open Elmish
open Bolero
open Bolero.Html

type Model =
    { Id: Guid
      Title: string }
    static member Empty =
        { Id = Guid.Empty
          Title = String.Empty }

let create title =
    { Id = Guid.NewGuid ()
      Title = title }

type Message =
    | Destroy

let update message model =
    match message with
    | Destroy ->
        model, Cmd.none

type Component () =
    inherit ElmishComponent<Model, Message> ()
    override _.View model dispatch =
        li [ attr.``class`` "todo" ] [
            div [ attr.``class`` "view" ] [
                input [ attr.``class`` "toggle"; attr.``type`` "checkbox" ]
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
