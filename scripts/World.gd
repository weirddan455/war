extends Node2D

const NUM_CELLS_X = 16
const NUM_CELLS_Y = 9
const CELL_SIZE = 64
const DIRECTIONS = [Vector2.LEFT, Vector2.RIGHT, Vector2.UP, Vector2.DOWN]

var cells = []
var selected_unit = null
var selected_unit_cell_x = null
var selected_unit_cell_y = null
var movable_cells = null
var path = []

onready var Pathfinding = preload("res://gdnative/pathfinding.gdns").new()

func _ready():
	for x in range(NUM_CELLS_X):
		cells.append([])
		cells[x].resize(NUM_CELLS_Y)
	cells[int($Warrior.position.x) / CELL_SIZE][int($Warrior.position.y) / CELL_SIZE] = $Warrior

func _unhandled_input(event):
	if event is InputEventMouseButton && event.pressed:
		if event.button_index == BUTTON_LEFT:
			var cell_x = int(event.position.x) / CELL_SIZE
			var cell_y = int(event.position.y) / CELL_SIZE
			if cell_x < 0 || cell_x >= NUM_CELLS_X || cell_y < 0 || cell_y >= NUM_CELLS_Y:
				print("Clicked outside map")
			elif selected_unit != null:
				var vec = Vector2(cell_x, cell_y)
				if vec in movable_cells:
					if path.back() != vec:
						path.append(vec)
					if !is_path_valid():
						path = Pathfinding.get_new_path(selected_unit_cell_x, selected_unit_cell_y, cell_x, cell_y, $TileMap)
					selected_unit.move(path)
					path.clear()
					$PathOverlay.clear()
					$PathArrow.clear()
					cells[selected_unit_cell_x][selected_unit_cell_y] = null
					cells[cell_x][cell_y] = selected_unit
					selected_unit = null
					selected_unit_cell_x = null
					selected_unit_cell_y = null
				else:
					print("Can't move there")
			elif cells[cell_x][cell_y] == null:
				print("No unit at cell " + str(cell_x) + ", " + str(cell_y))
			else:
				selected_unit = cells[cell_x][cell_y]
				selected_unit_cell_x = cell_x
				selected_unit_cell_y = cell_y
				var vec = Vector2(cell_x, cell_y)
				path.append(vec)
				$PathArrow.draw(vec)
				movable_cells = Pathfinding.get_movable_cells(cell_x, cell_y, selected_unit.MOVE_RANGE, $TileMap)
				$PathOverlay.draw(movable_cells)
		elif event.button_index == BUTTON_RIGHT:
			if selected_unit != null:
				path.clear()
				$PathOverlay.clear()
				$PathArrow.clear()
				selected_unit = null
				selected_unit_cell_x = null
				selected_unit_cell_y = null

	elif event is InputEventMouseMotion && selected_unit != null:
		var cell_x = int(event.position.x) / CELL_SIZE
		var cell_y = int(event.position.y) / CELL_SIZE
		var vec = Vector2(cell_x, cell_y)
		if vec in movable_cells && path.back() != vec:
			path.append(vec)
			if is_path_valid():
				$PathArrow.draw(vec)
			else:
				path = Pathfinding.get_new_path(selected_unit_cell_x, selected_unit_cell_y, cell_x, cell_y, $TileMap)
				$PathArrow.clear()
				for cell in path:
					$PathArrow.draw(cell)

func is_path_valid() -> bool:
	var cost = 0
	for i in range(1, path.size()):
		if path[i].distance_squared_to(path[i-1]) != 1:
			return false
		match $TileMap.get_cellv(path[i]):
			0:
				cost += 1
			1:
				cost += 2
			_:
				cost += 9001
	return cost <= selected_unit.MOVE_RANGE
