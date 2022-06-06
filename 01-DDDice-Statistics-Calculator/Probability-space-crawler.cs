namespace DnD_Probability_Space;
using static System.Math;
public enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

public record Partition {
	public List<uint> parts = new();
	public ulong compositions;

	void Compositions(List<uint> partition) {
		// .. write your code here ..



		compositions = 1;
	}
}

public record RollSum {
	public uint sum;
	public List<Partition> partitions = new();
	public ulong totalCompositions;
	public double probability; // = totalCompositions / possibleOutcomes
}

public class DnD_Dice_Probability_Space_Crawler {
	//
	// >>> these data are important for json serializer and catch all results of space crawl <<<
	//
	public readonly uint dices;
	public readonly DiceType diceType;
	public readonly uint minRollSum, maxRollSum; // defines inclusive range of possible roll outcomes
	public readonly ulong possibleOutcomes;
	public ulong allCompositions; // sum of totalCompositions of all roll sums, used as double check of correctness. Must be equal to possible Outcomes.
	public List<RollSum> rollSums_data = new();

	//
	// >>> end of serializer relevant data <<<
	//

	// Factorials up to 9! are needed to calculate number of compositions.
	// This assumes we will be working with rolls with at most 9 dices. 
	// For ten dices calculation this would result in out of bounds exception.
	readonly uint[] factorial = new uint[10]; 

	public DnD_Dice_Probability_Space_Crawler(uint d, DiceType dt) {
		dices = d;
		diceType = dt;

		// precalculate values of first ten factorials:
		factorial[0] = 1;
		for (uint i = 1; i < 10; i++) 
			factorial[i] = factorial[i-1] * i;

		minRollSum = dices;
		uint diceMaxVal = (uint)diceType;
		maxRollSum = dices * diceMaxVal;
		possibleOutcomes = (ulong) Pow(diceMaxVal, dices);
	}

	public void SearchPartitions() { // and for each partition compute number of its compositions...
		// .. write your code here ..

	}

	
}