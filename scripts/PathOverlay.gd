extends TileMap

func _ready():
	var image = Image.new()
	image.create(64, 64, false, Image.FORMAT_RGBA8)
	image.fill(Color(1, 1, 0, 0.5))
	var image_texture = ImageTexture.new()
	image_texture.create_from_image(image)
	tile_set = TileSet.new()
	tile_set.create_tile(0)
	tile_set.tile_set_texture(0, image_texture)

func draw(cells) -> void:
	for cell in cells:
		set_cellv(cell, 0)
