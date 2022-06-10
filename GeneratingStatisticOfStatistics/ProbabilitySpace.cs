namespace StatisticOfStatistics;
using System.Text.Json;
enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

record DnD_Dice_Probability_Space {
	public DiceType Type { get; }
	public int Dices { get; }
	public int Min { get; }
	public int Max { get; }
	public long PossibleOutcomes { get; }
	public List<long> Compositions { get; }
	public List<double> Probabilities { get; } = new()!;

	public DnD_Dice_Probability_Space(DiceType Type, int Dices, int Min, int Max, long PossibleOutcomes, List<long> Compositions, List<double> Probabilities) {
		this.Type = Type;
		this.Dices = Dices;
		this.Min = Min;
		this.Max = Max;
		this.PossibleOutcomes = PossibleOutcomes;
		this.Probabilities = Probabilities;
		this.Compositions = Compositions;
	}
}

class DnD_Dice_Probability_Calculator {
	readonly DiceType type;
	readonly int sides;
	readonly List<List<long>> probabilitySpace = new();
	readonly List<List<double>> probabilities = new();
	readonly List<long> possibleOutcomes = new();
	int min, max;

	public DnD_Dice_Probability_Calculator(DiceType t) {
		type = t;
		min = 1;
		max = (int)type;
		sides = max;

		// dummies:
		for (int i = 0; i <= 10; i++) {
			probabilitySpace.Add(new List<long>());
			probabilities.Add(new List<double>());
		}
		possibleOutcomes.Add(0);

		// inception of 1 dice space:
		probabilitySpace[1].Add(0); // dummy
		for (int i = 0; i < sides; i++)
			probabilitySpace[1].Add(1); // pocatek
		min++;
		max += sides;
	}

	// 1. ( j - k ) > 0
	// 2. ( j - k ) <=  (max - sides)
	// 3. if both are true : compositions += probSpace[dices-1][j-k]
	public void CalculateCompositions() {
		for (int dices = 2; dices < 10; dices++) {
			for (int dummy = 0; dummy < min; dummy++)
				probabilitySpace[dices].Add(0); // fill with dummy zeroes for code simplicity

			for (int j = min; j <= max; j++) {
				long compositions = 0;
				for (int k = 1; k <= sides; k++) {
					int x = j - k;
					if (x > 0 && x <= (max - sides)) // 1. & 2.
						compositions += probabilitySpace[dices - 1][x]; // 3.
				}
				probabilitySpace[dices].Add(compositions);
			}
			min++;
			max += sides;
		}
	}

	public void CalculateProbabilities() {
		long pOutcomes = sides;
		for (int dices = 1; dices < 10; dices++) {
			double d_possibleOutcomes = pOutcomes;
			foreach (long c in probabilitySpace[dices])
				probabilities[dices].Add(c / d_possibleOutcomes);
			possibleOutcomes.Add(pOutcomes);
			pOutcomes *= sides;
		}
	}

	public List<string> SerializeResult() {
		List<string> results = new();
		JsonSerializerOptions options = new() { WriteIndented = true };
		for (int dices = 1; dices < 10; dices++) {
			DnD_Dice_Probability_Space ps = new(type, dices, dices, dices * (int)type, possibleOutcomes[dices], probabilitySpace[dices], probabilities[dices]);
			string json = JsonSerializer.Serialize(ps, options);
			using StreamWriter writer = new($"./Rolls-Summary-Dynamic-Programming-{dices}d{(int)type}.json");
			writer.Write(json);
			results.Add(json);
		}
		return results;
	}
}