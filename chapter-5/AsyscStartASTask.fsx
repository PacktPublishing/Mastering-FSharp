open System.Net
open System.IO

    let downloadPageAsTask (url: string) =
        async {
            let req = HttpWebRequest.Create(url)
            use! resp = req.AsyncGetResponse()
            use respStream = resp.GetResponseStream()
            use sr = new StreamReader(respStream)
            return sr.ReadToEnd()
        }
        |> Async.StartAsTask
     let task = downloadPageAsTask("http://www.google.com")

     printfn "Do some work"
     task.Wait()
     printfn "done"