extends Sprite2D

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
