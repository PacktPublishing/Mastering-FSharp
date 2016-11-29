type Transaction =
    | Begin
    | End

type TransactionState =
    {
        isInTransaction : bool
    }

let transactionAgent =
    MailboxProcessor<Transaction>.Start(fun inbox ->
            let rec loop (state: TransactionState) = async {
                let! msg = inbox.Receive()
                match msg with
                | Begin ->
                    printfn "begin transaction"
                    return! loop { state with isInTransaction = true }
                | End ->
                    printfn "end transaction"
                    return! loop { state with isInTransaction = false }
            }
              loop ({ isInTransaction = false })
        )

        transactionAgent.Post(Begin)
        transactionAgent.Post(End)