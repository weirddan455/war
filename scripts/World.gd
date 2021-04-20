extends Node2D

enum {FIRE, WAIT}
enum {NOP, SELECT_UNIT, SELECT_ENEMY, DRAW_PATH, CANCEL, MOVE_UNIT}

const DIRECTIONS := [Vector2.LEFT, Vector2.RIGHT, Vector2.UP, Vector2.DOWN]

var player_units := {}
var enemy_units := {}
var selected_unit = null
var movable_cells = null
var path := []
var enemy_neighbors = null

onready var Pathfinding = preload("res://gdnative/pathfinding.gdns").new()

func _ready():
	for unit in $PlayerUnits.get_children():
		player_units[unit.cell] = unit
	for unit in $AIUnits.get_children():
		enemy_units[unit.cell] = unit

func combat(attacker, defender) -> void:
	defender.take_damage(attacker.ATTACK)
	attacker.take_damage(defender.ATTACK / 2)

func is_enemy_neighbor() -> bool:
	for direction in DIRECTIONS:
		if enemy_units.has(selected_unit.cell + direction):
			return true
	return false

func get_enemy_neighbors() -> Dictionary:
	var enemy_neighbors := {}
	for direction in DIRECTIONS:
		var neighbor :Vector2 = selected_unit.cell + direction
		if enemy_units.has(neighbor):
			enemy_neighbors[neighbor] = enemy_units[neighbor]
	return enemy_neighbors

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

func get_input_type(event):
	if $UI/CommandDialog.visible:
		return NOP
	if event is InputEventMouseButton && event.pressed:
		if event.button_index == BUTTON_LEFT:
			if enemy_neighbors != null:
				return SELECT_ENEMY
			if selected_unit != null:
				return MOVE_UNIT
			return SELECT_UNIT
		if event.button_index == BUTTON_RIGHT && selected_unit != null:
			return CANCEL
	if event is InputEventMouseMotion && selected_unit != null && enemy_neighbors == null:
		return DRAW_PATH
	return NOP

func _unhandled_input(event):
	if event is InputEventMouseButton || event is InputEventMouseMotion:
		var cell_position = $TileMap.world_to_map(event.position)
		match get_input_type(event):
			SELECT_UNIT:
				if player_units.has(cell_position):
					select_unit(cell_position)
			SELECT_ENEMY:
				if enemy_neighbors.has(cell_position):
					for unit in enemy_neighbors.values():
						unit.stop_animation()
					combat(selected_unit, enemy_neighbors[cell_position])
					enemy_neighbors = null
					selected_unit = null
			MOVE_UNIT:
				if movable_cells.has(cell_position):
					move_selected_unit(cell_position)
					if is_enemy_neighbor():
						$UI/CommandDialog/Fire.show()
					else:
						$UI/CommandDialog/Fire.hide()
					$UI/CommandDialog.rect_position = $TileMap.map_to_world_center(cell_position)
					$UI/CommandDialog.show()
			CANCEL:
				cancel_selection()
			DRAW_PATH:
				if movable_cells.has(cell_position) && path.back() != cell_position:
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

func _on_CommandDialog_pressed(command):
	if command == FIRE:
		enemy_neighbors = get_enemy_neighbors()
		for unit in enemy_neighbors.values():
			unit.strobe_animation()
	else:
		selected_unit = null
	$UI/CommandDialog.hide()

func unit_removed(unit):
	for cell in player_units:
		if player_units[cell] == unit:
			player_units.erase(cell)
			return
	for cell in enemy_units:
		if enemy_units[cell] == unit:
			enemy_units.erase(cell)
			return
