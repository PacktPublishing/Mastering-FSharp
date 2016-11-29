open System.Collections.Generic

let dictAgent =
    MailboxProcessor.Start(fun inbox ->
        async { let strings = Dictionary<string,int>()
                while true do
                let! msg = inbox.Receive()
                if strings.ContainsKey msg then
                    strings.[msg] <- strings.[msg] + 1
                else
                    strings.[msg] <- 0
                printfn "message '%s' now seen '%d' times" msg strings.[msg] } )


                    [ "Hello"; "World"; "Hello"; "John"; ]
                    |> List.iter (dictAgent.Post)