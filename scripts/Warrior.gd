extends Sprite

const CELL_SIZE = 64
const SPEED = 5
const MOVE_RANGE = 5
var move_position = null
var path = []

func move(new_path: Array) -> void:
	for cell in new_path:
		var x = (int(cell.x) * CELL_SIZE) + (CELL_SIZE / 2)
		var y = (int(cell.y) * CELL_SIZE) + (CELL_SIZE / 2)
		path.append(Vector2(x, y))
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
