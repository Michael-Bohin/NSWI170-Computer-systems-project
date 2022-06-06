namespace DnD_Probability_Space;
/*
class CompositionsCounter {
    public List<List<uint>> partitions = new();
    public List<ulong> compositionsCount = new();
    readonly ulong[] factorial = new ulong[10];
    public CompositionsCounter(uint targetSum, uint dices, uint min, uint max, uint inc, ulong[] f) {
        this.targetSum = targetSum;
        this.dices = dices;
        this.min = min;
        this.max = max;
        this.inc = inc;
        factorial = f;
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

        ulong compositions = factorial[_partition.Count]; // _partition.Count will be at most 9 for dices 9d'x'
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
*/