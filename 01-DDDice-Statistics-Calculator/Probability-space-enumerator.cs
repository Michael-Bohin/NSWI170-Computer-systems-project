/*
namespace DnD_Probability_Space;
using static System.Math;
using static System.Console;

public record TargetSumEnumeration {
    public ulong partitions, compositions = 0;
    /*public TargetSumEnumeration() {
        partitions = 0; 
        compositions = 0;
	}

    public TargetSumEnumeration(ulong p, ulong c) {
        _partitions = p;
        _compositions = c;
    }
    public SetPartitions(ulong p) => 
}

public class DDDice_ProbabilitySpaceEnumerator {
    readonly DiceType dType;
    readonly uint rolls;
    const uint increment = 1, min = 1;
    readonly uint minRollSum, maxRollSum;
    ulong totalPartitions, totalCompositions = 0;
    public TargetSumEnumeration[] targetSum;
    ulong[] factorial = new ulong[10];

    public DDDice_ProbabilitySpaceEnumerator(DiceType d, uint r) {
        dType = d;
        rolls = r;
        minRollSum = rolls;
        maxRollSum = rolls * (uint)dType;
        targetSum = new TargetSumEnumeration[maxRollSum+1];
        for(int i = 0; i <= maxRollSum; i++)
            targetSum[i] = new TargetSumEnumeration();  

        factorial[0] = 1;
        for (uint i = 1; i < 10; i++) {
            uint fact = 1;
            for (uint j = i; j > 0; j--)
                fact *= j;
            factorial[i] = fact;
        }
    }

    public void EnumerateAllPossibleOutcomes() {
        WriteLine($"Writting to file: ./d{(uint)dType}/DetailedLog/{rolls}d{(uint)dType}-Detailed-Calculation-log.txt");
        using (StreamWriter swLog = new($"./d{(uint)dType}/DetailedLog/{rolls}d{(uint)dType}-Detailed-Calculation-log.txt")) {
            for (uint _targetSum = minRollSum; _targetSum <= maxRollSum; _targetSum++) {
                CompositionsCounter cc = new(_targetSum, rolls, min, (uint)dType, increment, factorial);
                cc.SearchPartitions();
                cc.CountCompositions();
                cc.Print(swLog);

                targetSum[_targetSum].partitions = cc.partitionsTotal;
                targetSum[_targetSum].compositions = cc.compositionsTotal;
            }
        }
    }

    public void PrintSummary() {
        WriteLine($"Writting to file: ./d{(uint)dType}/{rolls}d{(uint)dType}-Roll-Summary.txt\n");
        using (StreamWriter sw = new($"./d{(uint)dType}/{rolls}d{(uint)dType}-Roll-Summary.txt")) {
            sw.WriteLine($"Summary of roll with {rolls} dices and dice type {min}-{(uint)dType} >> {rolls}d{(uint)dType}\n");
            for (uint i = minRollSum; i <= maxRollSum; i++) {
                totalPartitions += targetSum[i].partitions;
                totalCompositions += targetSum[i].compositions;
                sw.WriteLine($"Target sum: {i}, partitions: {targetSum[i].partitions}, compositions: {targetSum[i].compositions}");
            }

            sw.WriteLine($"Total number of different roll outcomes of roll {rolls}d{(uint)dType}:");
            sw.WriteLine($"Partitions: {totalPartitions}, compositions: {totalCompositions}");

            // Sumcheck dvojim pocitanim. Vime kolik ma vyjit celkem compositions: (pocet stran kostky) na (pocet hozenych kosteck).
            // Pokud nam to nevyjde vyhodit math error:

            ulong expectedCompositions = (ulong)Pow((double)((uint)dType - min + 1), (double)(rolls));

            sw.WriteLine($"Expected compositions: {expectedCompositions}, found compositions: {totalCompositions}\n\n");
            if (expectedCompositions != totalCompositions)
                throw new Exception("Math in code is not correct! Fix it.");

            sw.WriteLine(" >>> Probabilities <<<");
            double total = (double)totalCompositions;
            for (uint i = minRollSum; i <= maxRollSum; i++) {
                ulong comp = targetSum[i].compositions;
                double prob = ((double)comp) / total;
                double expectedInMillionRolls = prob * (double)1_000_000;
                sw.WriteLine($"Target sum {i}: probability - {comp} / {totalCompositions} = {prob:N4}, expected in 1 million rolls - {expectedInMillionRolls:N1}");
            }
        }
    }
}


*/