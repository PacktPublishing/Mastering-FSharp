open System.Threading

let testAwaitTask = async {
        printfn "Starting"
        let! child = Async.StartChildAsTask <| async {  // Async.StartChildAsTask shall be described later
                printfn "Child started"
                Thread.Sleep(5000)
                printfn "Child finished"
                return 100
            }
        printfn "Waiting for the child task"
        let! result = Async.AwaitTask child
        printfn "Child result %d" result
        }

        Async.Start testAwaitTask

