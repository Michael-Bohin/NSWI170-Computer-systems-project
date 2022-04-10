#include "funshield-constants.h"

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
};

#endif