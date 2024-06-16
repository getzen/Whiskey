class_name Player

enum PlayerKind {HUMAN, BOT}

var is_bot: bool
var hand: Array[Card] = []
var tricks: Array[Card] = []
var hand_points: int
var out_of_suits: Dictionary

func make_copy() -> Player:
	var p = Player.new()
	p.is_bot = self.is_bot
	for card in self.hand:
		p.hand.push_back(card.make_copy())
	for card in self.tricks:
		p.tricks.push_back(card.make_copy())
	p.hand_points = self.hand_points
	p.out_of_suits = self.out_of_suits.duplicate(true)
	return p 

func reset_for_new_hand():
	self.hand.clear()
	self.tricks.clear()
	self.hand_points = 0
	self.out_of_suits.clear()
	
func sort_hand():
	self.hand.sort_custom(func(a, b): return a.id < b.id)

func take_trick(cards: Array[Card]) -> void:
	for card in cards:
		self.hand_points += card.points
		self.tricks.push_back(card)
		
func set_out_of_suit(suit: Card.Suit):
	self.out_of_suits[suit] = 1
		
func is_out_of_suit(suit: Card.Suit) -> bool:
	return self.out_of_suits.has(suit)
