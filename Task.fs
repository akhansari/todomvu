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
    unit

let update message model =
    model, Cmd.none

type Component () =
    inherit ElmishComponent<Model, Message> ()
    override _.View model dispatch =
        li [ attr.``class`` "todo" ] [
            div [ attr.``class`` "view" ] [
                input [ attr.``class`` "toggle"; attr.``type`` "checkbox" ]
                label [] [ text model.Title ]
                button [ attr.``class`` "destroy" ] []
            ]
            input [ attr.``class`` "edit"; attr.``type`` "text" ]
        ]

let view model dispatch =
    ecomp<Component,_,_> [] model dispatch
