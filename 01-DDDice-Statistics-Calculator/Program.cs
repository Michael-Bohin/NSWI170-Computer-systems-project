namespace DnD_Probability_Calculator;
using static System.Math;

public enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };

class Program {
    static void Main() {
        for (uint rolls = 1; rolls < 10; rolls++)
			foreach (DiceType type in Enum.GetValues<DiceType>()) {
                DDDice_ProbabilitySpaceEnumerator pe = new(type, rolls);
                pe.EnumerateAllPossibleOutcomes();
                pe.PrintSummary();
            }
    }
}

record TargetSumEnumeration {
    public readonly ulong targetSum, partitions, compositions;
    public TargetSumEnumeration(ulong ts, ulong p, ulong c) {
        targetSum = ts;
        partitions = p;
        compositions = c;
    }
}

public class DDDice_ProbabilitySpaceEnumerator {
    readonly DiceType dType;
    readonly uint rolls;
    const uint increment = 1, min = 1;
    readonly uint minRollSum, maxRollSum;
    ulong totalPartitions, totalCompositions = 0;
    List<TargetSumEnumeration> targetSumStats = new();

    public DDDice_ProbabilitySpaceEnumerator(DiceType d, uint r) {
        dType = d;
        rolls = r;
        minRollSum = rolls;
        maxRollSum = rolls * (uint)dType;
	}

    

    public void EnumerateAllPossibleOutcomes() {
        using (StreamWriter swLog = new($"./d{dType}/DetailedLog/{rolls}d{dType}-Detailed-Calculation-log.txt")) {
            for (uint targetSum = minRollSum; targetSum <= maxRollSum; targetSum++) {
                CompositionsCounter cc = new(targetSum, rolls, min, (uint)dType, increment);
                cc.SearchPartitions();
                cc.CountCompositions();
                cc.Print(swLog);

                TargetSumEnumeration rst = new(targetSum, cc.partitionsTotal, cc.compositionsTotal);
                targetSumStats.Add(rst);
            }
        }
    }

    public void PrintSummary() {
        using (StreamWriter sw = new($"./d{dType}/{rolls}d{dType}-Roll-Summary.txt")) {
            sw.WriteLine($"Summary of roll with {rolls} dices and dice type {min}-{dType} >> {rolls}d{dType}\n");
            foreach (TargetSumEnumeration rst in targetSumStats) {
                totalPartitions += rst.partitions;
                totalCompositions += rst.compositions;
                sw.WriteLine($"Target sum: {rst.targetSum}, partitions: {rst.partitions}, compositions: {rst.compositions}");
            }

            sw.WriteLine($"Total number of different roll outcomes of roll {rolls}d{dType}:");
            sw.WriteLine($"Partitions: {totalPartitions}, compositions: {totalCompositions}");

            // Sumcheck dvojim pocitanim. Vime kolik ma vyjit celkem compositions: (pocet stran kostky) na (pocet hozenych kosteck).
            // Pokud nam to nevyjde vyhodit math error:

            ulong expectedCompositions = (ulong)Pow((double)((uint)dType - min + 1), (double)(rolls));

            sw.WriteLine($"Expected compositions: {expectedCompositions}, found compositions: {totalCompositions}\n\n");
            if (expectedCompositions != totalCompositions) 
                throw new Exception("Math in code is not correct! Fix it.");

            sw.WriteLine(" >>> Probabilities <<<");
            double total = (double)totalCompositions;
            foreach (TargetSumEnumeration rst in targetSumStats) {
                ulong comp = rst.compositions;
                double prob = ((double)comp) / total;
                double expectedInMillionRolls = prob * (double)1_000_000;
                sw.WriteLine($"Target sum {rst.targetSum}: probability - {comp} / {totalCompositions} = {prob:N4}, expected in 1 million rolls - {expectedInMillionRolls:N1}");
            }
        }
    }
}


class CompositionsCounter {
    public List<List<uint>> partitions = new();
    public List<ulong> compositionsCount = new();
    private ulong[] factorial = new ulong[10];
    public CompositionsCounter(uint targetSum, uint dices, uint min, uint max, uint inc) {
        this.targetSum = targetSum;
        this.dices = dices;
        this.min = min;
        this.max = max;
        this.inc = inc;
        factorial[0] = 1;

        for (uint i = 1; i < 10; i++) {
            uint fact = 1;
            for (uint j = i; j > 0; j--)
                fact *= j;
            factorial[i] = fact;
        }
    }

    readonly uint targetSum, dices, min, max, inc;

    public ulong partitionsTotal = 0;
    public ulong compositionsTotal = 0;
    public void SearchPartitions() {
        SearchPartitions(new List<uint>(), targetSum, min);
    }

    void SearchPartitions(List<uint> list, uint n, uint searched) {
        if (n == 0) {
            if (list.Count != dices)
                return;
            partitionsTotal++;
            partitions.Add(list);

        } else {
            if (list.Count == dices)
                return;

            for (uint i = searched; i <= max; i += inc) {
                List<uint> next = new();
                foreach (uint x in list)
                    next.Add(x);
                next.Add(i);
                SearchPartitions(next, n - i, i);
            }
        }
    }

    public void CountCompositions() {
        for (int i = 0; i < partitions.Count; i++) {
            ulong compositions = CountCompositions(partitions[i]);
            compositionsCount.Add(compositions);
            compositionsTotal += compositions;
        }
    }

    private ulong CountCompositions(List<uint> _partition) {
        List<uint> counts = new();
        for (uint i = min; i <= max; i += inc)
            counts.Add(0);
        
        foreach (int i in _partition)
            counts[i - 1]++;

        ulong compositions = factorial[_partition.Count];
        foreach (uint opakovani in counts)
            if (opakovani > 1)
                compositions /= factorial[opakovani];

        return compositions;
    }

    public void Print(StreamWriter swLog) {
        if (max > 10)
            return; // safety check to not log large files.. 
        swLog.WriteLine($"Target sum: {targetSum}, dices: {dices}");
        for (int i = 0; i < partitions.Count; i++) {
            uint sum = 0;
            for (int j = 0; j < partitions[i].Count; j++) {
                swLog.Write($"{partitions[i][j]} ");
                sum += partitions[i][j];
            }
            swLog.WriteLine($" => {sum}, compositions count: {compositionsCount[i]}");
        }
        swLog.WriteLine($"Partitions total: {partitionsTotal}, compositions total: {compositionsTotal}\n");
    }
}