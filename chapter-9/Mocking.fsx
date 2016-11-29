#r "../packages/NUnit/lib/net45/nunit.framework.dll"
#r "../packages/Foq/lib/net45/Foq.dll"
#load "MyLib.fs"

open MyLib
open NUnit.Framework

let createCustomer name =
    { new ICustomer with
        member __.Name = name
        member __.HasBoughtItem(_) = failwith "Not implemented" }

open Foq

let createCustomer2 (name: string) =
    Mock<ICustomer>()
        // Foq Method
        .Setup(fun x -> <@ x.HasBoughtItem(1) @>).Returns(true)
        // Foq Matching Arguments
        .Setup(fun x -> <@ x.HasBoughtItem(any()) @>).Returns(true)
        // Foq Property
        .Setup(fun x -> <@ x.Name @>).Returns(name)
        .Create()

[<Test>]
let ``Customer names with spaces are not valid``() =
    let customer1 = createCustomer "John"
    let customer2 = createCustomer2 "Anne Mary"
    Assert.AreEqual(true, validateCustomer customer1)
    Assert.AreEqual(false, validateCustomer customer2)
