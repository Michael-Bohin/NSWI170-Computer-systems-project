namespace DnD_Probability_Space;

class DDDice_Mass_Thrower {
	Random rand = new();
	readonly int max, exclusiveMax, throws;
	public readonly int minSum, maxSum;
	public uint[] statistic;

	public DDDice_Mass_Thrower(DiceType m, int t) {
		max = (int)m;
		exclusiveMax = max + 1;
		throws = t;
		minSum = t;
		maxSum = max * throws;
		statistic = new uint[maxSum+1];
	}

	public uint[] GenerateStatistic(uint rolls) {
		for(int i = 0; i < rolls; i++) {
			int sum = ThrowDice();
			statistic[sum]++;
		}
		return statistic;
	}

	int ThrowDice() {
		int rollSum = 0;
		for(int i = 0; i < throws; i++) 
			rollSum += rand.Next(1, exclusiveMax);
		return rollSum;
	}
}