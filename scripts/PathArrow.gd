extends TileMap

func draw(cell: Vector2):
	set_cellv(cell, 0)
	update_bitmask_area(cell)
