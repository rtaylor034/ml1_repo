namespace league_info_getter.Deserialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
public record LeagueMatch
{
    public MetadataObject Metadata { get; init; }
    public InfoObject Info { get; init; }
    public record MetadataObject
    {
        [JsonPropertyName("participants")]
        public List<string> ParticipantPUUIDs { get; init; }
        public string MatchID { get; init; }
    }
    public record InfoObject
    {
        public List<ParticipantObject> Participants { get; init; }
        public List<TeamObject> Teams { get; init; }
    }
    public record ParticipantObject
    {
        public string ChampionName { get; init; }
        public int ChampionID { get; init; }
        public int TeamID { get; init; }
    }
    public record TeamObject
    {
        public bool Win { get; init; }
        public int TeamID { get; init; }
    }
}
