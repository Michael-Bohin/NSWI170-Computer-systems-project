using static System.Console;

WriteLine("Hello, World!");
List<Dice> dices = new();

Dice d4 = new(1, 4, 1);
Dice d6 = new(1, 6, 1);
Dice d8 = new(1, 8, 1);
Dice d10 = new(1, 10, 1);
Dice d12 = new(1, 12, 1);
Dice d20 = new(1, 20, 1);
Dice d100 = new(0, 90, 10);

dices.Add(d4);	
dices.Add(d6);	
dices.Add(d8);
dices.Add(d10);	
dices.Add(d12);	
dices.Add(d20);
dices.Add(d100);

foreach(Dice d in dices)
	WriteLine(d.DescribeDice());

record Dice {
	public readonly int minVal;
	public readonly int maxVal;
	public readonly int increment; // this is needed because of the d100, which has increments by 10
	public int NumberOfSides { 
		get { 
			int count = 0;
			for(int i = minVal; i <= maxVal; i += increment) {
				count++;			
			}
			return count;
		}
	}

	public Dice(int min, int max, int inc) {
		if(max <= min || min < 0 || max < 0 || inc < 1)
			throw new ArgumentOutOfRangeException($"Dice has been initialised with impossible parameters: min {min}, max {max}, inc {inc}");
		this.minVal = min;	
		this.maxVal = max;	
		this.increment = inc;	
	}

	public string DescribeDice() => $"The dice minValue is: {minVal}, maxValue is: {maxVal}, incremenet: {increment}, Number of sides: {NumberOfSides}.";
}









