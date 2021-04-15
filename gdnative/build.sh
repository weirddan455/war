gcc -std=c11 -fPIC -O2 -I$GODOT_HEADERS -rdynamic -shared pathfinding.c -o libpathfinding.so
