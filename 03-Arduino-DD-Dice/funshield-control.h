#include "funshield-constants.h"

#ifndef DICETYPE_
#define DICETYPE_
enum DiceType { d4, d6, d8, d10, d12, d20, d100 };
#endif

#ifndef FUNSHIELD_CONTROL
#define FUNSHIELD_CONTROL

enum ButtonEvent { NOT_PRESSED, CLICK_DOWN, PRESSED_ON, RELEASED };

class Button {
public:
    ButtonEvent state;

    Button(size_t i) : index(i), prev(false), actual(false) { }

    void UpdateEvent() {
        prev = actual;
        actual = !digitalRead(buttonPins[index]);
        if (prev)
            state = actual ? PRESSED_ON : RELEASED;
        else
            state = actual ? CLICK_DOWN : NOT_PRESSED;
    }

private:
    size_t index;
    bool prev;
    bool actual;
};

class FunshieldOutput {
public:
    void writeNumber(size_t number) {
        for (size_t i = 0; i < 4; i++) {
            writeDigit(number % 10, i);
            number /= 10;
            if (number == 0)
                break;
        }
    }
    void writeDigit(size_t n, size_t pos) { writeGlyphR(digits[n], pos); }
    void writeByte(byte glyph, size_t pos) { writeGlyphR(glyph, pos); }

    void displayConfigMode(size_t throws, bool animateTime, DiceType diceType) {
        writeDigit(throws, 3);
        if (animateTime)
            writeByte(0x21, 2); // 21 reprezentuje 'd.'
        else
            writeByte(0xA1, 2); // A1 reprezentuje 'd'
        writeDice(diceType);
    }

private:
    void writeGlyphR(byte glyph, size_t pos) {
        writeGlyphBitmask(glyph, digit_muxpos[digit_positions - pos - 1]);
    }

    void writeGlyphBitmask(byte glyph, byte pos_bitmask) {
        digitalWrite(latch_pin, LOW);
        shiftOut(data_pin, clock_pin, MSBFIRST, glyph);
        shiftOut(data_pin, clock_pin, MSBFIRST, pos_bitmask);
        digitalWrite(latch_pin, HIGH);
    }

    void writeDice(DiceType diceType) {
        switch (diceType) {
        case d4:
            writeDigit(4, 1); break;
        case d6:
            writeDigit(6, 1); break;
        case d8:
            writeDigit(8, 1); break;
        case d10:
            writeDigit(1, 1);
            writeDigit(0, 0);
            break;
        case d12:
            writeDigit(1, 1);
            writeDigit(2, 0);
            break;
        case d20:
            writeDigit(2, 1);
            writeDigit(0, 0);
            break;
        case d100:
            writeDigit(0, 1);
            writeDigit(0, 0);
            break;
        default:
            writeNumber(9999); // signal critical error in code
        }
    }
};

#endif