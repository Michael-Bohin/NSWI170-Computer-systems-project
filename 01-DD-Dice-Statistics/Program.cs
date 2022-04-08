using static System.Console;
using static System.Math;

int minVal = 1;
int maxVal = 4;
int increment = 1;

for(int dices = 1; dices < 10; dices++) {
    RollCalculator rc = new(dices, minVal, maxVal, increment);
    using (StreamWriter swLog = new($"./d{maxVal}/DetailedLog/{dices}d{maxVal}-DISABLED-Detailed-Calculation-log.txt")) 
        rc.EnumerateAllPossibleRolls(swLog);

    using (StreamWriter swSummary = new($"./d{maxVal}/{dices}d{maxVal}-Roll-Summary.txt")) 
        rc.PrintSummary(swSummary);
}

record RollSubTree {
    public readonly long targetSum, partitions, compositions;
    public RollSubTree(long ts, long p, long c) {
        targetSum = ts;
        partitions = p;
        compositions = c;
	}
}

class RollCalculator {
    int dices, min, max, inc;
	readonly List<RollSubTree> targetSumsStats = new();
    long totalPart, totalComp;
	private readonly int minRollSum, maxRollSum;

	public RollCalculator(int dices, int min, int max, int inc) {
        this.dices = dices;
        this.min = min;
        this.max = max;
        this.inc = inc;
        minRollSum = min * dices;
        maxRollSum = max * dices;
    }

    public void EnumerateAllPossibleRolls(StreamWriter swLog) {
        for (int targetSum = minRollSum; targetSum <= maxRollSum; targetSum++) {
            CompositionsCounter cc = new(targetSum, dices, min, max, inc);
            cc.SearchPartitions();
            cc.CountCompositions();
            cc.Print(swLog);
            
            RollSubTree rst = new(targetSum, cc.partitionsTotal, cc.compositionsTotal);
            targetSumsStats.Add(rst);
        }
    }

    public void PrintSummary(StreamWriter swSummary) {
        swSummary.WriteLine($"Summary of roll with {dices} dices and dice type {min}-{max} >> {dices}d{max}\n");
        foreach (RollSubTree rst in targetSumsStats) {
            totalPart += rst.partitions;
            totalComp += rst.compositions;
            swSummary.WriteLine($"Target sum: {rst.targetSum}, partitions: {rst.partitions}, compositions: {rst.compositions}");
        }
        swSummary.WriteLine($"Total number of different roll outcomes of roll {dices}d{max}:");
        swSummary.WriteLine($"Partitions: {totalPart}, compositions: {totalComp}");

        // Sumcheck dvojim pocitanim. Vime kolik ma vyjit celkem compositions: (pocet stran kostky) na (pocet hozenych kosteck).
        // Pokud nam to nevyjde vyhodit math error.

        long expectedCompositions = (long) Pow((double)(max-min+1), (double)(dices));

        swSummary.WriteLine($"Expected compositions: {expectedCompositions}, found compositions: {totalComp}\n\n");
        if(expectedCompositions != totalComp) {
            throw new Exception("Math in code is not correct! Fix it.");
		}

        swSummary.WriteLine(" >>> Probabilities <<<");
        double total = (double)totalComp;
        foreach (RollSubTree rst in targetSumsStats) {
            long comp = rst.compositions;
            double prob = ((double)comp) / total;
            double expectedInMillionRolls = prob * (double)1_000_000;
            swSummary.WriteLine($"Target sum {rst.targetSum}: probability - {comp} / {totalComp} = {prob:N4}, expected in 1 million rolls - {expectedInMillionRolls:N1}");
        }
    }
}

class CompositionsCounter {
    public List<List<int>> partitions = new();
    public List<long> compositionsCount = new();
    private long[] factorial = new long[10];
    public CompositionsCounter(int targetSum, int dices, int min, int max, int inc) {
        this.targetSum = targetSum;
        this.dices = dices;
        this.min = min;
        this.max = max;
        this.inc = inc;
        factorial[0] = 1;

        for(int i = 1 ; i < 10;i++) {
            int fact = 1;
            for(int j = i; j > 0; j--)
                fact *= j;
            factorial[i] = fact;
		}
        //for(int i = 0 ; i < 10; i++) WriteLine($"i: {i}, Factorial: {factorial[i]}");
	}

    readonly int targetSum;
    readonly int dices;
    readonly int min;
    readonly int max;
    readonly int inc;
    public long partitionsTotal = 0;
    public long compositionsTotal = 0;
    public void SearchPartitions() {
        SearchPartitions(new List<int>(), targetSum, min);
    }

    void SearchPartitions(List<int> list, int n, int searched) {
        if (n == 0) {
            if(list.Count != dices)
                return;
            partitionsTotal++;
            partitions.Add(list);

        } else {
            if (list.Count == dices)
                return;

            for (int i = searched; i <= max; i = i + inc) {
                List<int> next = new();
                foreach (int x in list)
                    next.Add(x);
                next.Add(i);
                SearchPartitions(next, n - i, i);
            }
        }
    }

    public void CountCompositions() {
        for(int i = 0; i < partitions.Count; i++) {
            long compositions = CountCompositions(partitions[i]);
            compositionsCount.Add(compositions);
            compositionsTotal += compositions;
		}
	}

    private long CountCompositions(List<int> _partition) {
        // WriteLine("Counting partition: "); foreach(int i in _partition) Write(i + " "); WriteLine();
        List<int> counts = new();
        for(int i = min; i <= max; i += inc) 
            counts.Add(0);

        // be ware two types of dices! some have zero some dont. first do the one where it starts from 1
        if (min == 1)
            foreach(int i in _partition)
                counts[i-1]++;
        else 
            foreach(int i in _partition)
                counts[i]++;
        // + what if increment is 10 omg... :D

        long compositions = factorial[_partition.Count];
        foreach(int opakovani in counts) 
            if(opakovani > 1)
                compositions /= factorial[opakovani];

        return compositions;
	}

    public void Print(StreamWriter swLog) {
        if(max > 10)
            return; // safety check to not log large files.. 
        swLog.WriteLine($"Target sum: {targetSum}, dices: {dices}");
        for (int i = 0; i < partitions.Count; i++) {
            int sum = 0;
            for(int j = 0; j < partitions[i].Count; j++) {
                swLog.Write($"{partitions[i][j]} ");
                sum += partitions[i][j];
			}
            swLog.WriteLine($" => {sum}, compositions count: {compositionsCount[i]}");
		}
        swLog.WriteLine($"Partitions total: {partitionsTotal}, compositions total: {compositionsTotal}\n");
    }
}