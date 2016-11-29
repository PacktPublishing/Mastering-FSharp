open FSharp.Reflection

/// Active pattern to recognize both unions
/// or serialized (string * obj list) tuples
let (|TryUnion|_|) (x: obj): 'T option =
    match x with
    | null -> None
    | :? 'T as x -> Some x
    | :? (string*obj list) as union ->
        try
            let case, fields = union
            FSharpType.GetUnionCases(typeof<'T>)
            |> Seq.tryFind (fun uci -> uci.Name = case)
            |> Option.map (fun uci ->
                FSharpValue.MakeUnion(uci, List.toArray fields) |> unbox<'T>)
        with _ -> None
    | _ -> None

/// Helper function to convert a union in a tuple
/// that can be serialized and send remotely
let tuple (u: obj) =
    let uci, fields = FSharpValue.GetUnionFields(u, u.GetType())
    uci.Name, List.ofArray fields

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

open Akka.FSharp
open Akka.Actor
open System.Collections.Generic    

// When testing remotely, write the IP of **this system** instead of `localhost`
let config = Configuration.parse """
akka {  
    actor {
        provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
    }    
    remote.helios.tcp {
        transport-protocol = tcp
        hostname = localhost
        port = 7000                
    }
}
"""

type AdminMsg =
  | Talk of author: string * message: string
  | Enter of name: string * IActorRef
  | Leave of name: string

and UserMsg =
  | Message of author: string * message: string
  | AllowEntry
  | Expel

let makeAdmin system =
    spawn system "chat-admin" (fun mailbox ->
        let users = Dictionary<string, IActorRef>()
        let post msg = for u in users.Values do u <! msg
        let rec messageLoop() = actor {
            let! msg = mailbox.Receive()
            match msg: obj with
            | TryUnion (msg: AdminMsg) ->
                match msg with
                | Enter (name, actorRef) ->
                    if users.ContainsKey name |> not
                    then
                        let msg = sprintf "User %s entered the room" name
                        post <| Message("Admin", msg)
                        users.Add(name, actorRef)
                        actorRef <! tuple AllowEntry
                        msg
                    else sprintf "User %s is already in the room" name
                | Leave name ->
                    if users.Remove(name)
                    then
                        let msg = sprintf "User %s left the room" name
                        post <| Message("Admin", msg)
                        msg
                    else sprintf "User %s was not in the room" name
                | Talk(author, txt) ->
                    post <| tuple (Message (author, txt))                    
                    sprintf "%-8s> %s" author txt
                |> printfn "%s"
            | _ ->
                printfn "Unknown message received"
            return! messageLoop()
        }
        messageLoop())

type UserState = OutOfTheRoom | InTheRoom | WaitingApproval

type RandomIntervention = RandomIntervention

let makeRandomUser system (admin: IActorRef) name sentences =
  spawn system ("chat-member-"+name) (fun mailbox ->
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
        | TryUnion (msg: UserMsg) ->
            match msg with
            | Message _ -> return! messageLoop state
            | AllowEntry -> return! messageLoop InTheRoom
            | Expel -> return! messageLoop OutOfTheRoom
        | TryUnion (_: RandomIntervention) ->
            match state with
            | InTheRoom ->
                match rnd.Next(sentencesLength + 1) with
                | i when i < sentencesLength ->
                    admin <! Talk(name, sentences.[i])
                    return! messageLoop state
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

[<EntryPoint>]
let main _ = 
    // The remote system only listens for incoming connections.
    // It will receive actor creation request from local-system.
    use system = System.create "remote-system" config
    let admin = makeAdmin system
    let randomUser1 =
        makeRandomUser system admin "Sarah" sarahSentences
    let randomUser2 =
        makeRandomUser system admin "John" johnSentences
    System.Console.ReadLine() |> ignore
    0
