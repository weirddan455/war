extends Sprite
class_name Unit

const SPEED := 5
var move_position = null
var path := []

onready var tile_map :TileMap = get_node("/root/World/TileMap")
onready var cell := tile_map.world_to_map(position)

func move(new_path: Array) -> void:
	cell = new_path.back()
	for new_position in new_path:
		path.append(tile_map.map_to_world_center(new_position))
	set_process(true)

func _ready():
	set_process(false)

func _process(delta):
	if move_position == null:
		if path.empty():
			set_process(false)
			return
		move_position = path.pop_front()
	if move_position.x > position.x:
		if move_position.x > position.x + SPEED:
			position.x += SPEED
		else:
			position.x = move_position.x
	elif move_position.x < position.x:
		if move_position.x < position.x - SPEED:
			position.x -= SPEED
		else:
			position.x = move_position.x
	if move_position.y > position.y:
		if move_position.y > position.y + SPEED:
			position.y += SPEED
		else:
			position.y = move_position.y
	elif move_position.y < position.y:
		if move_position.y < position.y - SPEED:
			position.y -= SPEED
		else:
			position.y = move_position.y
	if position == move_position:
		move_position = null
