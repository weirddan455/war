extends TileMap

func is_inside_map(cell_position: Vector2) -> bool:
	var rect := get_used_rect()
	if cell_position.x < 0 || cell_position.x >= rect.size.x || cell_position.y < 0 || cell_position.y >= rect.size.y:
		return false
	return true

func map_to_world_center(position: Vector2) -> Vector2:
	return map_to_world(position) + (cell_size / 2)
