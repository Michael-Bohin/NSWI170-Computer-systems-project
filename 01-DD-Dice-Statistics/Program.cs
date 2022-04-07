using static System.Console;
using static System.Math;

WriteLine("Hello, World!");
List<Dice> dices = new();

Dice d4 = new(1, 4, 1, "d4");
Dice d6 = new(1, 6, 1, "d6");
Dice d8 = new(1, 8, 1, "d8");
Dice d10 = new(0, 9, 1, "d10");
Dice d12 = new(1, 12, 1, "d12");
Dice d20 = new(1, 20, 1, "d20");
Dice d100 = new(0, 90, 10, "d100");

dices.Add(d4);	
dices.Add(d6);	
dices.Add(d8);
dices.Add(d10);	
dices.Add(d12);	
dices.Add(d20);
dices.Add(d100);

foreach(Dice d in dices)
	WriteLine(d.DescribeDice());

List<int> numberOfThrows = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

int counter = 0;
foreach(Dice d in dices)
	foreach(int throws in numberOfThrows) {
		WriteLine($"{++counter}: Roll {throws}{d.name}. Range of outcomes [{d.minVal * throws}-{d.maxVal * throws}] -> |{1 + (((d.maxVal * throws) - (d.minVal * throws)) / d.increment)}|. Possible variations how to roll: {d.NumberOfSides}^{throws} = {Pow(d.NumberOfSides, throws)}");
		/*WriteLine($"Range of possible results: ");
		WriteLine($"Number of different outcomes of roll: ");
		WriteLine($"Number of combinations how dices can be rolled. Dice has {d.NumberOfSides} sides. We throw {throws} dices independently.");
		WriteLine($"Making number of ");
		WriteLine();*/
	}

record Dice {
	public readonly int minVal;
	public readonly int maxVal;
	public readonly int increment; // this is needed because of the d100, which has increments by 10
	public readonly string name;
	public int NumberOfSides { 
		get { 
			int count = 0;
			for(int i = minVal; i <= maxVal; i += increment) {
				count++;			
			}
			return count;
		}
	}

	public Dice(int min, int max, int inc, string name) {
		if(max <= min || min < 0 || max < 0 || inc < 1)
			throw new ArgumentOutOfRangeException($"Dice has been initialised with impossible parameters: min {min}, max {max}, inc {inc}");
		this.minVal = min;	
		this.maxVal = max;	
		this.increment = inc;	
		this.name = name;
	}

	public string DescribeDice() => $"The dice minValue is: {minVal}, maxValue is: {maxVal}, incremenet: {increment}, Number of sides: {NumberOfSides}.";
}











