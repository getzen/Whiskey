class_name Player
extends Node

enum PlayerKind {HUMAN, BOT}

var is_bot: bool
var hand: Array[Card] = []
var tricks: Array[Card] = []
var hand_points: int

func make_copy() -> Player:
	var p = Player.new()
	p.is_bot = self.is_bot
	for card in self.hand:
		p.hand.push_back(card.make_copy())
	for card in self.tricks:
		p.tricks.push_back(card.make_copy())
	p.hand_points = self.hand_points
	return p 

func reset_for_new_hand():
	self.hand.clear()
	self.tricks.clear()
	self.hand_points = 0
	
func take_trick(cards: Array[Card]) -> void:
	for card in cards:
		self.hand_points += card.points
		self.tricks.push_back(card)
