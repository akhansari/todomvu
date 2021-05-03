[<RequireQualifiedAccess>]
module TodoMvu.Task

open System
open Elmish
open Bolero
open Bolero.Html

type State = Active | Completed

type Model =
    { Id: Guid
      Title: string
      State: State }

module Model =

    let empty =
        { Id = Guid.Empty
          Title = String.Empty
          State = Active }

    let create title =
        { Id = Guid.NewGuid ()
          Title = title
          State = Active }

    let addTo tasks title =
        create title :: tasks

    let removeFrom tasks task =
        tasks |> List.filter (fun t -> t.Id <> task.Id)

    let updateIn tasks newTask =
        tasks |> List.map (fun oldTask -> if oldTask.Id = newTask.Id then newTask else oldTask)

type Message =
    | SetCompleted of bool
    | Destroy

let update message model =
    match message with
    | SetCompleted completed ->
        { model with State = if completed then Completed else Active }
        , Cmd.none
    | Destroy ->
        model, Cmd.none

type Component () =
    inherit ElmishComponent<Model, Message> ()
    override _.View model dispatch =
        li [ attr.classes [ "todo"; if model.State = Completed then "completed" ] ] [
            div [ attr.``class`` "view" ] [
                input
                    [ bind.``checked`` (model.State = Completed) (SetCompleted >> dispatch)
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
