namespace WorkerServicePlayground;

internal class WebClient
{
    readonly HttpClient comm = new();
    ILogger<FileWatch> log;

    public WebClient(ILogger<FileWatch> withLog) {
        log = withLog;
        comm.BaseAddress = Target(); ////TODO this must be read from settings
        comm.DefaultRequestHeaders.Accept.Clear();
        comm.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"text/plain"));
    }
    
    
    Uri Target() => new Uri(@"https://localhost:7243/");


    //that should handle multitries - if can't success try later
    internal async Task UploadNewCsv(string csvData, Action<bool> callback) {
        var response = await comm.PutAsync("/products/csv", new StringContent(csvData));
        callback?.Invoke(response.IsSuccessStatusCode);
    }
}
