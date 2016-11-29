#r "../packages/FSharp.Data/lib/net40/FSharp.Data.dll"

#I "../packages/FSharp.Charting"
#load "FSharp.Charting.fsx"
// If you're on Mono, comment the two lines above
// and use the following two instead
// #I "../packages/FSharp.Charting.Gtk"
// #load "FSharp.Charting.Gtk.fsx"

open FSharp.Data
open FSharp.Charting
open Microsoft.FSharp.Linq

let data = WorldBankData.GetDataContext()

// DRAW A PIE:
// Change the year and the countries to get different data
let year = 2015
let countries = [|
  data.Countries.India
  data.Countries.``United States``
  data.Countries.``Sri Lanka``
  data.Countries.Spain
|]

let gdp =
  countries
  |> Array.map(fun c -> c.Name, c.Indicators.``GDP (current US$)``.[year])

Chart.Pie(gdp, Name = sprintf "GDP %i" year)

// DRAW A LINE:
// Change the country and the year range to get different data
let country = data.Countries.India
let years = [| 2006 .. 2015|]

years
|> Array.map(fun y -> country.Indicators.``GDP (current US$)``.[y])
|> Chart.Line
