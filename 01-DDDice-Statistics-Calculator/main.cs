namespace DnD_Probability_Space;
using System.Diagnostics;

class Program {
    static void Main() {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        for (int dices = 1; dices < 10; dices++) {
            // Partition.ResetCachedVariations(dices); // reset variation equation results in cache
            foreach (DiceType type in Enum.GetValues<DiceType>()) {
                DnD_Dice_Probability_Space_Crawler crawler = new(dices, type);
                if(type == DiceType.d4)
                    crawler.ResetCachedVariations(dices);
                crawler.SearchPartitions();
                crawler.Sum_Compositions_Probabilities();
                crawler.PrintAndSerialize();
            }
            Console.WriteLine($"Finnished dices count: {dices}");
        }
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds/1000} seconds and {sw.ElapsedMilliseconds % 1000} milis");
    }
}