namespace DnD_Probability_Space;


class Program {
    static void Main() {
		foreach (DiceType type in Enum.GetValues<DiceType>()) 
            for (uint rolls = 1; rolls < 7; rolls++) {
                DDDice_ProbabilitySpaceEnumerator pe = new(type, rolls);
                pe.EnumerateAllPossibleOutcomes();
                pe.PrintSummary();

                DDDice_Mass_Thrower dd = new(type, (int)rolls);
                uint[] stats = dd.GenerateStatistic(1_000_000);
                
                using (StreamWriter sw = new StreamWriter($"./d{(uint)type}/{rolls}d{(uint)type}-Chi-square-test.csv")) {
                    sw.WriteLine("soucet-na-kostce;pocet-partitions;pocet-compositions;pravdepodobnost-souctu;pocet-ocekavanych-vyskytu-jevu-na-1-milion-hodu;pocet-pozorovanych-jevu;rozdil-ocekavane-hozene;chi-kvadrat-hodnota-souctu;");
                    double elementarnichJevu = Pow((double)type, (double)rolls);
                    double ChiSquareSum = 0;
                    ulong observedTotal = 0;
                    for (int targetSum = dd.minSum; targetSum <= dd.maxSum; targetSum++) {
                        // int partitions = pe.targetSumStats
                        ulong partitions = pe.targetSum[targetSum].partitions;
                        ulong compositions = pe.targetSum[targetSum].compositions;
                        double probability = (double) compositions / elementarnichJevu;
                        double expectedInMillion = probability * 1_000_000;
                        ulong actualObserved = dd.statistic[targetSum];
                        double difference = ((double)actualObserved - expectedInMillion);
                        double chiResult = difference * difference / expectedInMillion;

                        sw.Write($"{targetSum};{partitions};{compositions};");
                        sw.Write($"{probability};{expectedInMillion};{actualObserved};");
                        sw.WriteLine($"{difference};{chiResult};");
                        ChiSquareSum += chiResult;
                        observedTotal += actualObserved;
                    }
                    sw.WriteLine($"observed total: {observedTotal} chi square result: {ChiSquareSum}");
				}

            }
    }
}