#r "../node_modules/fable-core/Fable.Core.dll"
#r "../node_modules/fable-react/Fable.React.dll"

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.React.Props
module R=Fable.Helpers.React

type Todo =
    { id: DateTime; text: string }

// The PojoAttribute means the record will be
// compiled as a Plain JS Object, required by React
type [<Pojo>] TodoList =
    { items: Todo list; text: string }

type [<Pojo>] TodoAppProps =
    { url: string; pollInterval: float }

type TodoListView(props) =
    inherit React.Component<TodoList,obj>(props)

    member this.render() =
        let createItem (item: Todo) =
            R.li [Key (string item.id)]
                 [R.str item.text]
        List.map createItem this.props.items
        |> R.ul []

type TodoAppView(props) =
    inherit React.Component<obj,TodoList>(props)
    do base.setInitState({ items = []; text = "" })

    member this.onChange(e: React.SyntheticEvent) =
        let newtext = unbox e.target?value
        { this.state with text = newtext }
        |> this.setState

    member this.handleSubmit(e: React.SyntheticEvent) =
        e.preventDefault()
        let newItem = {
            text = this.state.text
            id = DateTime.Now
        }
        let nextItems = this.state.items@[newItem]
        this.setState({ items = nextItems; text="" })

    member this.render() =
        let buttonText =
            this.state.items.Length + 1
            |> sprintf "Add #%i"
        R.div [] [
            R.h3 [] [R.str "TODO"]
            R.com<TodoListView,_,_> this.state []
            R.form [OnSubmit this.handleSubmit] [
                R.input [
                    OnChange this.onChange
                    Value (U2.Case1 this.state.text)
                ] []
                R.button [] [R.str buttonText]
            ]
        ]

ReactDom.render(
    R.com<TodoAppView,_,_> None [],
    Browser.document.getElementById "content")

ReactDom.render(
    R.com<TodoAppView,_,_> {
        url = "/api/comments"
        pollInterval = 2000.
    } [],
    Browser.document.getElementById "content")