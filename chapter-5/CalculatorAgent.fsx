type CalculatorMsg =
        | Add of int * int * AsyncReplyChannel<int>
        | Multiply of int * int * AsyncReplyChannel<int>

    type CalculatorAgent () =

        let agent =
            MailboxProcessor<CalculatorMsg>.Start(fun inbox ->
                let rec loop() = async {
                    let! msg = inbox.Receive()
                    match msg with
                    | Add(x, y, r) ->
                        let result = x + y
                        r.Reply(result)
                        return! loop()

                    | Multiply(x, y, r) ->
                        let result = x * y
                        r.Reply(result)
                        return! loop()
                }

                loop()
            )

        member this.Add(x: int, y: int): int =
            agent.PostAndReply(fun r -> CalculatorMsg.Add(x, y, r))

        member this.AddAsync(x: int, y: int): Async<int> =
            agent.PostAndAsyncReply(fun r -> CalculatorMsg.Add(x, y, r))

        member this.Multiply(x: int, y: int): int =
            agent.PostAndReply(fun r -> CalculatorMsg.Multiply(x, y, r))

        member this.MultiplyAsync(x: int, y: int): Async<int> =  // Changed this from Async:int to Async<int>
            agent.PostAndAsyncReply(fun r -> CalculatorMsg.Multiply(x, y, r))


            let c = new CalculatorAgent()
            c.Add(10, 20)
