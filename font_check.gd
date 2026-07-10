extends SceneTree

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var scene := load("res://Main/game_scene.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	var checked := 0
	for node in scene.find_children("*", "Control", true, false):
		if node is Label or node is Button or node is RichTextLabel:
			var item := "normal_font" if node is RichTextLabel else "font"
			var font := node.get_theme_font(item)
			if font == null or "NotoSansJP" not in font.resource_path:
				push_error("FONT_CHECK_FAILED: %s uses %s" % [node.get_path(), font.resource_path if font != null else "null"])
				quit(1)
				return
			checked += 1
	print("FONT_CHECK_PASSED controls=%d" % checked)
	scene.queue_free()
	await process_frame
	quit()
