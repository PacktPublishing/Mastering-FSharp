 open System
 open System.IO
    open System.Net

    let downloadPage (url: string) =
        async.Delay (fun() ->
        let req = HttpWebRequest.Create(url)
        async.Bind(req.AsyncGetResponse(), fun resp ->
            async.Using(resp, fun resp ->
                let respStream = resp.GetResponseStream()
                async.Using(new StreamReader(respStream), fun reader ->
                    async.Return(reader.ReadToEnd())
                )
            )
        )
    )

    downloadPage("https://www.google.com")
    |> Async.RunSynchronously