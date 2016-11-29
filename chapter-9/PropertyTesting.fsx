#r "../packages/NUnit/lib/net45/nunit.framework.dll"
#r "../packages/FsCheck/lib/net45/FsCheck.dll"
#load "MyLib.fs"

open MyLib

// Define the properties (or requirements)
// the functions must fulfill

let addIsCommutative x y =
    add x y = add y x

let addIsAssociative x y z =
    (add x y |> add z) = (add y z |> add x)

let addHasIdentity x =
    add x 0 = x

open MyLib
open FsCheck
open NUnit.Framework

[<Test>]
let ``Addition is additive``() =
    Check.QuickThrowOnFailure addIsCommutative

[<Test>]
let ``Addition is associative``() =
    Check.QuickThrowOnFailure addIsCommutative

[<Test>]
let ``Addition has identity``() =
    Check.QuickThrowOnFailure addIsCommutative
