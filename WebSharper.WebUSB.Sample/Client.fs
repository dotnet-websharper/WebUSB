namespace WebSharper.WebUSB.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.WebUSB

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let statusMessage = Var.Create "Waiting for connection..."
    let device = Var.Create<USBDevice> JS.Undefined

    let usb = As<Navigator>(JS.Window.Navigator).Usb

    let connectUSB() = 
        promise {
            try 
                let requestOptions = {| filters = [||] |} |> As<obj array>

                let! selectedDevice  = usb.RequestDevice(requestOptions)

                do! selectedDevice.Open()
                do! selectedDevice.SelectConfiguration(1)
                do! selectedDevice.ClaimInterface(0)

                device := selectedDevice

                statusMessage := $"Connected to USB device: ${device.Value.ProductName}"
                printfn($"USB Device Connected: {device.Value}")
            with error ->
                statusMessage := $"Connection Failed: {error.Message}"
                printfn($"USB Connection Error: {error}")
        }

    let sendUSBData() =
        promise {
            if isNull (device.Value) then
                statusMessage := "Please connect a USB device first."

            else 
                try 
                    let encoder = JS.Eval "new TextEncoder()"
                    let data = encoder?encode("Hello USB!")

                    do! device.Value.TransferOut(1, data)
                    statusMessage := "Data sent to USB device."
                    printfn($"Data sent: {data}")
                with error ->
                    statusMessage := $"Data transfer failed: {error.Message}"
                    printfn($"USB Data Transfer Error: {error}")
        }

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            .status(statusMessage.V)
            .connectUSB(fun _ ->
                async {
                    do! connectUSB().AsAsync()
                }
                |> Async.StartImmediate
            )
            .sendUSBData(fun _ ->
                async {
                    do! sendUSBData().AsAsync()
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
