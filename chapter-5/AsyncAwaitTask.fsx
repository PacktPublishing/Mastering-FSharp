open System.Threading.Tasks
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

let testAwaitTask (t: Task<string>) =
    async {
        let! r = Async.AwaitTask t
        return r
    }

    downloadPageAsTask ("https://www.google.com")
  |> testAwaitTask
  |> Async.RunSynchronously
