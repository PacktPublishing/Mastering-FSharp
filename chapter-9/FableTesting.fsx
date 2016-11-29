#r "../node_modules/fable-core/Fable.Core.dll"
#r "../packages/NUnit/lib/net45/nunit.framework.dll"

open System
open Fable.Core
open Fable.Import
open NUnit.Framework

// If you don’t want to add the NUnit dependency
// (it is not needed for JS) you can comment out
// the previous two lines and open the following namespace

// open Fable.Core.Testing

[<TestFixture>]
module MyTests =

  // Convenience method
  let equal (expected: 'T) (actual: 'T) =
      Assert.AreEqual(expected, actual)

  [<Test>]
  let ``Structural comparison with arrays works``() =
    let xs1 = [| 1; 2; 3 |]
    let xs2 = [| 1; 2; 3 |]
    let xs3 = [| 1; 2; 4 |]
    equal true (xs1 = xs2)
    equal false (xs1 = xs3)
    equal true (xs1 <> xs3)
    equal false (xs1 <> xs2)