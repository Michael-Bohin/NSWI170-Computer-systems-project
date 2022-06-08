namespace DnD_Dice_Probability_Space;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Math;

// input from expected model (only probs)
record RollSumStat {
	public int RollSum { get; set; }
	public double Probability { get; set; }
	public double Expected { get; set; }
	public long Observed { get; set; }
	public double Diff { get; set; }
	public double Chi { get; set; }

	public RollSumStat(int RollSum, double Probability) {
		this.RollSum = RollSum;
		this.Probability = Probability;
	}
}

// output of the experiment
record DnD_Dice_Statistic_Experiment {
	public DiceType DiceType { get; init; }
	public int Dices { get; init; }
	public long ThrowCount { get; init; }
	public double ChiSquareSum { get; init; }
	public double Alpha_0_1_critical { get; init; }
	public double Alpha_0_05_critical { get; init; }
	public double Alpha_0_001_critical { get; init; }
	public string Decision { get; init; } = default!;
	public List<RollSumStat> RollSums { get; init;} = new();

	public DnD_Dice_Statistic_Experiment(DiceType DiceType, int Dices, long ThrowCount, double ChiSquareSum, double Alpha_0_1_critical, double Alpha_0_05_critical, double Alpha_0_001_critical, string Decision, List<RollSumStat> RollSums) {
		this.DiceType = DiceType;
		this.Dices = Dices;
		this.ThrowCount = ThrowCount;
		this.ChiSquareSum = ChiSquareSum;
		this.Alpha_0_1_critical = Alpha_0_1_critical;
		this.Alpha_0_05_critical = Alpha_0_05_critical;
		this.Alpha_0_001_critical = Alpha_0_001_critical;
		this.Decision = Decision;
		this.RollSums = RollSums;
	}
}


enum ExperimentDecision { InvalidExperiment_ExpectedMinimumLessThan5, NotRejected, RejectedAtAlpha10, RejectedAtAlpha5, RejectedAtAlpha1 }

record ExperimentResult {
	public string DiceType { get; } = default!;
	public long Throws { get;}
	public double MinimumExpectedSum { get; } // in order to determine, whether the experiment can or cannot be carried out
	public double ChiSquareValue { get; }
	public int DegreesOfFreedom { get;}
	public double Alpha_10_critical { get; }
	public double Alpha_5_critical { get; }
	public double Alpha_1_critical { get; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ExperimentDecision Decision { get; }
	public double DurationSec { get; private set; }
	public double DurationMs { get; private set; }

	public ExperimentResult(string DiceType, long Throws, double MinimumExpectedSum, double ChiSquareValue, int DegreesOfFreedom, double Alpha_10_critical, double Alpha_5_critical, double Alpha_1_critical, ExperimentDecision Decision) {
		this.DiceType = DiceType;
		this.Throws = Throws;
		this.MinimumExpectedSum = MinimumExpectedSum;
		this.ChiSquareValue = ChiSquareValue;
		this.DegreesOfFreedom = DegreesOfFreedom;
		this.Alpha_10_critical = Alpha_10_critical;
		this.Alpha_5_critical = Alpha_5_critical;
		this.Alpha_1_critical = Alpha_1_critical;
		this.Decision = Decision;
	}

	public void SetDuration(long sec, long ms) { 
		DurationSec = sec;
		DurationMs = ms;
	} 
}

class DnD_Dice_Mass_Thrower {
	readonly Random rand = new();
	readonly DiceType diceType;
	readonly int dices, minVal, maxVal, exclusiveMaxVal, minRollSum, maxRollSum;
	readonly long throws;
	readonly List<RollSumStat> rollSums = new();
	readonly long[] observed; // keep separate from rollSums where interpretation happens to stress objectivity by seperating expected and observed numbers up to interpretation
	double chiSquareSum = 0;
	ExperimentResult expResult;
	// public static double arciSum = 0;

	public DnD_Dice_Mass_Thrower(DiceType diceType, int dices, long throws) {
		this.diceType = diceType;
		this.dices = dices;
		this.throws = throws;
		minVal = this.dices;
		maxVal = (int) this.diceType;
		exclusiveMaxVal = maxVal + 1;
		minRollSum = dices;
		maxRollSum = maxVal * dices;
		observed = new long[maxRollSum+1];
		chiSquareSum = 0;
	}

	public void GenerateStatistic() {
		int rollSum;
		for (int i = 0; i < throws; i++) {
			rollSum = 0;
			for (int j = 0; j < dices; j++)
				rollSum += rand.Next(1, exclusiveMaxVal);
			observed[rollSum]++;
		}
	}

	public void LoadModel(string jsonExpectedModel) {
		JsonSerializerOptions options = new() { WriteIndented = true };
		var probabilitySpace = JsonSerializer.Deserialize<DnD_Dice_Probability_Space>(jsonExpectedModel, options);
		for(int i = 0; i < probabilitySpace.Probabilities.Count; i++)  {
			RollSumStat rss = new(i, probabilitySpace.Probabilities[i]);
			rollSums.Add(rss);
		} // if(probabilitySpace.Probabilities[i] != 0)
	}

	// Foreach roll sum:
	// Copy observed
	// calc expected, diff and chi
	public ExperimentResult InterpretExperiment() {
		double minimumExpected = double.MaxValue;

		for(int i = 0; i <= maxRollSum; i++) {
			if(rollSums[i].Probability != 0) {
				RollSumStat rss = rollSums[i];
				rss.Observed = observed[i];
				rss.Expected = rss.Probability * throws;
				minimumExpected = Min(minimumExpected, rss.Expected);
				rss.Diff = Abs(rss.Observed - rss.Expected);
				rss.Chi = rss.Diff * rss.Diff / rss.Expected;
				chiSquareSum += rss.Chi;
				// rollSums[i] = rss;
			}
		}

		expResult = EvaluateResult(minimumExpected);
		return expResult;
		//string result = $"{dices}{diceType}: {chiSquareSum}";
		// arciSum += chiSquareSum;
		// Console.WriteLine(result);
		// AllResults.Add(result);
	}

	ExperimentResult EvaluateResult(double minimumExpected) {
		string dt = $"{dices}{diceType}";
		int degreesOfFreedom = maxRollSum - minRollSum;

		// TODO evaluate based on degrees of freedom...
		double alpha10 = 100.0;
		double alpha5 = 90.0;
		double alpha1 = 80.0;

		ExperimentDecision decision = Decide(chiSquareSum, minimumExpected, alpha10, alpha5, alpha1);

		ExperimentResult result = new(dt, throws, minimumExpected, chiSquareSum, degreesOfFreedom, alpha10, alpha5, alpha1, decision);
		return result;

	}
	
	ExperimentDecision Decide(double chiSquareSum, double minimumExpected, double alpha10, double alpha5, double alpha1) {
		if(minimumExpected < 5)
			return ExperimentDecision.InvalidExperiment_ExpectedMinimumLessThan5;

		if(chiSquareSum < alpha10)
			return ExperimentDecision.NotRejected;

		if(chiSquareSum < alpha5)
			return ExperimentDecision.RejectedAtAlpha10;

		if(chiSquareSum < alpha1)
			return ExperimentDecision.RejectedAtAlpha5;

		// chisquaresum is equal or greater than alpha 1 -> rejected at alpha1
		return ExperimentDecision.RejectedAtAlpha1;
	}

	public void SerializeResult() {
		DnD_Dice_Statistic_Experiment exp = new(diceType, dices, throws, chiSquareSum, 0.3838, 0.3838, 0.3838, "not yet implemented", rollSums);

		JsonSerializerOptions options = new() { WriteIndented = true };
		string jsonResult = JsonSerializer.Serialize(exp, options);
		using StreamWriter writer = new($"./Experiment-Result-{dices}d{(int)diceType}.json");
		writer.Write(jsonResult);
	}
}