namespace league_info_getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class AuxilaryRunner()
{
    public readonly HttpClient Client = new()
    {
        BaseAddress = new("https://wiki.leagueoflegends.com/en-us/List_of_champions")
    };
    public async Task Run()
    {
        using HttpResponseMessage response = await Client.GetAsync("");

        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{jsonResponse}\n");

    }
}
