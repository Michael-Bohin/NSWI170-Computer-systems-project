namespace StatisticOfStatistics;
using System.Diagnostics;
using System.Text.Json;


class Program {
	static void Main() {
		List<ExperimentResult> experimentResults = new();

		
		long throwsInExperiment = 30_000;
		DiceType type = DiceType.d6;
		int dices = 3;
		int experimentRepetitions = 100_000;

		DnD_Dice_Probability_Calculator pc = new(type);
		pc.CalculateCompositions();
		pc.CalculateProbabilities();
		List<string> results = pc.SerializeResult();
		string result3d6 = results[2];

		for(int repetition = 0; repetition < experimentRepetitions; repetition++) {
			Stopwatch sw = Stopwatch.StartNew();
			sw.Start();

			DnD_Dice_Mass_Thrower mt = new(type, dices, throwsInExperiment);
			mt.GenerateStatistic();
			mt.LoadModel(result3d6);
			ExperimentResult expRes = mt.InterpretExperiment();

			sw.Stop();
			long seconds = sw.ElapsedMilliseconds / 1000;
			long milis = sw.ElapsedMilliseconds % 1000;
			if(repetition % 1000 == 0)
				Console.WriteLine($"repetition number: {repetition}"); //Total time elapsed for dice {expRes.DiceType}, experiment repetitions: {expRes.Throws}, {seconds} sec, {milis}

			mt.SerializeResult(experimentRepetitions);
			expRes.SetDuration(seconds, milis);
			experimentResults.Add(expRes);
		}
		
		JsonSerializerOptions options = new() { WriteIndented = true };
		string json = JsonSerializer.Serialize(experimentResults, options);
		using (StreamWriter writer = new($"./Experiments-Statistic-Summaries-Throws-{throwsInExperiment}-Reps-{experimentRepetitions}.json")) {
			writer.Write(json);
		}
		

		Summarize(throwsInExperiment, experimentRepetitions);
	}

	static void Summarize(long throwsInExperiment , int experimentRepetitions) {

		SummaryOfSummaries sos = new();

		using StreamReader sr = new($"./Experiments-Statistic-Summaries-Throws-{throwsInExperiment}-Reps-{experimentRepetitions}.json");
		string file = sr.ReadToEnd();
		var experimentResults = JsonSerializer.Deserialize<List<ExperimentResult>>(file);

		Summary s = new();
		s.SampleSize = experimentResults[0].Throws;

		foreach (ExperimentResult experimentResult in experimentResults) {
			string id = experimentResult.DiceType + " " + experimentResult.Throws;
			if (experimentResult.Decision == ExperimentDecision.InvalidExperiment_ExpectedMinimumLessThan5) {
				s.InvalidExperiments++;
			} else if (experimentResult.Decision == ExperimentDecision.NotRejected) {
				s.NotRejected++;
			} else if (experimentResult.Decision == ExperimentDecision.RejectedAtAlpha10) {
				s.RejectedAtAlpha_Ten++;
			} else if (experimentResult.Decision == ExperimentDecision.RejectedAtAlpha5) {
				s.RejectedAtAlpha_Five++;
			} else {
				// rejected at alpha1.. 
				s.RejectedAtAlpha_One++;
			}
		}

		sos.Summaries.Add(s);


		JsonSerializerOptions options = new() { WriteIndented = true };
		string result = JsonSerializer.Serialize(sos, options);
		using StreamWriter sw = new("./SummaryOfSummaries.json");
		sw.WriteLine(result);
	}
}