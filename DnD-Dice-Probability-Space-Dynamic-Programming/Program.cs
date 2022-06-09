namespace DnD_Dice_Probability_Space;
using System.Diagnostics;
using System.Text.Json;

class Program {
	static void Main() {
		List<ExperimentResult> experimentResults = new();
		long experiment_repetitions = 100_000_000;

		foreach (DiceType type in Enum.GetValues<DiceType>()) {
			DnD_Dice_Probability_Calculator pc = new(type);
			pc.CalculateCompositions();
			pc.CalculateProbabilities();
			List<string> results = pc.SerializeResult();

			int diceCounter = 1;
			foreach(string result in results) {
				Stopwatch sw = Stopwatch.StartNew();
				sw.Start();

				DnD_Dice_Mass_Thrower mt = new(type, diceCounter++, experiment_repetitions);
				mt.GenerateStatistic();
				mt.LoadModel(result);
				ExperimentResult expRes = mt.InterpretExperiment();

				sw.Stop();
				long seconds = sw.ElapsedMilliseconds / 1000;
				long milis = sw.ElapsedMilliseconds % 1000;
				Console.WriteLine($"Total time elapsed for dice {expRes.DiceType}, experiment repetitions: {expRes.Throws}, {seconds} sec, {milis} milis.");

				mt.SerializeResult();
				expRes.SetDuration(seconds, milis);
				experimentResults.Add(expRes);
			}
		}

		JsonSerializerOptions options = new() { WriteIndented = true };
		string json = JsonSerializer.Serialize(experimentResults, options);
		using StreamWriter writer = new($"./Experiments-Statistic-Summaries-Reps_{experiment_repetitions}.json");
		writer.Write(json);
	}
}