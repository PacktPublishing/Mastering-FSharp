open System
open System.IO
open System.Net

let testAwaitIAsyncResult (url: string) =
    async {
        let req = HttpWebRequest.Create(url)
        let aResp = req.BeginGetResponse(null, null)
        let! asyncResp = Async.AwaitIAsyncResult(aResp, 1000)
        if asyncResp then
            let resp = req.EndGetResponse(aResp)
            use respStream = resp.GetResponseStream()
            use sr = new StreamReader(respStream)
            return sr.ReadToEnd()
        else return ""
    }


     Async.RunSynchronously (testAwaitIAsyncResult
    "https://www.google.com")