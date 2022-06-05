#include "view.h"

void setup() {
    for (int i = 0; i < buttonPinsCount; ++i) 
        pinMode(buttonPins[i], INPUT);
    pinMode(latch_pin, OUTPUT);
    pinMode(clock_pin, OUTPUT);
    pinMode(data_pin, OUTPUT);
}

DDDice dice;
View view {dice};

void loop() {
    view.UpdateBtns();
    view.UpdateState();
    view.SetOutput();
}
