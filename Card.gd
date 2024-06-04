class_name Card

# NONE is used when bidding to pass.
enum Suit {CLUB, DIAMOND, HEART, SPADE, JOKER, NONE}

var id: int
var suit: Suit
var rank: float # float allows for 10.5 Joker rank
var points: int
var face_up: bool

# Called when Card.new(...) is called.
func _init(_id: int, _suit: Suit, _rank: float, _points: int) -> void:
	self.id = _id
	self.suit = _suit
	self.rank = _rank
	self.points = _points
	self.face_up = false
	
func make_copy() -> Card:
	var c = Card.new(self.id, self.suit, self.rank, self.points)
	return c
