extends Node2D

var player_units = {}
var selected_unit = null
var movable_cells = null
var path = []

onready var Pathfinding = preload("res://gdnative/pathfinding.gdns").new()

func _ready():
	for unit in $PlayerUnits.get_children():
		player_units[unit.cell] = unit

func move_selected_unit(cell_position: Vector2) -> void:
	if path.back() != cell_position:
		path.append(cell_position)
	if !is_path_valid():
		path = Pathfinding.get_new_path(selected_unit.cell.x, selected_unit.cell.y, cell_position.x, cell_position.y, $TileMap)
	player_units.erase(selected_unit.cell)
	selected_unit.move(path)
	player_units[selected_unit.cell] = selected_unit
	path.clear()
	$PathOverlay.clear()
	$PathArrow.clear()
	selected_unit = null

func select_unit(cell_position: Vector2) -> void:
	selected_unit = player_units[cell_position]
	path.append(cell_position)
	$PathArrow.draw(cell_position)
	movable_cells = Pathfinding.get_movable_cells(cell_position.x, cell_position.y, selected_unit.MOVE_RANGE, $TileMap)
	$PathOverlay.draw(movable_cells)

func cancel_selection() -> void:
	path.clear()
	$PathOverlay.clear()
	$PathArrow.clear()
	selected_unit = null

func _unhandled_input(event):
	if event is InputEventMouseButton && event.pressed:
		if event.button_index == BUTTON_LEFT:
			var cell_position = $TileMap.world_to_map(event.position)
			if !$TileMap.is_inside_map(cell_position):
				print("Clicked outside map")
			elif selected_unit != null:
				if cell_position in movable_cells:
					move_selected_unit(cell_position)
				else:
					print("Can't move there")
			elif !player_units.has(cell_position):
				print("No friendly unit at cell " + str(cell_position.x) + ", " + str(cell_position.y))
			else:
				select_unit(cell_position)
		elif event.button_index == BUTTON_RIGHT && selected_unit != null:
			cancel_selection()

	elif event is InputEventMouseMotion && selected_unit != null:
		var cell_position = $TileMap.world_to_map(event.position)
		if cell_position in movable_cells && path.back() != cell_position:
			path.append(cell_position)
			if is_path_valid():
				$PathArrow.draw(cell_position)
			else:
				path = Pathfinding.get_new_path(selected_unit.cell.x, selected_unit.cell.y, cell_position.x, cell_position.y, $TileMap)
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

func _on_EndTurn_pressed():
	for unit in $AIUnits.get_children():
		var ai_path = []
		ai_path.append(unit.cell + Vector2(-1, 0))
		ai_path.append(unit.cell + Vector2(-2, 0))
		ai_path.append(unit.cell + Vector2(-2, -1))
		unit.move(ai_path)
