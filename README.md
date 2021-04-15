# war
Work in progress prototype of a turn based strategy game using the Godot game engine (title subject to change)

## Licensing
The code is my own and is licensed under the MIT license.  Assets are *not* my own but are made available by their creators under free CC licences.  See assets/LICENSE for more information including attribution.

## To run
You will need to download/install the Godot game engine.  Once installed, you should be able to import the project into the editor.  I'm not releasing binaries at this early stage of development.  Pathfinding is implemented using a GDNative library writen in C.  Source code is available in gdnative/ but it must be compiled for the game to run.  For that, the Godot headers are required.  They can be found at https://github.com/godotengine/godot-headers

For Linux users, there is a build.sh script in gdnative/ for use with the GCC compiler.  Compiling should be as simple as:
```
git clone https://github.com/godotengine/godot-headers.git
export GODOT_HEADERS=(path to godot-headers)
./build.sh
```
Users of other platforms can find instructions at https://docs.godotengine.org/en/stable/tutorials/plugins/gdnative/gdnative-c-example.html
