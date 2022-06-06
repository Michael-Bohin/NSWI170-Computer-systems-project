namespace DnD_Probability_Space;
using static System.Math;
using static System.Console;
public enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

// this place could get significantly improved by precalculating same values of partition -> compositionCount to a table. 
// many partition will repeat the calculation to get the same result...
public record Partition {
	public List<uint> parts;
	public ulong compositions;
	static readonly uint[] factorial = new uint[] { 1, 1, 2, 6, 24, 120, 720, 5_040, 40_320, 362_880 };
	
	public Partition(List<uint> p, uint maxPart) { // tell partition constructor max value of any part
		parts = p;
		uint[] counts = new uint[maxPart + 1];
		foreach (int i in parts)
			counts[i]++;

		compositions = factorial[parts.Count];
		foreach (uint opakovani in counts)
			if (opakovani > 1)
				compositions /= factorial[opakovani];
	}
}

// Ways to speed up the crawler:
// 1. Pocet compositions je symetricky - druhou pulku nemusime pocitat, staci opsat vysledek
// 2. Pocitej vycet partitions pro ruzne soucty paralelne. Jejich vypocet je nezavisly.
// 3. Rozmysli si zpusob jak nekopirovat List<uint> v rekurzivnim hledani Searchpartitions.
// 4. Predpocitej si pocet partitions pro vsechny pocitane kombinace. 99% Vypocet variace s opakovanim hodnekrat opakuje.
// 5. Napis funkci ktera udela lookup vysledku
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
	List<List<Partition>> partitions = new();

	public DnD_Dice_Probability_Space_Crawler(uint d, DiceType dt) {
		dices = d;
		diceType = dt;

		minRollSum = dices;
		diceMaxVal = (uint)diceType;
		maxRollSum = dices * diceMaxVal;
		d_possibleOutcomes = Pow(diceMaxVal, dices);
		possibleOutcomes = (ulong)d_possibleOutcomes;

		for(uint i = 0; i <= maxRollSum; i++) {
			partitions.Add(new List<Partition>());
			compositionsSubtotals.Add(0);
			sum_probability.Add(0);
		}
	}

	public void SearchPartitions() { // and for each partition compute number of its compositions...
		for(uint i = minRollSum; i <= maxRollSum; i++) {
			targetSum = (int)i; // save the target sum here, so that recursive calls of search dont need to pass the value..
			SearchPartitions(new List<uint>(), i, 1);
		}
	}

	// most space (and alocation time) can be saved by figuring out how to make copying values of list redundant
	// this could be performed using trie data structure, however I will save time not implementing it...
	void SearchPartitions(List<uint> list, uint remaining, uint searched) {
		if (remaining == 0) {								// partition in list now contains sum of target sum of initial call
			if (list.Count != dices)						// assert that partition contains correct ammount of parts
				return;
			Partition p = new(list, diceMaxVal);			// partitions found, initiliaze it and it to partitions List
			partitions[targetSum].Add(p);

		} else {
			if (list.Count == dices)						// if count of parts equals count of dices, but sum has not been reached (remaining != 0), this branch will not get there...
				return;
			uint limit = Min(diceMaxVal, remaining);		// minimum from ( "max dice value" or "remaining to targetSum" )

			for (uint i = searched; i <= limit; i++) {		// foreach next part
				List<uint> next = new();					// initilize new List
				foreach (uint x in list)					// copy its values from parameter input
					next.Add(x);							
				next.Add(i);								// add the next part
				SearchPartitions(next, remaining - i, i);	// recurcsively call self with the new list
			}
		}
	}

	public void Sum_Compositions_Probabilities() {
		for(int i = 1; i <= maxRollSum; i++) {
			foreach(Partition p in partitions[i]) 
				compositionsSubtotals[i] += p.compositions;
			allCompositions += compositionsSubtotals[i];
			sum_probability[i] = (double) compositionsSubtotals[i] / d_possibleOutcomes;
		}

		// now assert allComositions == possibleOutcomes, if not, the code is incorrect -> kill the process
		if(allCompositions != possibleOutcomes)
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
			
			if(diceType == DiceType.d20 && dices > 6) 
				return; // detailed log files for these dice types are too large.

			sw.WriteLine("\n\n   >>> Detailed log <<< \n\n");

			// detailed log
			for (int i = 1; i < partitions.Count; i++) {
				if(compositionsSubtotals[i] == 0)
					continue;
				sw.WriteLine($"Target sum: {i}, has total of {compositionsSubtotals[i]} and probability: {sum_probability[i]}");
				sw.WriteLine("Partitions enumeration:");
				for (int j = 0; j < partitions[i].Count; j++) {
					foreach(uint part in partitions[i][j].parts)
						sw.Write($"{part} ");
					sw.WriteLine($" => compositions: {partitions[i][j].compositions}");
				}
				sw.WriteLine();
			}
		}
	}
}