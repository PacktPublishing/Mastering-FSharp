  open System.Threading

    let sleep t = Async.FromContinuations(fun (cont, erFun, _) ->
        let rec timer = new Timer(TimerCallback(callback))
        and callback state =
            timer.Dispose()
            cont(())
        timer.Change(t, Timeout.Infinite) |> ignore
        )

     let testSleep = async {
        printfn "Before"
        do! sleep 5000
        printfn "After 5000 msecs"
        }

         Async.Start testSleep