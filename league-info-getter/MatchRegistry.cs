namespace league_info_getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MatchRegistry
{
    private const int _TEAM_A = 100;
    private const int _TEAM_B = 200;
    private readonly HashSet<string> _allChamps;
    private readonly string[] _champList;
    private readonly Dictionary<string, int> _champMap;
    private readonly int _teamOffset;
    private List<Match> _matches = new();
    private HashSet<string> _encounteredMatchIds = new();

    public MatchRegistry(IEnumerable<string> champNames)
    {
        var champs = champNames.Select(x => x.ToLower()).ToArray();
        _teamOffset = champs.Length;
        _champMap = new(champs.Length);
        for (var i = 0; i < champs.Length; i++)
        {
            _champMap[champs[i]] = i;
        }
        _allChamps = new(champs);
        _champList = champs;
    }

    public void NullifyMatchIds(IEnumerable<string> matchIds)
    {
        _encounteredMatchIds.UnionWith(matchIds);
    }
    public bool AddMatch(Deserialized.LeagueMatch match)
    {
        if (match is null) return false;
        try
        {
            if (match.Metadata is null) return false;
            Console.WriteLine("TRYADD: " + match.Metadata.MatchID);
            if (!_encounteredMatchIds.Add(match.Metadata.MatchID)) return false;
            HashSet<string> teamAChamps = new(5);
            HashSet<string> teamBChamps = new(5);
            foreach (var participant in match.Info.Participants)
            {
                var champ = participant.ChampionName.ToLower();
                if (!_allChamps.Contains(champ))
                    throw new($"Unknown champion name: '{champ}'");
                switch (participant.TeamID)
                {
                case _TEAM_A:
                    teamAChamps.Add(champ);
                break;
                case _TEAM_B:
                    teamBChamps.Add(champ);
                break;
                default:
                    throw new($"Unrecognized team ID: '{participant.TeamID}'");
                }
            }
            var firstTeam = match.Info.Teams[0];
            var aWon =
                firstTeam is
                    {
                        Win: true,
                        TeamID: _TEAM_A,
                    } or
                    {
                        Win: false,
                        TeamID: _TEAM_B,
                    };
            _matches.Add(
            new()
            {
                MatchID = match.Metadata.MatchID,
                TeamAChamps = teamAChamps,
                TeamBChamps = teamBChamps,
                TeamAWon = aWon,
            });
            Console.WriteLine(_matches[^1]);
            return true;
        } catch (Exception e)
        {
            Console.WriteLine($"[!] EXCEPTION (adding): {e}");
            return false;
        }
        
    }

    /// <summary>
    ///     first row is header.
    /// </summary>
    /// <returns></returns>
    public string[] ToCSVRows()
    {
        string[] o = new string[(_matches.Count * 2) + 1];
        string[] columns = new string[(_champList.Length * 2) + 2];
        for (var i = 0; i < _champList.Length; i++)
        {
            columns[i] = "ally_" + _champList[i];
            columns[i + _teamOffset] = "opponent_" + _champList[i];
        }
        columns[^1] = "win";
        columns[^2] = "match_id";
        o[0] = string.Join(',', columns);
        for (var i = 0; i < _matches.Count; i++)
        {
            int champDataLength = columns.Length - 2;
            var match = _matches[i];
            var allySection = new bool[champDataLength / 2];
            var enemySection = new bool[champDataLength / 2];
            foreach (var allyChamp in match.TeamAChamps)
            {
                allySection[_champMap[allyChamp]] = true;
            }
            foreach (var opponentChamp in match.TeamBChamps)
            {
                enemySection[_champMap[opponentChamp]] = true;
            }
            o[(i*2) + 1] = string.Join(',', allySection.Concat(enemySection).Select(x => x ? "1" : "0").Concat([match.MatchID, match.TeamAWon ? "1" : "0"]));
            o[(i*2) + 2] = string.Join(',', enemySection.Concat(allySection).Select(x => x ? "1" : "0").Concat([match.MatchID, match.TeamAWon ? "0" : "1"]));
        }
        return o;
    }

    private record Match
    {
        public required string MatchID { get; init; }
        public required HashSet<string> TeamAChamps { get; init; }
        public required HashSet<string> TeamBChamps { get; init; }
        public required bool TeamAWon { get; init; }
        public override string ToString()
        {
            string[] lines = [$"MATCH: {MatchID}", "-----", $"Allies: {ShowList(TeamAChamps)}", $"Enemies: {ShowList(TeamBChamps)}", $"Won: {TeamAWon}", "-----",];
            return string.Join("\n", lines);

        }
        private static string ShowList(IEnumerable<object> items)
        {
            return "[" + string.Join(", ", items.Select(x => x.ToString())) + "]";
        }
    }
}