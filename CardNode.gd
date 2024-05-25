class_name CardNode
extends Sprite2D

signal card_pressed(id)

var id: int
var face_texture: Texture2D
var back_texture: Texture2D
var eligible: int = -1  # -1 = eligibility turned off

# new/_init funcs are not automatically called when a scene is instantiated.
func setup(card: Card) -> void:
	self.id = card.id
	self.name = str(card.id)
	face_texture = load(self.tex_path_for(card))
	back_texture = load("res://images/cards/back.png")
	texture = face_texture # start face down
	scale = Vector2(0.5, 0.5)
	self.set_points(card.points)
	self.set_eligibility(-1)
	
func tex_path_for(card: Card) -> String:
	var path = 'res://images/cards/'
	
	var suit = ''
	match card.suit:
		Card.Suit.CLUB:
			suit = 'clb'
		Card.Suit.DIAMOND:
			suit = 'dia'
		Card.Suit.HEART:
			suit = 'hrt'
		Card.Suit.SPADE:
			suit = 'spd'
		Card.Suit.JOKER:
			suit = 'joker'
	path += suit
	
	if card.suit != Card.Suit.JOKER:
		path += str(int(card.rank))
		
	path += '.png'
	return path

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
	
func width() -> float:
	return texture.get_width() * scale.x
	
func set_points(points: int) -> void:
	if points == 0:
		$PointsLabel.text = ""
	else:
		$PointsLabel.text = str(points) + " pts"
	
func set_eligibility(_eligible: int):
	self.eligible = _eligible
	match self.eligible:
		-1:
			self.modulate = Color.WHITE
		0:
			self.modulate = Color.GRAY
		1:
			self.modulate = Color.WHITE
	
func set_face_up(up: bool):
	if up:
		texture = face_texture
	else:
		texture = back_texture
	$PointsLabel.visible = up

# Pick up any events that haven't been captured by controls ('unhandled').
func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		# Convert the event position to local, including rotation.
		if get_rect().has_point(to_local(event.position)):
			emit_signal("card_pressed", self.id)
			# Stop propagation of event.
			get_tree().get_root().set_input_as_handled()
