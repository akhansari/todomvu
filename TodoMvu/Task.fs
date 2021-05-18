[<RequireQualifiedAccess>]
module TodoMvu.Task

open System
open System.Threading.Tasks
open Elmish
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components

type State = Active | Completed

type Mode = View | Edit of string

type Model =
    { Id: Guid
      Title: string
      State: State
      Mode: Mode }

module Model =

    let empty =
        { Id = Guid.Empty
          Title = String.Empty
          State = Active
          Mode = View }

    let create title =
        { Id = Guid.NewGuid ()
          Title = title
          State = Active
          Mode = View }

    let addTo tasks title =
        create title :: tasks

    let updateIn tasks newTask =
        tasks |> List.map (fun oldTask -> if oldTask.Id = newTask.Id then newTask else oldTask)

    let removeFrom tasks task =
        tasks |> List.filter (fun t -> t.Id <> task.Id)

    let setState tasks state =
        tasks |> List.map (fun t -> { t with State = state })

    let cleanState tasks state =
        tasks |> List.filter (fun t -> t.State <> state)

type Message =
    | SetCompleted of bool
    | Destroy
    | SetMode of Mode
    | SetTitle

let update message model =
    match message with
    | SetCompleted completed ->
        { model with State = if completed then Completed else Active }
        , Cmd.none
    | SetMode mode ->
        { model with Mode = mode }
        , Cmd.none
    | SetTitle ->
        match model.Mode with
        | View -> model
        | Edit title ->
            { model with
                Title = title
                Mode = View }
        , Cmd.none
    | Destroy ->
        model, Cmd.none

type Component () =
    inherit ElmishComponent<Model, Message> ()

    let inputRef = HtmlRef ()

    override _.View model dispatch =
        li
            [ attr.classes
                [ "todo"
                  if model.Mode <> View then "editing"
                  if model.State = Completed then "completed" ] ]
            [
                div [ attr.``class`` "view" ] [
                    input
                        [ bind.``checked`` (model.State = Completed) (SetCompleted >> dispatch)
                          attr.``class`` "toggle"
                          attr.``type`` "checkbox" ]
                    label
                        [ on.dblclick (fun _ -> SetMode (Edit model.Title) |> dispatch) ]
                        [ text model.Title ]
                    button
                        [ on.click (fun _ -> dispatch Destroy)
                          attr.``class`` "destroy" ]
                        []
                ]
                input
                    [ attr.ref inputRef
                      bind.input.string model.Title (Edit >> SetMode >> dispatch)
                      on.keydown (fun e -> if e.Key = "Escape" then SetMode View |> dispatch)
                      on.keypress (fun e -> if e.Key = "Enter" then dispatch SetTitle)
                      on.blur (fun e -> dispatch SetTitle)
                      attr.``class`` "edit"
                      attr.``type`` "text" ]
            ]

    override _.OnAfterRenderAsync _ =
        match inputRef.Value with
        | Some elem -> elem.FocusAsync().AsTask()
        | None      -> Task.CompletedTask

let view model dispatch =
    ecomp<Component,_,_> [] model dispatch
