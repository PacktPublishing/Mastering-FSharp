open System
open System.Linq
open Microsoft.FSharp.Linq

type SeqBuilder() =
    member x.For(source: seq<'t>, body: 't -> seq<'v>) =
        seq { for v in source do yield! body v }

    member x.Yield (item: 't) : seq<'t> =
        seq { yield item }

    member x.Zero () = Seq.empty

    // Add projection parameter if you want to improve the select function
    [<CustomOperation("select")>]
    member x.Select(source: seq<'t>, [<ProjectionParameter>] f: 't -> 'v) : seq<'v> =
        Seq.map f source

    //Add this Custom operation with MaintainVariableSpace value.
    [<CustomOperation("sort", MaintainsVariableSpace=true)>]
    member x.Sort (source: seq<'t>) =
        Seq.sort source

   let myseq = new SeqBuilder()

     let x1 =
        myseq {
                for i in 1..10 do
                select (i + 10)
        }



