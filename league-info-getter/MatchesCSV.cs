namespace league_info_getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// stupid
public record MatchesCSV
{
    public Row[] Rows { get; private init; }
    public string[] ColumnNames { get; private init; }
    private MatchesCSV()
    {

    }
    private static bool ParseToBool(string value)
    {
        return value switch
        {
            "0" => false,
            "1" => true,
            _ => throw new Exception($"{value} is not 0 or 1"),
        };
    }
    public static MatchesCSV? ReadFrom(string path)
    {
        var textRows = File.ReadAllLines(path);
        if (textRows.Length == 0) return null;
        var rows = new Row[textRows.Length - 1];
        var columns = textRows[0].Split(',');
        int teamOffset = (columns.Length - 2) / 2;
        var allyCols = columns[0..teamOffset];
        var enemyCols = columns[teamOffset..^2];
        for (int i = 1; i < textRows.Length; i++)
        {
            var rowVals = textRows[i].Split(',');
            var allyVals = rowVals[..teamOffset];
            var enemyVals = rowVals[teamOffset..^2];
            rows[i - 1] =
                new()
                {
                    AllyChampions =
                        allyCols.Zip(allyVals)
                            .Select(x => KeyValuePair.Create(x.First, ParseToBool(x.Second)))
                            .ToArray(),
                    EnemyChampions =
                        enemyCols.Zip(enemyVals)
                            .Select(x => KeyValuePair.Create(x.First, ParseToBool(x.Second)))
                            .ToArray(),
                    MatchID = rowVals[^2],
                    Won = ParseToBool(rowVals[^1])
                };
        }
        return new()
        {
            Rows = rows,
            ColumnNames = columns,
        };
    }
    public record Row
    {
        public KeyValuePair<string, bool>[] AllyChampions { get; init; }
        public KeyValuePair<string, bool>[] EnemyChampions { get; init; }
        public string MatchID { get; init; }
        public bool Won { get; init; }
    }
}