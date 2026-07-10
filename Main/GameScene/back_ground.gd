class_name BackGroundGd
extends AnimatedSprite2D

func _ready() -> void:
	var image := Image.load_from_file(ProjectSettings.globalize_path("res://assets/background.png"))
	if image == null or image.is_empty(): return
	var frames := SpriteFrames.new()
	frames.add_animation("BackGround")
	frames.add_frame("BackGround", ImageTexture.create_from_image(image))
	frames.set_animation_loop("BackGround", true)
	sprite_frames = frames
	animation = "BackGround"
	play("BackGround")
