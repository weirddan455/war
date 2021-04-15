extends Node2D

const NUM_CELLS_X = 16
const NUM_CELLS_Y = 9
const CELL_SIZE = 64
var highlighted_cell = Rect2(Vector2(0, 0), Vector2(CELL_SIZE, CELL_SIZE))

func _unhandled_input(event):
	if event is InputEventMouseMotion:
		var mouse_cell = Vector2((int(event.position.x) / CELL_SIZE) * CELL_SIZE, (int(event.position.y) / CELL_SIZE) * CELL_SIZE)
		if highlighted_cell.position != mouse_cell:
			highlighted_cell.position = mouse_cell
			update()

func _draw():
	for i in range (NUM_CELLS_X - 1):
		draw_line(Vector2((CELL_SIZE - 1) + (i * CELL_SIZE), 0), Vector2((CELL_SIZE - 1) + (i * CELL_SIZE), CELL_SIZE * NUM_CELLS_Y), Color.black, 2, false)
	for i in range (NUM_CELLS_Y - 1):
		draw_line(Vector2(0, (CELL_SIZE - 1) + (i * CELL_SIZE)), Vector2(CELL_SIZE * NUM_CELLS_X, (CELL_SIZE - 1) + (i * CELL_SIZE)), Color.black, 2, false)
	draw_rect(highlighted_cell, Color.red, false, 2, false)
