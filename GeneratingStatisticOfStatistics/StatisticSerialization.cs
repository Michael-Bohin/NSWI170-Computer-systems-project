namespace StatisticOfStatistics;

record Summary {
	public long SampleSize { get; set; }
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