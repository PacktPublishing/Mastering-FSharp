#r "System.Data.Services.Client"
#r "../packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"

open System
open System.Linq
open System.Data
open Microsoft.FSharp.Data.TypeProviders

type northwindOData = ODataService<"http://services.odata.org/Northwind/Northwind.svc/">
let db = northwindOData.GetDataContext()

//Select operation in OData service

let pQuery =
    query {
        for p in db.Products do
            select p
    }

pQuery |> Seq.iter(fun prod ->
    printfn "%d; %s; %A" prod.ProductID prod.ProductName prod.UnitPrice)

// Specifying conditions in where clause

let pWQuery =
    query {
        for p in db.Products do
            where (p.UnitPrice.Value > 25.0m)
    }

pWQuery |> Seq.iter(fun prod ->
    printfn "%d; %s; %A" prod.ProductID prod.ProductName prod.UnitPrice)


// Using different combination in the query

let pWQuery2 =
    query {
        for p in db.Products do
        where (p.ProductName.Contains("C"))
    }
pWQuery2 |> Seq.iter(fun prod -> printfn "%d; %s; %A" prod.ProductID prod.ProductName prod.UnitPrice)

