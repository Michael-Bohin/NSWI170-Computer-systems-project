namespace DnD_Probability_Space;

class Program {
    static void Main() {
		foreach (DiceType type in Enum.GetValues<DiceType>()) 
            for (uint dices = 1; dices < 10; dices++) {
                DnD_Dice_Probability_Space_Crawler crawler = new(dices, type);
                crawler.SearchPartitions();
                crawler.Sum_Compositions_Probabilities();
                crawler.Print();
            }
    }
}