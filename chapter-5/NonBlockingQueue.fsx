 open System.Collections.Generic
    open System.Threading

    type internal Action<'T> =
        | Put of 'T * AsyncReplyChannel<unit>
        | Get of AsyncReplyChannel<'T>
        | Count of AsyncReplyChannel<int>


    type NonBlockingQueue<'T>() =
        let cts = new CancellationTokenSource()

        let mbox = MailboxProcessor<Action<'T>>.Start(fun inbox ->
                        let rec loop(queue: Queue<'T>) = async {
                            let! msg = inbox.Receive()
                            match msg with
                            | Put(v, r) ->
                                queue.Enqueue(v)
                                r.Reply(())
                                return! loop queue

                            | Get(r) ->
                                queue.Dequeue()
                                |> r.Reply
                                return! loop queue

                            | Count(r) ->
                                queue.Count
                                |> r.Reply
                                return! loop queue
                        }

                        loop (new Queue<'T>())
                   , cts.Token)

        member x.Enqueue(v) =
               mbox.PostAndReply(fun r -> Action.Put(v,r))

        member x.EnqueueAsync(v) =
               mbox.PostAndAsyncReply(fun r -> Action.Put(v, r))
        member x.Dequeue() =
               mbox.PostAndReply(fun r -> Action.Get(r))

        member x.DequeueAsync() =
               mbox.PostAndAsyncReply(fun r -> Action.Get(r))

        member x.Count = mbox.PostAndReply(Action.Count)

        interface System.IDisposable with
               member x.Dispose() = cts.Cancel()


        let bQueue = new NonBlockingQueue<string>()
        let r = new System.Random()

        let generate() =
                    let rec loop(i) = async {
                         if i < 10 then
                            bQueue.Enqueue(r.NextDouble() |> string)
                            printfn "Thread Id : %d" Thread.CurrentThread.ManagedThreadId
                            return! loop(i+1)
                     }

                    loop(0)

        generate() |> Async.Start
        bQueue.Count