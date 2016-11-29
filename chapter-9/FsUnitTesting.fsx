#r "../packages/NUnit/lib/net45/nunit.framework.dll"
#r "../packages/FsUnit/lib/net45/FsUnit.NUnit.dll"
#load "MyLib.fs"

open MyLib
open NUnit.Framework

// Helper to make assertions more idiomatic in F#

let equals expected actual =
    Assert.AreEqual(expected, actual)

let ``Adding two plus two yields four``() =
    add 2 2
    |> equals 4

// Using helpers from FsUnit

open FsUnit

4.99 |> should (equalWithin 0.05) 5.0
4.0 |> should not' ((equalWithin 0.05) 5.0)
7 |> should greaterThan 3
7 |> should greaterThanOrEqualTo 3

