// Open modules in this order, as Akka.FSharp can shadow
// some members from Akka.Actor
open Akka.FSharp
open Akka.Actor

// Create remote deployment configuration from the system address
let remoteDeploy systemPath =
    let address =
        match ActorPath.TryParseAddress systemPath with
        | false, _ -> failwith "ActorPath address cannot be parsed"
        | true, a -> a
    Deploy(RemoteScope(address))

// When testing remotely, write the IP of **this system** instead of `localhost`
let config = Configuration.parse """
akka {
    actor {
        provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
    }
    remote.helios.tcp {
        transport-protocol = tcp
        hostname = localhost
        port = 9001
    }
}
"""

// When testing remotely, write the IP of **the remote system** instead of `localhost`
let [<Literal>] remoteSystemAddress = "akka.tcp://remote-system@localhost:7000"

// These literal tags allow some level of safety
// while keeping the messages serializable
module Msg =
    let [<Literal>] Talk = "Talk"
    let [<Literal>] Enter = "Enter"
    let [<Literal>] Input = "Input"

let makeHumanUser system (name: string) =
    spawne system "chat-member-human-remote"
        <@
            // Use this operator to make value boxing less verbose
            let (!) (x:obj) = box x
            fun mailbox ->
                let admin =
                    // Select the admin actor by its path
                    let path = remoteSystemAddress + "/user/chat-admin"
                    select path mailbox.Context.System
                let rec messageLoop(): Cont<string*obj list, unit> =
                    actor {
                        let! msg = mailbox.Receive()
                        match msg with
                        | Msg.Input, [txt] ->
                            // The message is just a tuple of a string
                            // and a list of serializable objects,
                            // which can be sent over the network
                            // without sharing any assembly.
                            admin <! (Msg.Talk, [!name; !txt])
                        | _ -> ()
                        return! messageLoop()
                    }
                admin <! (Msg.Enter, [!name; !mailbox.Self])
                messageLoop()
        @>
        // Serialize the code and deploy the actor in the remote system
        [ SpawnOption.Deploy(remoteDeploy remoteSystemAddress) ]

[<EntryPoint>]
let main _ =
    use system = System.create "local-system" config
    printf "Type your name: "
    let name = System.Console.ReadLine()
    let humanUser = makeHumanUser system name
    let rec consoleLoop(): Async<unit> = async {
        printf "> "
        let txt = System.Console.ReadLine()
        if System.String.IsNullOrWhiteSpace(txt) |> not then
            humanUser <! ("Input", [box txt])
        return! consoleLoop()
    }
    printfn "Type a message to send it to the chat."
    consoleLoop() |> Async.RunSynchronously
    0
