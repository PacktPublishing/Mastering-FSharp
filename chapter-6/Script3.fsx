#r "System.Data.Entity"
#r "System.Data.Linq"
#r "../packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"

open System
open System.Data
open System.Data.Linq
open System.Data.EntityClient
open Microsoft.FSharp.Data.TypeProviders

type EntityConnection = SqlEntityConnection<ConnectionString="Server=(localdb)\MSSQLLocalDB;Initial Catalog=Northwind;Integrated Security=SSPI;MultipleActiveResultSets=true",Pluralize = true>;;
let northwindEntities = EntityConnection.GetDataContext()

type NorthwindDbSchema = SqlDataConnection<"Data Source= (localdb)\MSSQLLocalDB;Initial Catalog=Northwind;Integrated Security=True">;;
let northwindDB = NorthwindDbSchema.GetDataContext()
northwindDB.DataContext.Log <- System.Console.Out

// LINQ to Entities Query
let pQuery =
        query {
                for p in northwindDB.Products do
                select p
        }

pQuery |> Seq.iter(fun prod ->
        printfn "%d; %s; %A" prod.ProductID prod.ProductName prod.UnitPrice)

// SQL Entity Type Provider

let psQuery =
        query {
                for p in northwindDB.Products do
                select p
        }

psQuery |> Seq.iter(fun prod ->
        printfn "%d; %s" prod.ProductID  prod.ProductName)

// SQL Entity Type Provider with different type of LINQ operators

let pWQuery =
        query {
                for p in northwindDB.Products do
                where (p.UnitPrice.Value > 15m)
        }

pWQuery |> Seq.iter(fun prod ->
        printfn "%d; %s; %A" prod.ProductID prod.ProductName prod.UnitPrice)



