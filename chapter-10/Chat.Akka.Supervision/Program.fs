open Akka.Actor
open Akka.FSharp

open System
open System.Collections.Generic

let sarahSentences = [
        "Hi everybody!"
        "It feels great to be here!"
        "I missed you all so much!"
        "I couldn't agree more with that."
        "Oh, just look at the time! I should be leaving..."
    ]

let johnSentences = [
        "Hmm, I didn't expect YOU to be here."
        "I must say, I don't feel very comfortable."
        "Is this room always so boring?"
        "I shouldn't be losing my time here."
    ]

type AdminMsg =
  | Talk of author: string * message: string
  | Enter of name: string * IActorRef
  | Leave of name: string

and UserMsg =
  | Message of author: string * message: string
  | AllowEntry
  | Expel

let actorSystem =
    Configuration.defaultConfig()
    |> System.create "my-system"

let admin = spawn actorSystem "chat-admin" (fun mailbox ->
    let users = Dictionary<string, IActorRef>()
    let post msg = for u in users.Values do u <! msg
    let rec messageLoop() = actor {
        let! msg = mailbox.Receive()
        match msg with
        | Enter (name, actorRef) ->
            if users.ContainsKey name |> not then
                post <| Message("Admin", sprintf "User %s entered the room" name)
                users.Add(name, actorRef)
                actorRef <! AllowEntry
        | Leave name ->
            if users.Remove(name) then
                post <| Message("Admin", sprintf "User %s left the room" name)
        | Talk(author, txt) ->
            post <| Message(author, txt)

        return! messageLoop()
    }
    messageLoop())

type UserState = OutOfTheRoom | InTheRoom | WaitingApproval

type RandomIntervention = RandomIntervention

// Define a custom exception that will be thrown by random users.
// F# exceptions are easier to declare and can be pattern matched. 
exception UserException of name: string

let makeRandomUser supervisor name sentences =
  spawn supervisor ("chat-member-"+name) (fun mailbox ->
    let rnd = System.Random()
    let sentencesLength = List.length sentences
    let rec msgGenerator() = async {
        do! rnd.Next(4000) |> Async.Sleep
        mailbox.Self <! RandomIntervention
        return! msgGenerator()
    }
    msgGenerator() |> Async.Start
    let rec messageLoop (state: UserState) = actor {
        let! msg = mailbox.Receive()
        match msg: obj with
        | :? UserMsg as msg ->
            match msg with
            | Message _ -> return! messageLoop state
            | AllowEntry -> return! messageLoop InTheRoom
            | Expel -> return! messageLoop OutOfTheRoom
        | :? RandomIntervention ->
            match state with
            | InTheRoom ->
                match rnd.Next(sentencesLength + 2) with
                | i when i < sentencesLength ->
                    admin <! Talk(name, sentences.[i])
                    return! messageLoop state
                | i when i = sentencesLength ->
                    UserException name |> raise // Throw the exception
                | _ ->
                    admin <! Leave name
                    return! messageLoop OutOfTheRoom
            | OutOfTheRoom ->
                admin <! Enter(name, mailbox.Self)
                return! messageLoop WaitingApproval
            | WaitingApproval ->
                return! messageLoop state
        | _ -> ()
    }
    messageLoop OutOfTheRoom
)

let randomUserSupervisor =
    spawnOpt actorSystem "random-user-supervisor"
        <| fun mailbox ->
            let randomUser1 = makeRandomUser mailbox "Sarah" sarahSentences
            let randomUser2 = makeRandomUser mailbox "John" johnSentences
            let rec messageLoop() = actor {
                let! msg = mailbox.Receive()
                // Do nothing as this supervisor won't receive messages.
                // In the case we need to forward the message to a child,
                // we can use Forward, as in `randomUser1.Forward(msg)` 
                return! messageLoop()
            }
            messageLoop()
        <| [ SupervisorStrategy(
                Strategy.OneForOne(function
                | UserException name ->
                    printfn "Resuming %s..." name
                    Directive.Resume
                | _ -> Directive.Escalate)) ]

type HumanMsg =
    | Input of string
    | Output

let makeHumanUser name =
    spawn actorSystem "chat-member-human" (fun mailbox ->
        let msgs = ResizeArray()
        let rec messageLoop() = actor {
            let! msg = mailbox.Receive()
            match msg: obj with
            | :? HumanMsg as msg ->
                match msg with
                | Input txt -> admin <! Talk(name, txt)
                | Output ->
                    let msgsCopy = msgs.ToArray()
                    msgs.Clear()
                    let sender = mailbox.Sender()
                    sender <! msgsCopy
            | :? UserMsg as msg ->
                match msg with
                | Message(author, txt) ->
                    sprintf "%-8s> %s" author txt |> msgs.Add
                | _ -> () // Ignore other messages for the human UserMsg
            | _ -> ()
            return! messageLoop()
        }
        admin <! Enter(name, mailbox.Self)
        messageLoop()
    )

[<EntryPoint>]
let main argv =
    printf "Type your name: "
    let name = Console.ReadLine()
    let humanUser = makeHumanUser name
    let rec consoleLoop(): Async<unit> = async {
        printf "> "
        let txt = Console.ReadLine()
        if System.String.IsNullOrWhiteSpace(txt) |> not then
            humanUser <! HumanMsg.Input txt
        // Wait a bit to receive your own messafe from the admin
        do! Async.Sleep 200
        // Get the messages stored by the humanUser actor
        let! msgs = humanUser <? HumanMsg.Output
        msgs |> Array.iter (printfn "%s")
        return! consoleLoop()
    }
    printfn "Type a message to send it to the chat and read others' interventions."
    printfn @"Leave the line blank to ""pass your turn""."
    consoleLoop() |> Async.RunSynchronously

    0 // return an integer exit code
