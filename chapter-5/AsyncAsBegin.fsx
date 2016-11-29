  open System.IO
    open System.Net

    let downloadPage (url: string) =
        async {
            let req = HttpWebRequest.Create(url)
            use! resp = req.AsyncGetResponse()
            use respStream = resp.GetResponseStream()
            use sr = new StreamReader(respStream)
            return sr.ReadToEnd()
        }

     type Downloader() =
        let beginMethod, endMethod, cancelMethod =
            Async.AsBeginEnd downloadPage
        member this.BeginDownload(url, callback, state : obj) =
            beginMethod(url, callback, state)
        member this.EndDownload(ar) =
            endMethod ar
        member this.CancelDownload(ar) =
            cancelMethod(ar)

        downloadPage("https://www.google.com") |> Async.RunSynchronously