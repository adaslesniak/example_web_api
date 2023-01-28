using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServicePlayground;

internal class WebClient
{
    readonly HttpClient comm = new();
    ILogger<WatchYourProducts> log;

    public WebClient(ILogger<WatchYourProducts> withLog) {
        log = withLog;
        comm.BaseAddress = Target(); ////TODO this must be read from settings
        comm.DefaultRequestHeaders.Accept.Clear();
        comm.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"text/plain"));
    }
    
    
    Uri Target() => new Uri(@"https://localhost:7243/");


    internal async Task UploadNewCsv(string csvData, Action<bool> callback) {
        var response = await comm.PostAsync("/products/csv", new StringContent(csvData));
        callback?.Invoke(response.IsSuccessStatusCode);
    }
}
