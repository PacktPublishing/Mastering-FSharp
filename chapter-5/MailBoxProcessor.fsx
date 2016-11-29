let agent =
   MailboxProcessor.Start(fun inbox ->
     async { while true do
               let! msg = inbox.Receive()
               printfn "got message '%s'" msg } )


               agent.Post "Hello World"
