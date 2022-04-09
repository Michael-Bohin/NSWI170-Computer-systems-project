#include "view.h"

// the setup function runs once when you press reset or power the board
void setup() {
    for (int i = 0; i < buttonPinsCount; ++i) {
        pinMode(buttonPins[i], INPUT);
    }
    pinMode(latch_pin, OUTPUT);
    pinMode(clock_pin, OUTPUT);
    pinMode(data_pin, OUTPUT);
}

// the loop function runs over and over again until power down or reset
DDDice dice;
View view {dice};

void loop() {
    view.UpdateBtns();
    view.UpdateState();
    view.SetOutput();
}
