namespace DnD_Probability_Space;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Math;
using static System.Console;
public enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

public record TrieNode {
	public TrieNode(ulong value) {
		val = value;
	}
	public ulong val;
	public Dictionary<int, TrieNode> children = new(); // for max repetition posssible at 9d20
}

public class CachedVariationsCount {
	public TrieNode root = new TrieNode(1);
	public ulong citatel = 1;
	static readonly ulong[] factorial = new ulong[] { 1, 1, 2, 6, 24, 120, 720, 5_040, 40_320, 362_880 };

	public void ResetCachedVariations(int nextDicesCount) { // always dices factorial
		citatel = factorial[nextDicesCount];
		root = new TrieNode(citatel); // if there is no repetition the number of compositions is simply dices factorial..
	}

	// assumes repettions are arriving cached
	public ulong GetVariation(List<int> repetitions) {
		if (repetitions.Count == 0) // case no repetitions -> variation is permutation
			return citatel;

		// 1. foreach repetition
		// 2.		check if next trie node exists, if it doesnt, instantiate it with proper value
		// 3.		set node to be he next node in trie path
		// 4. return value from node

		TrieNode node = root;
		foreach (int rep in repetitions) {
			if (!node.children.ContainsKey(rep))
				node.children[rep] = new TrieNode(node.val / factorial[rep]); // here the division is done only once -> in the implementation before it was repeated thousands of times
			node = node.children[rep];
		}

		return node.val;
	}
}

public class DnD_Dice_Probability_Space_Crawler {
	//
	// >>> these data are important for json serializer and catch all results of space crawl <<<
	//
	[JsonInclude]
	public readonly int Dices, DiceMaxVal, MinRollSum, MaxRollSum;
	public DiceType DiceType { get; }
	//ublic int DiceMaxVal { get; } 
	//public int MinRollSum { get; } // defines inclusive range of possible roll outcomes
	//public int MaxRollSum { get; }  // defines inclusive range of possible roll outcomes
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
	static readonly CachedVariationsCount variationsCache = new();
	// sum of totalCompositions of all roll sums, used as double check of correctness. Must be equal to possible Outcomes.

	public DnD_Dice_Probability_Space_Crawler(int Dices, DiceType DiceType) {
		this.Dices = Dices;
		this.DiceType = DiceType;

		MinRollSum = Dices;
		DiceMaxVal = (int)DiceType;
		MaxRollSum = Dices * DiceMaxVal;
		d_possibleOutcomes = Pow(DiceMaxVal, Dices);
		PossibleOutcomes = (ulong)d_possibleOutcomes;

		for (int i = 0; i <= MaxRollSum; i++) {
			CompositionsSubtotals.Add(0);
			SumProbability.Add(0);
		}
	}

	// will only providing parameterless constructor work? 
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
	}

	public void SearchPartitions() { // and for each partition compute number of its compositions...
									 // only count rollSum with same compositions count once (symetrie)
		int upTo = MinRollSum / 2 + MaxRollSum / 2;
		for (int i = MinRollSum; i <= upTo; i++) {
			targetSum = i; // save the target sum here, so that recursive calls of search dont need to pass the value..
			SearchPartitions(new List<int>(), i, 1);
			AllCompositions += CompositionsSubtotals[targetSum];
			SumProbability[i] = (double)CompositionsSubtotals[i] / d_possibleOutcomes;
		}
	}

	// most space (and alocation time) can be saved by figuring out how to make copying values of list redundant
	// this could be performed using trie data structure, however I will save time not implementing it...
	void SearchPartitions(List<int> list, int remaining, int searched) {
		if (remaining == 0) {                               // partition in list now contains sum of target sum of initial call
			if (list.Count != Dices)                        // assert that partition contains correct ammount of parts
				return;
			// Partition p = new(list, DiceMaxVal);            // partitions found, initiliaze it and add compositions count to total
			// CompositionsSubtotals[targetSum] += p.compositions;
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

	ulong CalculateCompositions(List<int> parts) {
		int[] counts = new int[DiceMaxVal + 1];
		foreach (int i in parts)
			counts[i]++;

		List<int> repetitions = new();
		foreach (int opakovani in counts)
			if (opakovani > 1)
				repetitions.Add(opakovani);

		if (repetitions.Count > 1)
			repetitions.Sort(); // this opp might make it not worth it..

		return variationsCache.GetVariation(repetitions);
	}

	public void ResetCachedVariations(int nextDicesCount) => variationsCache.ResetCachedVariations(nextDicesCount);

	public void Sum_Compositions_Probabilities() {
		// now copy raw results: number of compositions and probability to its symetric rollSums..
		int x = MinRollSum;
		int y = MaxRollSum;
		while (x < y) {
			CompositionsSubtotals[y] = CompositionsSubtotals[x];
			AllCompositions += CompositionsSubtotals[x];
			SumProbability[y] = SumProbability[x];
			x++;
			y--;
		}
		// now assert allComositions == possibleOutcomes, if not, the code is incorrect -> kill the process
		if (AllCompositions != PossibleOutcomes)
			throw new Exception($"All compositions and possible outcomes are not the same at the end of probability space crawl!\n There is critical error in code.\n {AllCompositions} != {PossibleOutcomes}, dices: {Dices}, diceType: {DiceType}");
	}

	public void PrintAndSerialize() {
		/*using (StreamWriter sw = new($"./{Dices}d{(int)DiceType}-Rolls-Summary-Crawler.txt")) {
			sw.WriteLine($"Dices: {Dices}, dice type: {DiceType}, possible combinations: {PossibleOutcomes}");
			// sum and probabilities of sums:
			for (int i = 1; i <= MaxRollSum; i++) {
				if (CompositionsSubtotals[i] == 0)
					continue;
				sw.WriteLine($"Roll sum: {i}, compositions: {CompositionsSubtotals[i]}, probability: {SumProbability[i]}");
			}
		}*/

		JsonSerializerOptions options = new() { WriteIndented = true };
		string json = JsonSerializer.Serialize(this, options);

		using (StreamWriter sw = new($"./{Dices}d{(int)DiceType}-Rolls-Summary-Crawler.json"))
			sw.Write(json);

	}
}



/*
public enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

public record TrieNode {
	public TrieNode(ulong value) {
		val = value;
	}
	public ulong val;
	public Dictionary<int, TrieNode> children = new (); // for max repetition posssible at 9d20
}

public class CachedVariationsCount {
	public TrieNode root = new TrieNode(1);
	public ulong citatel = 1;
	static readonly uint[] factorial = new uint[] { 1, 1, 2, 6, 24, 120, 720, 5_040, 40_320, 362_880 };

	public void ResetCachedVariations(uint nextDicesCount) { // always dices factorial
		citatel = factorial[nextDicesCount];
		root = new TrieNode(citatel); // if there is no repetition the number of compositions is simply dices factorial..
	}

	// assumes repettions are arriving cached
	public ulong GetVariation(List<int> repetitions) {
		if(repetitions.Count == 0) // case no repetitions -> variation is permutation
			return citatel;

		// 1. foreach repetition
		// 2.		check if next trie node exists, if it doesnt, instantiate it with proper value
		// 3.		set node to be he next node in trie path
		// 4. return value from node

		TrieNode node = root;
		foreach (int rep in repetitions) {
			if(!node.children.ContainsKey(rep))
				node.children[rep] = new TrieNode(node.val / factorial[rep]); // here the division is done only once -> in the implementation before it was repeated thousands of times
			node = node.children[rep];
		}

		return node.val;
	}
}

// this place could get significantly improved by precalculating same values of partition -> compositionCount to a table. 
// many partition will repeat the calculation to get the same result...
public class Partition {
	public List<uint> parts;
	public ulong compositions;
	static CachedVariationsCount variationsCache =  new();

	public Partition(List<uint> p, uint maxPart) { // tell partition constructor max value of any part
		parts = p;
		int[] counts = new int[maxPart + 1];
		foreach (int i in parts)
			counts[i]++;

		List<int> repetitions = new();
		foreach (int opakovani in counts)
			if (opakovani > 1)
				repetitions.Add(opakovani);

		if(repetitions.Count > 1)
			repetitions.Sort(); // this opp might make it not worth it..

		compositions = variationsCache.GetVariation(repetitions);
	}

	public static void ResetCachedVariations(uint nextDicesCount) { // always dices factorial
		variationsCache.ResetCachedVariations(nextDicesCount);
	}
}

// Ways to speed up the crawler:
// 1. Pocitej vycet partitions pro ruzne soucty paralelne. Jejich vypocet je nezavisly.

public class DnD_Dice_Probability_Space_Crawler {
	//
	// >>> these data are important for json serializer and catch all results of space crawl <<<
	//
	public readonly uint dices;
	public readonly DiceType diceType;
	public readonly uint diceMaxVal;
	public readonly uint minRollSum, maxRollSum; // defines inclusive range of possible roll outcomes
	public readonly ulong possibleOutcomes;
	public List<ulong> compositionsSubtotals = new();
	public List<double> sum_probability = new();
	
	//
	// >>> end of serializer relevant data <<<
	//
	int targetSum;
	readonly double d_possibleOutcomes; // reasonable to also save as double, as it would be recasted from long to double many times in probs calculation...
	ulong allCompositions; // sum of totalCompositions of all roll sums, used as double check of correctness. Must be equal to possible Outcomes.

	public DnD_Dice_Probability_Space_Crawler(uint d, DiceType dt) {
		dices = d;
		diceType = dt;

		minRollSum = dices;
		diceMaxVal = (uint)diceType;
		maxRollSum = dices * diceMaxVal;
		d_possibleOutcomes = Pow(diceMaxVal, dices);
		possibleOutcomes = (ulong)d_possibleOutcomes;

		for(uint i = 0; i <= maxRollSum; i++) {
			compositionsSubtotals.Add(0);
			sum_probability.Add(0);
		}
	}

	public void SearchPartitions() { // and for each partition compute number of its compositions...
		// only count rollSum with same compositions count once (symetrie)
		uint upTo = minRollSum / 2 + maxRollSum / 2;
		for(int i = (int)minRollSum; i <= upTo; i++) {
			targetSum = i; // save the target sum here, so that recursive calls of search dont need to pass the value..
			SearchPartitions(new List<uint>(), (uint)i, 1);
			allCompositions += compositionsSubtotals[targetSum];
			sum_probability[i] = (double)compositionsSubtotals[i] / d_possibleOutcomes;
		}
	}

	// most space (and alocation time) can be saved by figuring out how to make copying values of list redundant
	// this could be performed using trie data structure, however I will save time not implementing it...
	void SearchPartitions(List<uint> list, uint remaining, uint searched) {
		if (remaining == 0) {								// partition in list now contains sum of target sum of initial call
			if (list.Count != dices)						// assert that partition contains correct ammount of parts
				return;
			Partition p = new(list, diceMaxVal);			// partitions found, initiliaze it and add compositions count to total
			compositionsSubtotals[targetSum] += p.compositions;

		} else {
			if (list.Count == dices)						// if count of parts equals count of dices, but sum has not been reached (remaining != 0), this branch will not get there...
				return;
			uint limit = Min(diceMaxVal, remaining);		// minimum from ( "max dice value" or "remaining to targetSum" )

			for (uint i = searched; i <= limit; i++) {		// foreach next part
				list.Add(i);
				SearchPartitions(list, remaining - i, i);	// recurcsively call self with extended list
				list.RemoveAt(list.Count-1);
			}
		}
	}

	public void Sum_Compositions_Probabilities() {
		// now copy raw results: number of compositions and probability to its symetric rollSums..
		int x = (int)minRollSum;
		int y = (int)maxRollSum;
		while (x < y) {
			compositionsSubtotals[y] = compositionsSubtotals[x];
			allCompositions += compositionsSubtotals[x];
			sum_probability[y] = sum_probability[x];
			x++;
			y--;
		}
		// now assert allComositions == possibleOutcomes, if not, the code is incorrect -> kill the process
		if (allCompositions != possibleOutcomes)
			throw new Exception($"All compositions and possible outcomes are not the same at the end of probability space crawl!\n There is critical error in code.\n {allCompositions} != {possibleOutcomes}, dices: {dices}, diceType: {diceType}");
	}

	public void Print() {
		WriteLine($"Writting to file: ./d{(uint)diceType}/{dices}d{(uint)diceType}-Rolls-Summary-Crawler.txt");
		using (StreamWriter sw = new($"./d{(uint)diceType}/{dices}d{(uint)diceType}-Rolls-Summary-Crawler.txt")) {
			sw.WriteLine($"Dices: {dices}, dice type: {diceType}, possible combinations: {possibleOutcomes}");
			// sum and probabilities of sums:
			for(int i = 1; i <= maxRollSum; i++) {
				if(compositionsSubtotals[i] == 0)
					continue;
				sw.WriteLine($"Roll sum: {i}, compositions: {compositionsSubtotals[i]}, probability: {sum_probability[i]}");
			}
		}
	}

	public void Serialize() {
		var json = JsonSerializer.Serialize(this, DnD_Dice_Probability_Space_CrawlerJSONContext.Default.DnD_Dice_Probability_Space_Crawler);
		WriteLine(json.ToString());
	}
}*/