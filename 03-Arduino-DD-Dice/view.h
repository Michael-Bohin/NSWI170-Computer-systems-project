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
            btn1Released();

        if (btn2.state == CLICK_DOWN) 
            btn2ClickedDown();

        if (btn3.state == CLICK_DOWN) 
            btn3ClickedDown();
    }
    
    void SetOutput() {
        if (normalMode)
            displayNormalMode();
        else 
            displayConfigMode();
    }

private:
    DDDice dice;
    Button btn1, btn2, btn3;
    FunshieldOutput out;
    Animation animation;

    size_t rollSum = 38;
    bool normalMode = true;
    bool animating = false;
    unsigned long rollTime = 0;

    size_t throws = 1;
    size_t diceType = 0;
    DiceType types[7] {DiceType::d4, DiceType::d6, DiceType::d8, DiceType::d10, DiceType::d12, DiceType::d20, DiceType::d100};

    void initRoll() {
        animating = true;
        rollTime = micros(); // start the measruement of time when the button is pressed down
    }

    void btn1Released() {
        if (normalMode) {
            animating = false;
            rollSum = dice.Roll(types[diceType], throws, micros() - rollTime);
        } else {
            normalMode = true;
        }
    }

    void btn2ClickedDown() {
        if (normalMode) 
            normalMode = false;
        else 
            throws = throws == 9 ? 1 : throws += 1; // incrementaly cycle 1-9
    }

    void btn3ClickedDown() {
        if (normalMode) 
            normalMode = false;
        else 
            diceType = diceType == 6 ? 0 : diceType += 1; // incrementaly cycle 0-6
    }

    void displayNormalMode() {
        if (animating)
            out.writeNumber(8888);
        else
            out.writeNumber(rollSum);
    }

    void displayConfigMode() {
        out.writeDigit(throws, 3);
        out.writeByte(0xA1, 2); // A1 reprezentuje 'd'

        switch(diceType) {
        case 0: 
            out.writeDigit(4, 1); break;
        case 1:
            out.writeDigit(6, 1); break;
        case 2:
            out.writeDigit(8, 1); break;
        case 3:
            out.writeDigit(1, 1); 
            out.writeDigit(0, 0);
            break;
        case 4:
            out.writeDigit(1, 1);
            out.writeDigit(2, 0);
            break;
        case 5:
            out.writeDigit(2, 1);
            out.writeDigit(0, 0);
            break;
        case 6:
            out.writeDigit(0, 1);
            out.writeDigit(0, 0);
            break;
        default:
            out.writeNumber(9999); // signal critical error in code
        }
    }


};

#endif
