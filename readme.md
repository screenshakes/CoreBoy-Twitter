# TwitterBoy

TwitterBoy is a bot that runs the [CoreBoy](https://github.com/davidwhitney/CoreBoy) emulator and uses your profile picture as display and comments of a tweet as inputs.

# Pre-Reqs
.NET Core 3.1

# How to run
1) Compile the TwitterBoy csproj
2) Copy the settings_example and credentials_example files in the same folder as your executable
3) Remove the "_example" from both filename
4) In the settings file, replace the tweet id by your tweet
5) In the credentials file, replace all the A's by your credentials
6) Run the executable from the command line and provide your rom like so: TwitterBoy.exe game.gb

# Controls
- Q to toggle between manual and comments mode
- W to reload the settings

In manual mode:
- DPad - Arrow keys
- A - Z
- B - X
- Start - C
- Select - V

# Game Specific
## Pokemon Red / Blue
There is a [pokemon](https://github.com/screenshakes/TwitterBoy/tree/pokemon) branch that provide a class to automatically generate and upload a team status as your Twitter banner.\
To use this you need to add a folder named "sprites" in the same folder as your executable and place png sprites named from 1 to 151.\
You also need to add the font you want to use, named "font.ttf", in the same folder.\
It also uses the memory editor to continually force the instant text glitch.
