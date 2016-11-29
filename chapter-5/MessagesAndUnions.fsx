type Calculator =
        | Add of int * int
        | Multiply of int * int

    let calculatorAgent =
        MailboxProcessor<Calculator>.Start(fun inbox ->
            let rec loop() = async {
                let! msg = inbox.Receive()
                match msg with
                | Add(x, y) ->
                    printfn "Add - %d" (x + y)
                    return! loop()

                | Multiply(x, y) ->
                    printfn "Multi - %d" (x * y)
                    return! loop()
            }

            loop()
        )

        calculatorAgent.Post(Add(10, 20))