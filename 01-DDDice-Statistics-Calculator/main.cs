namespace DnD_Probability_Space;

class Program {
    static void Main() {
        for (uint dices = 1; dices < 10; dices++) {
            Partition.ResetCachedVariations(dices); // reset variation equation results in cache
            foreach (DiceType type in Enum.GetValues<DiceType>()) {
                DnD_Dice_Probability_Space_Crawler crawler = new(dices, type);
                crawler.SearchPartitions();
                crawler.Sum_Compositions_Probabilities();
                crawler.Print();
            }
        }
    }
}