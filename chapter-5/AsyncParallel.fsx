open System.IO
open System.Net
open System

open Microsoft.FSharp.Control.WebExtensions

let downloadPage (url: string) =
        async {
            let req = HttpWebRequest.Create(url)
            use! resp = req.AsyncGetResponse()
            use respStream = resp.GetResponseStream()
            use sr = new StreamReader(respStream)
            return sr.ReadToEnd()
        }



 let parallel_download() =
        let sites = ["http://www.bing.com";
                     "http://www.google.com";
                     "http://www.yahoo.com";
                     "http://www.search.com"]

        let htmlOfSites =
            Async.Parallel [for site in sites -> downloadPage site ]
            |> Async.RunSynchronously
        printfn "%A" htmlOfSites


