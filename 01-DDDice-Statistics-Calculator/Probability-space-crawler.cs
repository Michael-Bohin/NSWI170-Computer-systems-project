namespace DnD_Probability_Space;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Math;
public enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

public class DnD_Dice_Probability_Space_Crawler {
	//
	// >>> these data are important for json serializer and catch all results of space crawl <<<
	//
	[JsonInclude]
	public readonly int Dices, DiceMaxVal, MinRollSum, MaxRollSum;
	public DiceType DiceType { get; }
	public ulong PossibleOutcomes { get; }
	// calculated after constructor:
	public List<ulong> CompositionsSubtotals { get; } = new();
	public List<double> SumProbability { get; } = new();
	public ulong AllCompositions { get; private set; }
	//
	// >>> end of serializer relevant data <<<
	//
	int targetSum;
	readonly double d_possibleOutcomes; // reasonable to also save as double, as it would be recasted from long to double many times in probs calculation...
	readonly ulong[] factorial = new ulong[] { 1, 1, 2, 6, 24, 120, 720, 5_040, 40_320, 362_880 };
	readonly ulong citatel;

	public DnD_Dice_Probability_Space_Crawler(int Dices, DiceType DiceType) {
		this.Dices = Dices;
		this.DiceType = DiceType;

		MinRollSum = Dices;
		DiceMaxVal = (int)DiceType;
		MaxRollSum = Dices * DiceMaxVal;
		d_possibleOutcomes = Pow(DiceMaxVal, Dices);
		PossibleOutcomes = (ulong)d_possibleOutcomes;
		citatel = factorial[Dices];

		for (int i = 0; i <= MaxRollSum; i++) {
			CompositionsSubtotals.Add(0);
			SumProbability.Add(0);
		}
	}

	[JsonConstructor]
	public DnD_Dice_Probability_Space_Crawler(int Dices, DiceType DiceType, List<ulong> CompositionsSubtotals, List<double> SumProbability, ulong AllCompositions) {
		this.Dices = Dices;
		this.DiceType = DiceType;
		this.CompositionsSubtotals = CompositionsSubtotals;
		this.SumProbability = SumProbability;
		this.AllCompositions = AllCompositions;

		MinRollSum = Dices;
		DiceMaxVal = (int)DiceType;
		MaxRollSum = Dices * DiceMaxVal;
		d_possibleOutcomes = Pow(DiceMaxVal, Dices);
		PossibleOutcomes = (ulong)d_possibleOutcomes;
		citatel = factorial[Dices];
	}

	// and for each partition compute number of its compositions...
	// only count rollSum with same compositions count once (symetrie)
	public void SearchPartitions() { 
		int upTo = MinRollSum / 2 + MaxRollSum / 2;
		for (int i = MinRollSum; i <= upTo; i++) {
			targetSum = i; // save the target sum here, so that recursive calls of search dont need to pass the value..
			SearchPartitions(new List<int>(), i, 1);
			AllCompositions += CompositionsSubtotals[targetSum];
			SumProbability[i] = (double)CompositionsSubtotals[i] / d_possibleOutcomes;
		}
	}

	void SearchPartitions(List<int> list, int remaining, int searched) {
		if (remaining == 0) {                               // partition in list now contains sum of target sum of initial call
			if (list.Count != Dices)                        // assert that partition contains correct ammount of parts
				return;

			CompositionsSubtotals[targetSum] += CalculateCompositions(list);

		} else {
			if (list.Count == Dices)                        // if count of parts equals count of dices, but sum has not been reached (remaining != 0), this branch will not get there...
				return;
			int limit = Min(DiceMaxVal, remaining);        // minimum from ( "max dice value" or "remaining to targetSum" )

			for (int i = searched; i <= limit; i++) {      // foreach next part
				list.Add(i);
				SearchPartitions(list, remaining - i, i);   // recurcsively call self with extended list
				list.RemoveAt(list.Count - 1);
			}
		}
	}

	// make use of the fact that the way the recursion search
	// is written guarantees that parts are sorted in nondecreasing manner
	ulong CalculateCompositions(List<int> parts) {
		List<int> repetitions = new();
		int current = 1, count = 0;
		foreach (int part in parts) {
			if(part == current) {
				count++;
			} else {
				if(count > 1) 
					repetitions.Add(count);
				count = 1;
				current = part;
			}
		}

		if(count > 1)
			repetitions.Add(count);

		if (repetitions.Count == 0) // case no repetitions -> variation is permutation
			return citatel;

		ulong result = citatel;
		foreach(int rep in repetitions) 
			result /= factorial[rep];
		return result;
	}

	public void Sum_Compositions_Probabilities() {
		// copy raw results: number of compositions and probability to its symetric rollSums..
		int x = MinRollSum;
		int y = MaxRollSum;
		while (x < y) {
			CompositionsSubtotals[y] = CompositionsSubtotals[x];
			AllCompositions += CompositionsSubtotals[x];
			SumProbability[y] = SumProbability[x];
			x++;
			y--;
		}
		// assert allComositions == possibleOutcomes, if not, the code is incorrect -> kill the process
		if (AllCompositions != PossibleOutcomes)
			throw new Exception($"All compositions and possible outcomes are not the same at the end of probability space crawl!\n There is critical error in code.\n {AllCompositions} != {PossibleOutcomes}, dices: {Dices}, diceType: {DiceType}");
	}

	public void PrintAndSerialize() {
		JsonSerializerOptions options = new() { WriteIndented = true };
		string json = JsonSerializer.Serialize(this, options);

		using (StreamWriter sw = new($"./{Dices}d{(int)DiceType}-Rolls-Summary-Crawler.json"))
			sw.Write(json);
	}
}