#include "model.h"
#include "funshield-control.h"

#ifndef VIEW_H
#define VIEW_H

class View {
public:
    View(DDDice _dice) : dice(_dice), btn1(0), btn2(1), btn3(2) {}
    
    void UpdateBtns() {
        btn1.UpdateEvent();
        btn2.UpdateEvent();
        btn3.UpdateEvent();
    }

    void UpdateState() {
        if (btn1.state == CLICK_DOWN && normalMode) 
            initRoll();

        if (btn1.state == RELEASED) 
            finishRoll();

        if (btn2.state == CLICK_DOWN) 
            incrementThrows();

        if (btn3.state == CLICK_DOWN) 
            incrementDiceType();

        if (btn1.state == PRESSED_ON && !normalMode && btn3.state == CLICK_DOWN) {
            animateTime = !animateTime;
            invalidateNormalModeTransform = true;
        }
    }
    
    void SetOutput() {
        if (normalMode)
            displayNormalMode();
        else 
            out.displayConfigMode(throws, animateTime, diceType);
        out.incrementMultiplex();
    }

private:
    DDDice dice;
    Button btn1, btn2, btn3;
    FunshieldOutput out;

    size_t rollSum = 38;
    bool normalMode = true;
    bool invalidateNormalModeTransform = false;
    unsigned long rollTime = 0;

    size_t throws = 1;
    DiceType diceType = d4;

    // data fields for animation:
    bool animating = false;
    size_t pastRollSum = 0;
    unsigned long pastRollTime = 0;
    bool animateTime = false;


    void initRoll() { // btn1 clicked down
        animating = true;
        rollTime = micros(); // start the measruement of time when the button is pressed down
    }

    void finishRoll() { // btn1 released
        if (normalMode) {
            animating = false;
            rollSum = dice.Roll(diceType, throws, micros() - rollTime);
        } else {
            if (!invalidateNormalModeTransform) 
                normalMode = true;
            else 
                invalidateNormalModeTransform = false;
        }
    }

    void incrementThrows() { // btn2 clicked down
        if (normalMode) 
            normalMode = false;
        else 
            throws = throws == 9 ? 1 : throws += 1; // incrementaly cycle 1-9
    }

    void incrementDiceType() {
        if (normalMode) {
            normalMode = false;
        } else { // incrementaly cycle 0-6:
            size_t diceId = (size_t)diceType;
            diceId = diceId == 6 ? 0 : ++diceId;
            diceType = (DiceType)diceId;
        }
    }

    void displayNormalMode() {
        if (animating) {            
            if (animateTime) 
                animateMilliseconds();
            else 
                animatePastRolls();
        } else
            out.writeNumber(rollSum);
    }

    void animateMilliseconds() {
        unsigned long time = micros() - rollTime;
        time /= 1000; // convert to micros
        time %= displayLimit; // modulo limit of display -> 10 seconds
        out.writeNumber(time);
    }

    void animatePastRolls() {
        if (pastRollTime < (millis() - 200)) { // roll each 200milis = 200 000 micros
            pastRollTime = millis();
            pastRollSum = dice.Roll(diceType, throws, micros() - rollTime);
        }
        out.writeNumber(pastRollSum);
    }
};

#endif