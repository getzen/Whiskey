class_name CardNode
extends Sprite2D

signal mouse_entered(id)
signal mouse_exited(id)

var id: int
var face_texture: Texture2D
var back_texture: Texture2D
var eligible: int = -1  # -1 = eligibility turned off
var is_joker: bool # needed for proper points display


# new/_init funcs are not automatically called when a scene is instantiated.
func setup(card: Card) -> void:
	self.id = card.id
	self.name = str(card.id)
	face_texture = load(self.tex_path_for(card))
	back_texture = load("res://images/cards/back.png")
	texture = face_texture # start face down
	scale = Vector2(0.5, 0.5)
	self.is_joker = card.suit == Card.Suit.JOKER
	self.set_points(card.points)
	self.set_eligibility(-1)
	
	# Set the Area2D click detection shape.
	var shape = RectangleShape2D.new() as RectangleShape2D
	shape.size = get_rect().size # get_rect() accounts for scale
	$Area2D/CollisionShape2D.shape = shape
	
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
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
	
func width() -> float:
	return texture.get_width() * scale.x
	
func set_points(points: int) -> void:
	if points == 0 && !self.is_joker:
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

func _on_area_2d_mouse_entered() -> void:
	emit_signal("mouse_entered", self.id)
	
func _on_area_2d_mouse_exited() -> void:
	emit_signal("mouse_exited", self.id)
