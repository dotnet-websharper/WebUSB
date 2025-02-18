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
- Node.js and npm (for building web assets).
- WebSharper tools.

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
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.WebUSB

[<JavaScript>]
module Client =
    // Define the connection to the HTML template
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // Variable to display USB connection status
    let statusMessage = Var.Create "Click the button to connect to a USB device."

    // Function to request a USB device connection
    let connectUSB () =
        promise {
            try
                let usb = As<Navigator>(JS.Window.Navigator).Usb
                let! device = usb.RequestDevice(USBDeviceRequestOptions(Filters = [||]))
                do! device.Open()
                statusMessage := "Connected to USB device!"
            with ex ->
                Console.Error("USB connection failed:", ex.Message)
                statusMessage := "USB connection failed!"
        }

    [<SPAEntryPoint>]
    let Main () =
        // Initialize the UI template and bind USB connection status
        IndexTemplate.Main()
            .connectUSB(fun _ ->
                async {
                    do! connectUSB().AsAsync()
                }
                |> Async.Start
            )
            .status(statusMessage.V)
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to request and connect to a USB device using the WebUSB API in a WebSharper project.

## Important Considerations

- **Permissions**: Users must grant permission for USB access when prompted by the browser.
- **Secure Context**: The WebUSB API only works on secure origins (HTTPS) for security reasons.
- **Limited Browser Support**: Some browsers may not fully support the WebUSB API; check [MDN WebUSB API](https://developer.mozilla.org/en-US/docs/Web/API/WebUSB_API) for the latest compatibility information.

## License

This project is licensed under the Apache License 2.0. See the [LICENSE](LICENSE.md) file for details.

## Acknowledgments

Special thanks to the WebSharper team and contributors for their efforts in enabling seamless integration of APIs into F# projects.
