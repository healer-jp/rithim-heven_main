extends SceneTree


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	var packed_scene: PackedScene = load(
		"res://Main/game_scene.tscn"
	) as PackedScene

	if packed_scene == null:
		push_error(
			"SCENE_LOAD_FAILED: res://Main/game_scene.tscn"
		)
		quit(1)
		return

	var scene: Node = packed_scene.instantiate()
	root.add_child(scene)

	await process_frame

	var checked: int = 0

	for node in scene.find_children("*", "Control", true, false):
		if node is Label or node is Button or node is RichTextLabel:
			var control: Control = node as Control

			var item: String = (
				"normal_font"
				if control is RichTextLabel
				else "font"
			)

			var font: Font = control.get_theme_font(item)

			if font == null or "NotoSansJP" not in font.resource_path:
				var font_path: String = (
					font.resource_path
						if font != null
						else "null"
				)

				push_error(
					"FONT_CHECK_FAILED: %s uses %s"
					% [control.get_path(), font_path]
				)

				quit(1)
				return

			checked += 1

	print("FONT_CHECK_PASSED controls=%d" % checked)

	scene.queue_free()
	await process_frame

	quit()
