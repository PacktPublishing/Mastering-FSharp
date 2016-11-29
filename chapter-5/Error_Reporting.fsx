let error_supervisor =
       MailboxProcessor<System.Exception>.Start(fun inbox ->
         async { while true do
                   let! err = inbox.Receive()
                   printfn "an error '%A' occurred in an agent" err })

let agent =
  new MailboxProcessor<int>(fun inbox ->
         async { while true do
                   let! msg = inbox.Receive()
                   if msg % 100 = 0 then
                       failwith "I don't like that cookie!" })

    agent.Error.Add(fun error -> error_supervisor.Post error)
    agent.Start()