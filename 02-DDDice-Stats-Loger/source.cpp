#include <iostream>
#include <fstream>
#include <vector>
#include "DDDice.h"
using namespace std;

int main()
{
    cout << "Hello World!\n";
    vector<size_t> resultFrequency;
    for (size_t i = 0; i < 181; i++) // just take max from 9d20 -> dont test d100
        resultFrequency.push_back(0);

    DiceType diceType = DiceType::d20;
    size_t throws = 9;

    DDDice dice{diceType, throws};
    const size_t M = 1000000;

    for (size_t i = 0; i < M; i++) {
        size_t rollSum = dice.Roll();
        resultFrequency[rollSum]++;
    }

    ofstream myfile;
    myfile.open("example.txt");

    size_t maxVal = throws;
    if (diceType == DiceType::d4)
        maxVal *= 4;
    else if (diceType == DiceType::d6)
        maxVal *= 6;
    else if (diceType == DiceType::d8)
        maxVal *= 8;
    else if (diceType == DiceType::d10) 
        maxVal *= 9;
    else if (diceType == DiceType::d12) 
        maxVal *= 12;
    else if (diceType == DiceType::d20) 
        maxVal *= 20;
    else 
        throw "Invalid dyce type exception!";
    

    for (size_t i = 0; i <= maxVal; i++) {
        myfile << "Roll sum " << i << ": " << resultFrequency[i];
        if (resultFrequency[i] != 0)
            myfile << "   >>> Frequency: " << ((double)resultFrequency[i] / (double)M);
        myfile << '\n';
    }
        
    myfile.close();
    cout << "See you next time!\n";
}