#r "../packages/Suave/lib/net40/Suave.dll"
#r "../packages/DotLiquid/lib/net451/DotLiquid.dll"
#r "../packages/Suave.DotLiquid/lib/net40/Suave.DotLiquid.dll"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

let greet area (reqParams: (string * (string option)) list) =
    reqParams
    |> List.tryPick (fun (k,v) -> if k = "name" then v else None)
    |> function Some name -> name | None -> "World"
    |> sprintf "Hello from %s, %s!" area

// Templating with DotLiquid (see "template.html")

type Teacher = {
    Name: string
    Students: string list
}

let model = {
    Name="Sarah"
    Students=["Peter"; "Linda"; "Mariah"]
}

DotLiquid.setTemplatesDir(__SOURCE_DIRECTORY__)

let page (model: Teacher) =
    DotLiquid.page ("template.html") model

// Use the code below instead to use Suave Experimental
// templating DSL instead of DotLiquid

// #r "../packages/Suave.Experimental/lib/net40/Suave.Experimental.dll"
// open Suave.Html

// let page (model: Teacher) =
//     html [
//         head [ title "Suave HTML" ]
//         body [
//             tag "h1" [] (text model.Name)
//             tag "ul" [] (flatten
//                 [for item in model.Students do
//                     yield tag "li" [] (item.ToUpper() |> text)]
//             )
//         ]
//     ] |> xmlToString

let app =
    choose [
        path "/" >=> page model
        path "/public" >=> choose [
            // Access the HttpRequest and check the query of form parameters
            GET >=> request(fun request -> greet "public" request.query |> OK)
            POST >=> request(fun request -> greet "public" request.form |> OK)
        ]
        // This route is protected by HTTP Basic Authentication
        path "/private" >=> Authentication.authenticateBasic
            (fun (username, password) -> username = "me" && password = "abc")
            (choose [
                GET >=> request(fun request -> greet "private" request.query |> OK)
                POST >=> request(fun request -> greet "private" request.form |> OK)
            ])
        RequestErrors.NOT_FOUND "Found no handlers"
    ]

startWebServer defaultConfig app
