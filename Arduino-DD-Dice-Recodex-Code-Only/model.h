#ifndef DICETYPE_
#define DICETYPE_
enum DiceType { d4, d6, d8, d10, d12, d20, d100 };
#endif

#ifndef DDDice_h
#define DDDice_h

class DDDice {
public:
	size_t Roll(DiceType dice, size_t throws, unsigned long seed) {
		// d100 is the most strange -> process as d10 and multiply all results by 10
		randomSeed(seed);
		size_t min = 1;
		if (dice == d10 || dice == d100)
			min = 0;
		size_t max = SetMax(dice) + 1; // arduino random() function has max in exclusive logic

		size_t sum = 0;
		for (size_t i = 0; i < throws; i++)
			sum += random( min, max);
		if (dice == d100)
			sum *= 10;
		return sum;
	}

private:
	size_t SetMax(DiceType dice) {
		if (dice == d4) {
			return 4;
		} else if (dice == d6) {
			return 6;
		} else if (dice == d8) {
			return 8;
		} else if (dice == d10 || dice == d100) {
			return 9;
		} else if (dice == d12) {
			return 12;
		} else if (dice == d20) {
			return 20;
		} else {
			//throw "DDDice class receieved roll with unknown DiceType. Critical code error.";
		}
	}
};

#endif