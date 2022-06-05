#include <stdexcept>
#include <random>

#ifndef DDDice_h
#define DDDice_h

enum class DiceType { d4, d6, d8, d10, d12, d20, d100};

class DDDice {
public:

	DDDice(DiceType _dice, size_t _throws) : dice(_dice), throws(_throws) {
		// d100 is the most strange -> process as d10 and multiply all results by 10
		// inclusive logic of range bounds
		min = 1;
		if (dice == DiceType::d10 || dice == DiceType::d100)
			min = 0;
		max = SetMax(dice);
	}

	size_t Roll() { 
		std::uniform_int_distribution<size_t> distrib{min, max};
		size_t sum = 0;
		for (size_t i = 0; i < throws; i++)
			sum += distrib(generator);
		if (dice == DiceType::d100)
			sum *= 10;
	
		return sum;
	}

	// for later use in arduino
	// size_t Roll(DiceType dice, size_t throws, unsigned long randomSeed) { }

private:
	std::default_random_engine generator{ (unsigned int)std::time(0) };
	size_t min, max, throws;
	DiceType dice;

	size_t SetMax(DiceType dice) {
		if (dice == DiceType::d4) {
			return 4;
		} else if (dice == DiceType::d6) {
			return 6;
		} else if (dice == DiceType::d8) {
			return 8;
		} else if (dice == DiceType::d10 || dice == DiceType::d100) {
			return 9;
		} else if (dice == DiceType::d12) {
			return 12;
		} else if (dice == DiceType::d20) {
			return 20;
		} else {
			throw std::invalid_argument("DDDice class receieved roll with unknown DiceType. Critical code error.");
		}
	}
};

#endif