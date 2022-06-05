# Arduino D&D Dice

- Used Model-View design pattern. Model is class DDDice, View is using class Button to read input and class FunshieldOutput to signal output. These two classes (Button and FunshieldOutput) are handling the Arduino IO specifics and enable class View to just control the situation using C++ logic only.

- Model is unaware of View. 

- There are two things I have done outside the scope of assignment:

  1. There are two different animations during 'dice roll'. One is displaying past Rolls the other duration of the roll in milliseconds. You can toggle between them as you leave config mode. While having the button1 pressed down press button3 which performs the the toggle. (Be careful with unreliable buttons, sometimes this needs to be repeated due to hardware.)

     Arduino signals which animation is currently on by displaying either 'd' or 'd.' . 
     'd' -> Display past rolls.
     'd.' -> Duration of rolls in ms.

  2. I have enumerated all probabilities of all possible roll outcomes. (Yes even the 9d20, which consists of possible: 20^9 outcomes.) It needed to figure out some equations like return all possible partitions for some outcome and than equation of how many different combinations are there for such partitions. 

     The code can be found here: [NSWI170-Computer-systems-project/Program.cs at main · Michael-Bohin/NSWI170-Computer-systems-project (github.com)](https://github.com/Michael-Bohin/NSWI170-Computer-systems-project/blob/main/01-DDDice-Statistics-Calculator/Program.cs)