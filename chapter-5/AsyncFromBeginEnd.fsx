open System.Net
open System.IO
let downloadPageBeginEnd (url: string) =
        async {
            let req = HttpWebRequest.Create(url)
            use! resp = Async.FromBeginEnd(req.BeginGetResponse,req.EndGetResponse)
            use respStream = resp.GetResponseStream()
            use sr = new StreamReader(respStream)
            return sr.ReadToEnd()
        }

        downloadPageBeginEnd("https://www.google.com") |> Async.RunSynchronously