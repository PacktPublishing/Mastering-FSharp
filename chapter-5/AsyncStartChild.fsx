open System.Threading

    let subTask v = async   {
        printfn "Task %d started" v
        Thread.Sleep (v * 1000)
        printfn "Task %d finished" v
        return v
        }

     let mainTask = async    {
        printfn "Main task started"

        let! childTask1 = Async.StartChild (subTask 1)
        let! childTask2 = Async.StartChild (subTask 5)
        printfn "Subtasks started"

        let! child1Result = childTask1
        printfn "Subtask1 result: %d" child1Result

        let! child2Result = childTask2
        printfn "Subtask2 result: %d" child2Result

        printfn "Subtasks completed"
        return ()
    }


    Async.RunSynchronously mainTask