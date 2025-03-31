# WebSharper WebUSB API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [WebUSB API](https://developer.mozilla.org/en-US/docs/Web/API/WebUSB_API), enabling seamless communication with USB devices in WebSharper applications.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the WebUSB API.

2. **Sample Project**:
   - Demonstrates how to use the WebUSB API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/WebUSB/).

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.WebUSB
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/WebUSB.git
   cd WebUSB
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.WebUSB/WebSharper.WebUSB.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.WebUSB.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/WebUSB/](https://dotnet-websharper.github.io/WebUSB/)

## Example Usage

Below is an example of how to use the WebUSB API in a WebSharper project:

```fsharp
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

    // Variable to display USB connection status
    let statusMessage = Var.Create "Waiting for connection..."
    let device = Var.Create<USBDevice> JS.Undefined

    // Function to request a USB device connection
    let connectUSB() =
        promise {
            try
                let requestOptions = {| filters = [||] |} |> As<obj array>

                let! selectedDevice  = JS.Window.Navigator.Usb.RequestDevice(requestOptions)

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

    // Function to send data to a USB device
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

```

This example demonstrates how to request and connect to a USB device using the WebUSB API in a WebSharper project.
