namespace WebSharper.WebUSB

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    module Enum = 
        let Direction = 
            Pattern.EnumStrings "Direction" [
                "in"
                "out"
            ]

        let USBTransferStatus =
            Pattern.EnumStrings "USBTransferStatus" [
                "ok"      
                "stall"   
                "babble"  
            ]

        let USBEndpointType =
            Pattern.EnumStrings "USBEndpointType" [
                "bulk"       
                "interrupt"  
                "isochronous" 
            ]
    
    let USBEndpoint =
        Class "USBEndpoint"
        |+> Static [
            Constructor (!?T<int>?endpointNumber * !?Enum.Direction?direction) 
        ]
        |+> Instance [
            "endpointNumber" =? T<int> 
            "direction" =? Enum.Direction 
            "type" =? Enum.USBEndpointType 
            "packetSize" =? T<int> 
        ]

    let USBDevice =
        Class "USBDevice"

    let USBAlternateInterface =
        Class "USBAlternateInterface"
        |+> Static [
            Constructor (!?T<int>?alternateSetting) 
        ]
        |+> Instance [
            "alternateSetting" =? T<int> 
            "interfaceClass" =? T<int> 
            "interfaceSubclass" =? T<int> 
            "interfaceProtocol" =? T<int> 
            "interfaceName" =? T<string> 
            "endpoints" =? !|USBEndpoint 
        ]

    let USBInterface =
        Class "USBInterface"
        |+> Static [
            Constructor (!?T<int>?interfaceNumber) 
        ]
        |+> Instance [
            "interfaceNumber" =? T<int> 
            "alternate" =? USBAlternateInterface 
            "alternates" =? !|USBAlternateInterface 
            "claimed" =? T<bool> 
        ]

    let USBConnectionEventOptions = 
        Pattern.Config "USBConnectionEventOptions" {
            Required = []
            Optional = [
                "device", USBDevice.Type
            ]
        }

    let USBConnectionEvent =
        Class "USBConnectionEvent"
        |=> Inherits T<Dom.Event> 
        |+> Static [
            Constructor (T<string>?``type`` * USBConnectionEventOptions?options) 
        ]
        |+> Instance [
            "device" =? USBDevice 
        ]

    let USBConfiguration =
        Class "USBConfiguration"
        |+> Static [
            Constructor (!?USBDevice?device * !?T<uint>?configurationValue)
        ]
        |+> Instance [

            
            "configurationValue" =? T<int> 
            "configurationName" =? T<string> 
            "interfaces" =? !| USBInterface 
        ]
    
    let USBInTransferResult =
        Class "USBInTransferResult"
        |+> Static [
            Constructor (!?Enum.USBTransferStatus?status * !?T<DataView>?data) 
        ]
        |+> Instance [
            "data" =? T<DataView> 
            "status" =? Enum.USBTransferStatus 
        ]

    let USBOutTransferResult =
        Class "USBOutTransferResult"
        |+> Static [
            Constructor (!?Enum.USBTransferStatus?status * !?T<int>?bytesWritten) 
        ]
        |+> Instance [
            "bytesWritten" =? T<int> 
            "status" =? Enum.USBTransferStatus   
        ]

    let USBControlTransferSetup =
        Pattern.Config "USBControlTransferSetup" {
            Required = []
            Optional = [
                "requestType", T<string>  
                "recipient", T<string>    
                "request", T<int>         
                "value", T<int>           
                "index", T<int>
            ]
        }

    USBDevice 
    |+> Instance [        
        "configuration" =? USBConfiguration 
        "configurations" =? !| USBConfiguration 
        "deviceClass" =? T<int> 
        "deviceProtocol" =? T<int> 
        "deviceSubclass" =? T<int> 
        "deviceVersionMajor" =? T<int> 
        "deviceVersionMinor" =? T<int> 
        "deviceVersionSubminor" =? T<int> 
        "manufacturerName" =? T<string> 
        "opened" =? T<bool> 
        "productId" =? T<int> 
        "productName" =? T<string> 
        "serialNumber" =? T<string> 
        "usbVersionMajor" =? T<int> 
        "usbVersionMinor" =? T<int> 
        "usbVersionSubminor" =? T<int> 
        "vendorId" =? T<int> 
        
        "claimInterface" => T<int>?interfaceNumber ^-> T<Promise<unit>> 
        "clearHalt" => Enum.Direction?direction * T<int>?endpointNumber ^-> T<Promise<unit>> 
        "controlTransferIn" => !?USBControlTransferSetup?setup * T<int>?length ^-> T<Promise<_>>[USBInTransferResult] 
        "controlTransferOut" => !?USBControlTransferSetup?setup * !? T<ArrayBuffer>?data ^-> T<Promise<_>>[USBInTransferResult] 
        "close" => T<unit> ^-> T<Promise<unit>> 
        "forget" => T<unit> ^-> T<Promise<unit>> 
        "isochronousTransferIn" => T<int>?endpointNumber * T<int>?packetLengths ^-> T<Promise<unit>> 
        "isochronousTransferOut" => T<int>?endpointNumber * T<ArrayBuffer>?data * T<int>?packetLengths ^-> T<Promise<unit>> 
        "open" => T<unit> ^-> T<Promise<unit>> 
        "releaseInterface" => T<int>?interfaceNumber ^-> T<Promise<unit>> 
        "reset" => T<unit> ^-> T<Promise<unit>> 
        "selectAlternateInterface" => T<int>?interfaceNumber * T<int>?alternateSetting ^-> T<Promise<unit>> 
        "selectConfiguration" => T<int>?configurationValue ^-> T<Promise<unit>> 
        "transferIn" => T<int>?endpointNumber * T<int>?length ^-> T<Promise<unit>> 
        "transferOut" => T<int>?endpointNumber * T<ArrayBuffer>?data ^-> T<Promise<unit>> 
    ] |> ignore

    let USBDeviceFilter =
        Pattern.Config "USBDeviceFilter" {
            Required = []
            Optional = [
                "vendorId", T<string>  
                "productId", T<string> 
                "classCode", T<string> 
                "subclassCode", T<string> 
                "protocolCode", T<string> 
                "serialNumber", T<string> 
            ]
        }

    let USB =
        Class "USB"
        |=> Inherits T<Dom.EventTarget> 
        |+> Instance [
            "getDevices" => T<unit> ^-> T<Promise<_>>[!|USBDevice] 
            "requestDevice" => (!|T<obj>)?filters ^-> T<Promise<_>>[USBDevice] 

            "onconnect" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnConnect instead"
            "onconnect" =@ !?(USBConnectionEvent ^-> T<unit>)
            |> WithSourceName "OnConnect"
            "ondisconnect" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnDisonnect instead"
            "ondisconnect" =@ !?(USBConnectionEvent ^-> T<unit>)
            |> WithSourceName "OnDisonnect"
        ]
    
    let Navigator =
        Class "Navigator"
        |+> Instance [
            "usb" =? USB 
        ]

    let WorkerNavigator =
        Class "WorkerNavigator"
        |+> Instance [
            "usb" =? USB 
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.WebUSB" [
                WorkerNavigator
                Navigator
                USB
                USBDeviceFilter
                USBControlTransferSetup
                USBOutTransferResult
                USBInTransferResult
                USBConfiguration
                USBConnectionEvent
                USBConnectionEventOptions
                USBInterface
                USBAlternateInterface
                USBDevice
                USBEndpoint
                Enum.USBEndpointType
                Enum.USBTransferStatus
                Enum.Direction
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
