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

    downloadPage("https://www.google.com")|> Async.RunSynchronously