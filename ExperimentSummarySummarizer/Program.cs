using System.Text.Json;
using System.Text.Json.Serialization;

int[] sampleSizes  = new int[] { 100000, 500_000, 1_000_000, 10_000_000, 100_000_000 };

SummaryOfSummaries sos = new();

Summary total = new();
total.SampleSize = -1; // total of all

Summary invalidExperimens = new();
invalidExperimens.SampleSize = -99; // summary of invalid experiments

Summary invalidExperimentsOver1 = new();
invalidExperimentsOver1.SampleSize = -100;

foreach (int sampleSize in sampleSizes) {
	using StreamReader sr = new($"./Experiments-Statistic-Summaries-Reps_{sampleSize}.json");
	string file = sr.ReadToEnd();
	var experimentResults = JsonSerializer.Deserialize<List<ExperimentResult>>(file);

	Summary s = new();
	s.SampleSize = experimentResults[0].Throws;

	foreach(ExperimentResult experimentResult in experimentResults) {
		// Console.WriteLine(experimentResult.DiceType + " " + experimentResult.Decision);
		string id = experimentResult.DiceType + " " + experimentResult.Throws;
		if (experimentResult.Decision == ExperimentDecision.InvalidExperiment_ExpectedMinimumLessThan5) {
			s.InvalidExperiments++;
			sos.Invalid.Add(id);
			
			ExperimentDecision ed = Referee.Decide(experimentResult.Pvalue);

			if(ed == ExperimentDecision.NotRejected) {
				invalidExperimens.NotRejected++;
				if(experimentResult.MinimumExpectedSum >1.0)
					invalidExperimentsOver1.NotRejected++;
			} else if(ed == ExperimentDecision.RejectedAtAlpha10) {
				// Console.WriteLine(experimentResult.MinimumExpectedSum);
				invalidExperimens.RejectedAtAlpha_Ten++;
				if (experimentResult.MinimumExpectedSum > 1.0)
					invalidExperimentsOver1.RejectedAtAlpha_Ten++;
			} else if( ed == ExperimentDecision.RejectedAtAlpha5) {
				// Console.WriteLine(experimentResult.MinimumExpectedSum);
				invalidExperimens.RejectedAtAlpha_Five++;
				if (experimentResult.MinimumExpectedSum > 1.0)
					invalidExperimentsOver1.RejectedAtAlpha_Five++;
			} else if(ed == ExperimentDecision.RejectedAtAlpha1) {
				// Console.WriteLine(experimentResult.MinimumExpectedSum);
				invalidExperimens.RejectedAtAlpha_One++;
				if (experimentResult.MinimumExpectedSum > 1.0)
					invalidExperimentsOver1.RejectedAtAlpha_One++;
			}

		} else if(experimentResult.Decision == ExperimentDecision.NotRejected) {
			s.NotRejected++;
			sos.NotRejected.Add(id);
		} else if(experimentResult.Decision == ExperimentDecision.RejectedAtAlpha10) {
			Console.WriteLine(experimentResult.MinimumExpectedSum);
			s.RejectedAtAlpha_Ten++;
			sos.RejectedAtTen.Add(id);
		} else if(experimentResult.Decision == ExperimentDecision.RejectedAtAlpha5) {
			Console.WriteLine(experimentResult.MinimumExpectedSum);
			s.RejectedAtAlpha_Five++;
			sos.RejectedAtFive.Add(id);
		} else {
			// rejected at alpha1.. 
			Console.WriteLine(experimentResult.MinimumExpectedSum);
			s.RejectedAtAlpha_One++;
			sos.RejectedAtOne.Add(id);
		}
	}

	total.InvalidExperiments += s.InvalidExperiments;
	total.NotRejected += s.NotRejected;
	total.RejectedAtAlpha_Ten += s.RejectedAtAlpha_Ten;
	total.RejectedAtAlpha_Five += s.RejectedAtAlpha_Five;
	total.RejectedAtAlpha_One += s.RejectedAtAlpha_One;
	sos.Summaries.Add(s);
}

sos.Summaries.Add(total);
sos.Summaries.Add(invalidExperimens);
sos.Summaries.Add(invalidExperimentsOver1);

JsonSerializerOptions options = new() { WriteIndented = true };
string result = JsonSerializer.Serialize(sos, options);
using StreamWriter sw = new("./SummaryOfSummaries.json");
sw.WriteLine(result);


static class Referee {
	public static ExperimentDecision Decide(double pValue) {
		if(pValue > 0.1)
			return ExperimentDecision.NotRejected;
		if(pValue > 0.05)
			return ExperimentDecision.RejectedAtAlpha10;
		if(pValue > 0.01)
			return ExperimentDecision.RejectedAtAlpha5;

		return ExperimentDecision.RejectedAtAlpha1;
	}
}

enum ExperimentDecision { InvalidExperiment_ExpectedMinimumLessThan5, NotRejected, RejectedAtAlpha10, RejectedAtAlpha5, RejectedAtAlpha1 }

record ExperimentResult {
	public string DiceType { get; } = default!;
	public long Throws { get; }
	public double MinimumExpectedSum { get; } // in order to determine, whether the experiment can or cannot be carried out
	public double ChiSquareValue { get; }
	public int DegreesOfFreedom { get; }
	public double Alpha_10_critical { get; }
	public double Alpha_5_critical { get; }
	public double Alpha_1_critical { get; }
	public double Pvalue { get; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ExperimentDecision Decision { get; }
	public double DurationSec { get; private set; }
	public double DurationMs { get; private set; }

	public ExperimentResult(string DiceType, long Throws, double MinimumExpectedSum, double ChiSquareValue, int DegreesOfFreedom, double Alpha_10_critical, double Alpha_5_critical, double Alpha_1_critical, double Pvalue, ExperimentDecision Decision) {
		this.DiceType = DiceType;
		this.Throws = Throws;
		this.MinimumExpectedSum = MinimumExpectedSum;
		this.ChiSquareValue = ChiSquareValue;
		this.DegreesOfFreedom = DegreesOfFreedom;
		this.Alpha_10_critical = Alpha_10_critical;
		this.Alpha_5_critical = Alpha_5_critical;
		this.Alpha_1_critical = Alpha_1_critical;
		this.Pvalue = Pvalue;
		this.Decision = Decision;
	}

	public void SetDuration(long sec, long ms) {
		DurationSec = sec;
		DurationMs = ms;
	}
}

record Summary { 
	public long SampleSize { get; set;}
	public int InvalidExperiments { get; set; }
	public int NotRejected { get; set; }
	public int RejectedAtAlpha_Ten { get; set; }
	public int RejectedAtAlpha_Five { get; set; }
	public int RejectedAtAlpha_One { get; set; }
}

record SummaryOfSummaries {
	public List<Summary> Summaries { get; } = new();
	public List<string> Invalid { get; } = new();
	public List<string> NotRejected { get; } = new();
	public List<string> RejectedAtTen { get; } = new();
	public List<string> RejectedAtFive { get; } = new();
	public List<string> RejectedAtOne { get; } = new();
}