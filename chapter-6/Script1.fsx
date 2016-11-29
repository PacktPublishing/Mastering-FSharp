open System
open System.Linq
open Microsoft.FSharp.Linq

type SeqBuilder() =
    member x.For(source: seq<'t>, body: 't -> seq<'v>) =
        seq { for v in source do yield! body v }

    member x.Yield (item: 't) : seq<'t> = seq { yield item }

    member x.Zero () = Seq.empty

    [<CustomOperation("select")>]
    member x.Select(source: seq<'t>, f: 't -> 'v) : seq<'v> =
        Seq.map f source

let myseq = new SeqBuilder()

let x =
    myseq {
        for i in 1..10 do
        select (fun i -> i + 10)
    }

