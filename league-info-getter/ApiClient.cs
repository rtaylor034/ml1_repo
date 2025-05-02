namespace league_info_getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ApiClient(string apiKey)
{
    private readonly string _apiKey = apiKey;
    private readonly HttpClient _client = new()
    {
        BaseAddress = new("https://americas.api.riotgames.com")
    };
    public async Task<string> Get(string path, params (string key, string value)[] query)
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(2000);
        var token = tokenSource.Token;
        try
        {
            string queries = string.Concat(query.Select(q => $"&{q.key}={q.value}"));
            string uri = $"{path}?api_key={_apiKey}{queries}";
            Console.WriteLine("> REQUESTING: " + uri);
            using HttpResponseMessage response = await _client.GetAsync(uri, token);

            var o = await response.Content.ReadAsStringAsync(token);
            Console.WriteLine("> GOT RESPONSE: " + response.IsSuccessStatusCode);

            return o;
        } catch (Exception e)
        {
            Console.WriteLine("[!] EXCEPTION IN REQUEST: " + e);
            return "";
        }
        
    }

}
