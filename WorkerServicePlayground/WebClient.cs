using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServicePlayground;

internal class WebClient
{
    readonly HttpClient comm = new();

    internal WebClient() {
        comm.BaseAddress = Target(); ////TODO this must be read from settings
        comm.DefaultRequestHeaders.Accept.Clear();
        comm.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"*/*"));
    }
    
    
    Uri Target() => new Uri(@"https://localhost:7243/");


    internal async Task UploadNewCsv(string csvData, Action inCaseOfProblems) {
        var response = await comm.PostAsync("/products/csv", new StringContent(csvData));
        if(false == response.IsSuccessStatusCode) {
            inCaseOfProblems?.Invoke();
        }

    }
}
