open System

let testAwaitWaitHandle waitHandle = async {
        printfn "Before waiting"
        let! r = Async.AwaitWaitHandle waitHandle
        printfn "After waiting"
    }

let runTestAwaitWaitHandle () =
        let event = new System.Threading.ManualResetEvent(false)
        Async.Start <| testAwaitWaitHandle event
        System.Threading.Thread.Sleep(3000)
        printfn "Before raising"
        event.Set() |> ignore
        printfn "After raising"

runTestAwaitWaitHandle()
