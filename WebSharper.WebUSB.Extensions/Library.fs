namespace WebSharper.WebUSB

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type Navigator with
        [<Inline "$this.usb">]
        member this.Usb with get(): USB = X<USB>

    type WorkerNavigator with
        [<Inline "$this.usb">]
        member this.Usb with get(): USB = X<USB>
