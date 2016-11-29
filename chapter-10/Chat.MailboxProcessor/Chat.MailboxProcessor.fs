module Chat.MailboxProcessor

open System
open System.Collections.Generic

type Actor<'T> = MailboxProcessor<'T>

type AdminMsg =
  | Talk of author: string * message: string
  | Enter of name: string * Actor<UserMsg>
  | Leave of name: string

and UserMsg =
  | Message of author: string * message: string
  | AllowEntry
  | Expel

let admin = Actor<AdminMsg>.Start(fun actor ->
  // Keep the list of users in the chat room
  let users = Dictionary<string, Actor<UserMsg>>()
  // ...and use a function helper to post a message to all of them
  let post msg = for u in users.Values do u.Post(msg)

  // Use an asynchronous recursive to represent the non-blocking loop
  let rec messageLoop() = async {
      let! msg = actor.Receive() // Wait until a message comes
      match msg with
      | Enter (name, actorRef) ->
          // For simplicity, just ignore duplicated users
          if users.ContainsKey name |> not then
              post <| Message("Admin", sprintf "User %s entered the room" name)
              users.Add(name, actorRef)
              actorRef.Post(AllowEntry)
      | Leave name ->
          if users.Remove(name) then
              post <| Message("Admin", sprintf "User %s left the room" name)
      | Talk(author, txt) ->
          post <| Message(author, txt)

      return! messageLoop() // Loop to top
    }

  messageLoop() // Fire up the loop
)

let makeMiddleAgent f = Actor.Start(fun actor ->
      let rec messageLoop() = async {
          let! msg = actor.Receive()
          f msg
          return! messageLoop()
      }
      messageLoop())

type UserState = OutOfTheRoom | InTheRoom | WaitingApproval

type [<RequireQualifiedAccess>] RndUserMsg =
    | RandomIntervention
    | UserMsg of UserMsg

let makeRandomUser name sentences = Actor.Start(fun actor ->
    let rnd = System.Random()
    let sentencesLength = List.length sentences

    let middleAgent = makeMiddleAgent(fun msg ->
        actor.Post(RndUserMsg.UserMsg msg))

    let rec msgGenerator() = async {
        do! rnd.Next(4000) |> Async.Sleep
        actor.Post(RndUserMsg.RandomIntervention)
        return! msgGenerator()
    }
    msgGenerator() |> Async.Start

    let rec messageLoop (state: UserState) = async {
        let! msg = actor.Receive()
        match msg with
        // Ignore messages from other users
        | RndUserMsg.UserMsg (Message _) -> return! messageLoop state
        | RndUserMsg.UserMsg AllowEntry -> return! messageLoop InTheRoom
        | RndUserMsg.UserMsg Expel -> return! messageLoop OutOfTheRoom
        | RndUserMsg.RandomIntervention _ ->
            match state with
            | InTheRoom ->
                // Pick a random sentence or leave the room
                match rnd.Next(sentencesLength + 1) with
                | i when i < sentencesLength ->
                    admin.Post(Talk(name, sentences.[i]))
                    return! messageLoop state
                | _ ->
                    admin.Post(Leave name)
                    return! messageLoop OutOfTheRoom
            | OutOfTheRoom ->
                admin.Post(Enter(name, middleAgent))
                return! messageLoop WaitingApproval
            | WaitingApproval ->
                return! messageLoop state // Do nothing, just keep waiting
    }
    // Start the loop with initial state
    messageLoop OutOfTheRoom
)

let randomUser1 =
    makeRandomUser "Sarah" [
        "Hi everybody!"
        "It feels great to be here!"
        "I missed you all so much!"
        "I couldn't agree more with that."
        "Oh, just look at the time! I should be leaving..."
    ]

let randomUser2 =
    makeRandomUser "John" [
        "Hmm, I didn't expect YOU to be here."
        "I must say, I don't feel very comfortable."
        "Is this room always so boring?"
        "I shouldn't be losing my time here."
    ]

type [<RequireQualifiedAccess>] HumanMsg =
    | Input of string
    | Output of AsyncReplyChannel<string[]>
    | UserMsg of UserMsg

let makeHumanUser name = Actor.Start(fun actor ->
    let msgs = ResizeArray()
    let rec messageLoop() = async {
        let! msg = actor.Receive()
        match msg with
        | HumanMsg.Input txt -> admin.Post(Talk(name, txt))
        | HumanMsg.Output reply ->
            let msgsCopy = msgs.ToArray()
            msgs.Clear()
            reply.Reply(msgsCopy)
        | HumanMsg.UserMsg(Message(author, txt)) ->
            sprintf "%-8s> %s" author txt |> msgs.Add
        | _ -> () // Ignore other messages for the human user
        return! messageLoop()
    }
    let middleAgent = makeMiddleAgent(fun msg ->
        actor.Post(HumanMsg.UserMsg msg))
    // Put the human user (actually, the middle agent) directly in the room
    admin.Post(Enter(name, middleAgent))
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
            humanUser.Post(HumanMsg.Input txt)
        // Wait a bit to receive your own messafe from the admin
        do! Async.Sleep 200
        // Get the messages stored by the humanUser actor
        let! msgs = humanUser.PostAndAsyncReply(fun replyChannel -> HumanMsg.Output replyChannel)
        msgs |> Array.iter (printfn "%s")
        return! consoleLoop()
    }
    printfn "Type a message to send it to the chat and read others' interventions."
    printfn @"Leave the line blank to ""pass your turn""."
    consoleLoop() |> Async.RunSynchronously

    0 // return an integer exit code
