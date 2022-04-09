// Funshield constants

#ifndef FUNSHIELD_CONSTANTS_H__
#define FUNSHIELD_CONSTANTS_H__

// convenience constants for switching on/off
constexpr int ON = LOW;
constexpr int OFF = HIGH;

// 7-segs
constexpr int latch_pin = 4;
constexpr int clock_pin = 7;
constexpr int data_pin = 8;

// buzzer
constexpr int beep_pin = 3;

// LEDs
constexpr int led1_pin = 13;
constexpr int led2_pin = 12;
constexpr int led3_pin = 11;
constexpr int led4_pin = 10;

// buttons
constexpr int button1_pin = A1;
constexpr int button2_pin = A2;
constexpr int button3_pin = A3;

// trimr
constexpr int trimr_pin = A0;

// digits
constexpr int digits[11] = { 0xc0, 0xf9, 0xa4, 0xb0, 0x99, 0x92, 0x82, 0xf8, 0x80, 0x90, 0xff };
constexpr int digit_muxpos[4] = { 0x01, 0x02, 0x04, 0x08 };

constexpr int buttonPins[]{ button1_pin, button2_pin, button3_pin };
constexpr int buttonPinsCount = sizeof(buttonPins) / sizeof(buttonPins[0]);
constexpr int displayDigits = 4;
constexpr int digit_positions = sizeof(digit_muxpos) / sizeof(digit_muxpos[0]);
constexpr size_t displayLimit = 10000;

byte segmentMap[] = {
  0xC0, // 0  0b11000000
  0xF9, // 1  0b11111001
  0xA4, // 2  0b10100100
  0xB0, // 3  0b10110000
  0x99, // 4  0b10011001
  0x92, // 5  0b10010010
  0x82, // 6  0b10000010
  0xF8, // 7  0b11111000
  0x80, // 8  0b10000000
  0x90  // 9  0b10010000
};

#endif