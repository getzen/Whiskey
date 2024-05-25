extends Sprite2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass

func set_suit(suit: Card.Suit) -> void:
	match suit:
		Card.Suit.CLUB:
			texture = load("res://images/club.png")
		Card.Suit.DIAMOND:
			texture = load("res://images/diamond.png")
		Card.Suit.HEART:
			texture = load("res://images/heart.png")
		Card.Suit.SPADE:
			texture = load("res://images/spade.png")
