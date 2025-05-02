namespace league_info_getter;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

public class Runner
{
    public Runner(string dataDirectory, string rootPuuid)
    {
        DataDirectory = dataDirectory;
        RootPUUID = rootPuuid;
        _paths = new(dataDirectory);
        _serializeOpts = new(JsonSerializerDefaults.Web);
        _rand = new();
    }

    public string DataDirectory { get; }
    public string RootPUUID { get; }
    private PathStore _paths { get; }
    private JsonSerializerOptions _serializeOpts { get; }
    private Random _rand { get; }

    public ApiClient API { get; } =
        new(Environment.GetEnvironmentVariable("RIOT_API_KEY")!);

    public async Task Run()
    {
        var matchesCsv = MatchesCSV.ReadFrom(_paths.Matches);
        var registry = new MatchRegistry(File.ReadAllLines(_paths.ChampionList));
        if (matchesCsv is not null)
        {
            registry.NullifyMatchIds(matchesCsv.Rows.Select(x => x.MatchID));
        }
        try
        {
            await PopulateRegistry(registry, RootPUUID, 3, 3, 15);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[!] EXCEPTION: {e}");
        }
        var nRows = registry.ToCSVRows();
        File.AppendAllLines(_paths.Matches, nRows[(matchesCsv is null ? 0 : 1)..]);
        Console.WriteLine(">> END <<");
    }

    private async Task PopulateRegistry(MatchRegistry registry, string rootPuuid, int iterations, int playersPerIteration, int matchesPerPlayer)
    {
        var matches = await GetMatchesFromPlayer(rootPuuid, matchesPerPlayer);
        for (var i = 0; i <= iterations; i++)
        {
            matches = await GetNextMatches(matches, matchesPerPlayer, playersPerIteration);
            foreach (var match in matches)
            {
                registry.AddMatch(match);
            }
        }
    }

    private async Task<List<Deserialized.LeagueMatch>> GetMatchesFromPlayer(string playerPuuid, int matches)
    {
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;
        tokenSource.CancelAfter(1000);
        try
        {
            var o = new List<Deserialized.LeagueMatch>(matches);
            var matchesResponse =
                await API.Get(
                $"lol/match/v5/matches/by-puuid/{playerPuuid}/ids",
                ("count", matches.ToString()), ("start", _rand.Next(matches).ToString()), ("type", "ranked"));
            var matchIds =
                await DeserializeJson<List<string>>(matchesResponse, token);
            if (matchIds is null) return [];
            List<Task<Deserialized.LeagueMatch?>> matchRequests = [];
            foreach (var id in matchIds)
            {
                matchRequests.Add(RequestMatch(id));
            }
            var recievedMatches =
                (await Task.WhenAll(matchRequests));
            o.AddRange(recievedMatches.Where(x => x is not null)!);
            return o;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[!] EXCEPTION IN GETTING MATCHES: {e}");
            return [];
        }
    }

    private async Task<Deserialized.LeagueMatch?> RequestMatch(string id)
    {
        var matchResponse = await API.Get($"lol/match/v5/matches/{id}");
        try
        {
            var match = await DeserializeJson<Deserialized.LeagueMatch>(matchResponse, CancellationToken.None);
            return match;
        }
        catch
        {
            return null;
        }
    }

    private async Task<T?> DeserializeJson<T>(string json, CancellationToken token) where T : class
    {

        try
        {
            return await Task.Run(() => JsonSerializer.Deserialize<T>(json, _serializeOpts), token);
        } catch
        {
            return null;
        }
    }

    private async Task<List<Deserialized.LeagueMatch>> GetNextMatches(List<Deserialized.LeagueMatch> previousMatches, int matchesPerPlayer, int players)
    {
        var o = new List<Deserialized.LeagueMatch>(matchesPerPlayer * players);
        HashSet<int> indicies = new(players);
        while (indicies.Count < players)
        {
            var i = _rand.Next(previousMatches.Count);
            indicies.Add(i);
        }
        foreach (var i in indicies)
        {
            try
            {
                var match = previousMatches[i];
                if (!IsValidMatch(match)) continue;
                var ids = match.Metadata.ParticipantPUUIDs;
                var playerId = ids[1 + _rand.Next(ids.Count - 1)];
                o.AddRange(await GetMatchesFromPlayer(playerId, matchesPerPlayer));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[!] EXCEPTION IN BRANCHING: {e}");
            }
        }
        return o;
    }
    private bool IsValidMatch(Deserialized.LeagueMatch? match)
    {
        return match is not null && match.Info is not null && match.Metadata is not null;
    }
    private record PathStore
    {
        private readonly string _root;

        public PathStore(string root)
        {
            _root = root;
        }

        public string Matches => Path.Join(_root, "out/matches.csv");
        public string ChampionList => Path.Join(_root, "champion_list.txt");
    }
}