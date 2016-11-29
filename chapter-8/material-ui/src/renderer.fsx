#r "../../../node_modules/fable-core/Fable.Core.dll"
#r "../../../node_modules/fable-react/Fable.React.dll"

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open Fable.Helpers.React.Props

module R = Fable.Helpers.React
type RCom = React.ComponentClass<obj>

let deepOrange500 = importMember<string> "material-ui/styles/colors"
// Will translate to the following ES6 member import
// import { deepOrange500 } from "material-ui/styles/colors"

let RaisedButton = importDefault<RCom> "material-ui/RaisedButton"
// Will translate to the following ES6 default import
// import RaisedButton from "material-ui/RaisedButton"

let Dialog = importDefault<RCom> "material-ui/Dialog"
let FlatButton = importDefault<RCom> "material-ui/FlatButton"
let MuiThemeProvider = importDefault<RCom> "material-ui/styles/MuiThemeProvider"
let getMuiTheme = importDefault<obj->obj> "material-ui/styles/getMuiTheme"

// Helpers for dynamic programming
let inline (~%) x = createObj x
let inline (=>) x y = x ==> y

let muiTheme =
    %["palette" =>
        %["accent1Color" => deepOrange500]]
    |> getMuiTheme

type MainState = { isOpen: bool; secret: string }

type Main(props) as this =
    inherit React.Component<obj,MainState>(props)
    do this.state <- {isOpen=false; secret=""}

    member this.handleRequestClose() =
        this.setState({isOpen=false; secret=""})

    member this.handleTouchTap() =
         this.setState({isOpen=true; secret="1-2-3-4-5"})

    // Use the code below instead to readt the data from a file

    // member this.handleTouchTap() =
    //     let filePath = Fable.Import.Node.__dirname + "/data/secret.txt"
    //     Fable.Import.Node.fs.readFile(filePath, fun err buffer ->
    //         if (box err) <> null then
    //             failwith "Couldn't read file"
    //         this.setState({isOpen=true; secret=string buffer}))

    member this.render() =
        let standardActions =
            R.from FlatButton
                %["label" => "Ok"
                  "primary" => true
                  "onTouchTap" => this.handleRequestClose] []
        R.from MuiThemeProvider
            %["muiTheme" => muiTheme] [
                R.div [Style [TextAlign "center"
                              PaddingTop 200]] [
                    R.from Dialog
                        %["open" => this.state.isOpen
                          "title" => "Super Secret Password"
                          "actions" => standardActions
                          "onRequestClose" => this.handleRequestClose]
                        [R.str this.state.secret]
                    R.h1 [] [R.str "Material-UI"]
                    R.h2 [] [R.str "example project"]
                    R.from RaisedButton
                        %["label" => "Super Secret Password"
                          "secondary" => true
                          "onTouchTap" => this.handleTouchTap] []
                ]
            ]

(importDefault "react-tap-event-plugin")()

ReactDom.render(
    R.com<Main,_,_> None [],
    document.getElementById("app")
)