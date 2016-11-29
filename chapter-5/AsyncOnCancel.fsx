open System
open System.Threading

let simulatedLengthyOperation() =
        async {
            // Async.Sleep checks for cancellation at the end of the interval,
            // loop over many short intervals instead of sleeping for a long one.
                while true do
                do! Async.Sleep(100)
        }

        let computation id (tokenSource:System.Threading.CancellationTokenSource) =
            async {
                    use! cancelHandler = Async.OnCancel(fun () ->
                     printfn "Lengthy operation %s cancelled" id)
                    do! simulatedLengthyOperation()
            }
            |> fun workflow -> Async.Start(workflow, tokenSource.Token)

        let tokenSource1 = new System.Threading.CancellationTokenSource()
        let tokenSource2 = new System.Threading.CancellationTokenSource()

        computation "A" tokenSource1
        computation "B" tokenSource2
        printfn "Started computations."
        System.Threading.Thread.Sleep(1000)
        printfn "Sending cancellation signal."
        tokenSource2.Cancel()
        tokenSource1.Cancel()
