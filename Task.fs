[<RequireQualifiedAccess>]
module TodoMvu.Task

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client

type Model =
    { Id: Guid
      Title: string }

let create title =
    { Id = Guid.NewGuid ()
      Title = title }
