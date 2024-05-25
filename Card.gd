class_name Card

# NONE is used when bidding to pass.
enum Suit {CLUB, DIAMOND, HEART, SPADE, JOKER, NONE}

var id: int
var suit: Suit
var rank: float # float allows for 10.5 Joker rank
var points: int
var face_up: bool
var eligible: int # -1 == off, 0 == ineligible, 1 = eligible

# Called when Card.new(...) is called.
func _init(_id: int, _suit: Suit, _rank: float, _points: int) -> void:
	self.id = _id
	self.suit = _suit
	self.rank = _rank
	self.points = _points
	self.face_up = false
	self.eligible = -1
