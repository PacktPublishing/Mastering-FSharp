open System


type MyEvent(v : string) =
        inherit EventArgs()
        member this.Value = v;

    let testAwaitEvent (evt : IEvent<MyEvent>) = async {
        printfn "Before waiting"
        let! r = Async.AwaitEvent evt
        printfn "After waiting: %O" r.Value
        do! Async.Sleep(1000)
        return ()
        }

     let runAwaitEventTest () =
        let evt = new Event<Handler<MyEvent>, _>()
        Async.Start <| testAwaitEvent evt.Publish
        System.Threading.Thread.Sleep(3000)
        printfn "Before raising"
        evt.Trigger(null, new MyEvent("value"))
        printfn "After raising"

        runAwaitEventTest()


