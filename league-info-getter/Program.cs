namespace league_info_getter;
using System;
using System.IO;
internal class Program
{
    private static readonly string[] EXPECTED_ARGS = ["Data Directory", "Root PUUID"];
    static async Task Main(string[] args)
    {
        bool main = true;
        if (main)
        {
            await MainRun(args);
        } else
        {
            await AuxRun(args);
        }
    }
    static async Task AuxRun(string[] args)
    {
        AuxilaryRunner aux = new();
        await aux.Run();
    }
    static async Task MainRun(string[] args)
    {
        Console.WriteLine("----[ RUN INFO ]----");
        Console.WriteLine($"IN: '{Environment.CurrentDirectory}'");
        Console.WriteLine("ARGS: ");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"[{i}] '{args[i]}'");
        }
        Console.WriteLine("--------------------");

        if (args.Length < EXPECTED_ARGS.Length)
        {
            Console.WriteLine("Expected arguements: ");
            for (int i = 0; i < EXPECTED_ARGS.Length; i++)
            {
                Console.WriteLine($"[{i}] <{EXPECTED_ARGS[i]}>");
            }
        }
        var dataDir =
            Path.IsPathFullyQualified(args[0])
                ? args[0]
                : Path.Combine(Environment.CurrentDirectory, args[0]);
        if (!Path.Exists(dataDir))
            throw new Exception($"'{dataDir}' does not exist.");
        Runner runner = new(dataDir, args[1]);
        await runner.Run();
    }
}
