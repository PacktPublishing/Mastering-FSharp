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



    let syncContext = System.Threading.SynchronizationContext()
    let asyncDownloadPage(url) = async {
          do! Async.SwitchToContext(syncContext)
          let! result = downloadPage(url)
          return result
          }

         // textbox.Text <- result } // you need to create a windows project to include a textbox in the UI.

    asyncDownloadPage "http://www.google.com"
    |> Async.Ignore
    |> Async.Start
