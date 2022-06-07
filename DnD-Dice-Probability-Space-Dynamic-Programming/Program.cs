using System.Diagnostics;
using System.Text.Json;

Stopwatch sw = Stopwatch.StartNew();
sw.Start();
foreach (DiceType type in Enum.GetValues<DiceType>()) {
	int min = 1, max = (int)type, sides = max;
	List<List<long>> probSpace = new();
	probSpace.Add(new List<long>()); // dummy 
	probSpace.Add(new List<long>());
	probSpace[1].Add(0); // dummy
	for (int i = 0; i < sides; i++)
		probSpace[1].Add(1); // pocatek
	min++; max += sides;

	for (int dices = 2; dices < 10; dices++) {
		probSpace.Add(new List<long>()); // throwing with x dices
		for (int dummy = 0; dummy < min; dummy++)  // fill with dummy zeroes for code simplicity:
			probSpace[dices].Add(0);

		for (int j = min; j <= max; j++) {
			long compositions = 0;
			// 1. ( j - k ) > 0
			// 2. ( j - k ) <=  (max - sides)
			// 3. if both are true : compositions += probSpace[dices-1][j-k]
			for (int k = 1; k <= sides; k++) {
				int x = j - k;
				if (x > 0 && x <= (max - sides))
					compositions += probSpace[dices - 1][x];
			}
			probSpace[dices].Add(compositions);
		}
		min++;
		max += sides;
	}

	JsonSerializerOptions options = new() { WriteIndented = true };
	string json = JsonSerializer.Serialize(probSpace, options);

	using StreamWriter writer = new($"./x-d{(int)type}-Rolls-Summary-Dynamic-Programming.json");
	writer.Write(json);
}
sw.Stop();
Console.WriteLine($"Total time elapsed: {sw.ElapsedMilliseconds /1000} sec, {sw.ElapsedMilliseconds%1000} milis.");

enum DiceType { d4 = 4, d6 = 6, d8 = 8, d10 = 10, d12 = 12, d20 = 20 };