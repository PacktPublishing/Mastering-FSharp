open System
open Microsoft.FSharp.Control

type Transaction =
    | Begin
    | End

type TransactionState =
    {
        mutable isInTransaction : bool
    }

type Agent<'T> = MailboxProcessor<'T> // Added this to define agent.

let transactionAgent = Agent<Transaction>.Start(fun inbox ->
        async {
            let tState : TransactionState  = { isInTransaction = false }
            while true do
                let! msg = inbox.Receive()
                match msg with
                | Begin ->
                    tState.isInTransaction <- true
                    printfn "begin transaction"
                | End ->
                    tState.isInTransaction <- false
                    printfn "end transaction"
        })

         transactionAgent.Post(Begin)
          transactionAgent.Post(End)