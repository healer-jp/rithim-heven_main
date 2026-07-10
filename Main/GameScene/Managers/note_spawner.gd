class_name NoteSpawnerGd
extends Node

@export var note_scene: PackedScene

var manager: GameManagerGd
var chart_manager: ChartManagerGd
var container: Node2D

func _ready() -> void:
	manager = get_node("../../GameManager")
	chart_manager = get_node("../ChartManager")
	container = get_node("../NoteContainer")
	manager.state_changed.connect(_on_state_changed)

func _process(_delta: float) -> void:
	if manager.state not in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]: return
	if chart_manager.create_note >= chart_manager.chart[0].size(): return
	var index := chart_manager.create_note
	var lead_time := 1000.0 * (60.0 / float(chart_manager.bpm))
	if chart_manager.chart[0][index] - lead_time <= chart_manager.get_time():
		var note := note_scene.instantiate() as NotesGd
		note.note_type = chart_manager.chart[1][index]
		note.note_id = index
		container.add_child(note)
		chart_manager.create_note += 1

func _on_state_changed(new_state: int) -> void:
	if new_state in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]:
		for child in container.get_children(): child.queue_free()
