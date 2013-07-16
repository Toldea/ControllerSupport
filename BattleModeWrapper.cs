using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


namespace ControllerSupport {
	public class BattleModeWrapper {
		private BattleMode battleMode;
		private HandManager handManager;
		private MethodInfo cardClickedMethodInfo;
		private MethodInfo toggleUnitStatsMethodInfo;
		private MethodInfo deselectAllTilesMethodInfo;
		private GUIBattleModeMenu gameMenu;
		private bool controlBoard;
		private int tileRow;
		private int tileColumn;
		private float lastPlayedCardTimeStamp = -1000f;
		private float viewingCardTimeStamp = -1000f;

		public BattleModeWrapper (BattleMode battleMode) {
			Initialize (battleMode);
		}

		public bool Validate(BattleMode battleMode) {
			if (this.battleMode == null || this.handManager == null) {
				Console.WriteLine ("ControllerSupport: BattleModeWrapper.Validate: BattleMode or HandManager are invalid, reinitializing..");
				Initialize (battleMode);
				return true;
			}
			return false;
		}

		private void Initialize(BattleMode battleMode) {
			this.battleMode = battleMode;
			this.handManager = GetHandManager ();
			cardClickedMethodInfo = battleMode.GetType().GetMethod("cardClicked", BindingFlags.NonPublic | BindingFlags.Instance);
			toggleUnitStatsMethodInfo = battleMode.GetType().GetMethod("toggleUnitStats", BindingFlags.NonPublic | BindingFlags.Instance);
			deselectAllTilesMethodInfo = battleMode.GetType ().GetMethod ("deselectAllTiles", BindingFlags.NonPublic | BindingFlags.Instance);
			gameMenu = (GUIBattleModeMenu)typeof(BattleMode).GetField ("menu", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
			controlBoard = false;
			tileRow = 2;
			tileColumn = 1;
		}

		public void CycleShowRecentlyPlayedCards() {
			// Get a list of all recently played scrolls (both ours and the enemies!).
			TileColor leftColor = (battleMode.isLeftColor(TileColor.black)) ? TileColor.black : TileColor.white;
			TileColor rightColor = (leftColor == TileColor.black) ? TileColor.white : TileColor.black;
			List<Transform> leftSpellList = (List<Transform>)battleMode.GetType().GetMethod("getSpellListFor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(battleMode, new object[]{leftColor});
			List<Transform> rightSpellList = (List<Transform>)battleMode.GetType().GetMethod("getSpellListFor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(battleMode, new object[]{rightColor});
			List<Transform> spellList = new List<Transform> ();
			spellList.AddRange (leftSpellList);
			spellList.AddRange (rightSpellList);
			if (spellList.Count == 0) {
				return;
			}
			// Sort them by their played timestamp.
			spellList.Sort(delegate(Transform spellObject1, Transform spellObject2) {
				CardView spell1CardView = spellObject1.GetComponent<CardView> ();
				CardView spell2CardView = spellObject2.GetComponent<CardView> ();
				float spell1StartTime = (float)typeof(CardView).GetField ("startTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (spell1CardView);
				float spell2StartTime = (float)typeof(CardView).GetField ("startTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (spell2CardView);
				return (spell2StartTime.CompareTo(spell1StartTime));
			});

			// Get the timestamp from the most currently played card.
			CardView lastPlayedCard = spellList[0].GetComponent<CardView>();
			float newLastPlayedCardTimeStamp = (float)typeof(CardView).GetField ("startTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (lastPlayedCard);
			int viewingCardIndex = 0;
			// If we don't have a valid lastPlayedCard time stamp or if a newer card has been played since then, set our currently viewing card stamp to the latest card stamp.
			if (lastPlayedCardTimeStamp < .0f || lastPlayedCardTimeStamp < newLastPlayedCardTimeStamp) {
				lastPlayedCardTimeStamp = newLastPlayedCardTimeStamp;
				viewingCardTimeStamp = newLastPlayedCardTimeStamp;
				viewingCardIndex = 0;
			} else {
				// We have a valid and up to date time stamp for the last played card, cycle to the next card in the list.
				float timeStamp;
				for (int i = 1; i < spellList.Count; i++) {
					// Loop through each spell until we hit one with a time stamp earlier as our viewingCardTimeStamp.
					CardView cv = spellList [i].GetComponent<CardView>();
					timeStamp = (float)typeof(CardView).GetField ("startTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (cv);
					if (timeStamp < viewingCardTimeStamp) {
						viewingCardTimeStamp = timeStamp;
						viewingCardIndex = i;
						break;
					}
					// If we hit the last spell and it didn't had an earlier time stamp, cycle back to the first one.
					if (i == spellList.Count - 1) {
						viewingCardTimeStamp = newLastPlayedCardTimeStamp;
						viewingCardIndex = 0;
					}
				}
			}

			// Select the card we want to view next.
			CardView cardView = spellList[viewingCardIndex].GetComponent<CardView> ();
			battleMode.GetType().GetMethod("showCardRule", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(battleMode, new object[]{cardView.getCardInfo()});
			typeof(BattleMode).GetField ("unitRuleShowing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleMode, true);
		}

		public void CardClicked(CardView cardView, int mouseButton) {
			if (cardView != null) {
				cardClickedMethodInfo.Invoke (battleMode, new object[] { cardView, mouseButton });
			}
		}

		public ResourceType[] GetResourceTypes() {
			return (ResourceType[])typeof(BattleMode).GetField ("resTypes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
		}

		public void EndTurnPressed() {
			if (!TurnEnded ()) {
				battleMode.endturnPressed ();
			}
		}

		public void ShowMenu() {
			gameMenu.toggleMenu ();
		}
		public bool ShowingMenu() {
			return gameMenu.showMenu;
		}
		public GUIBattleModeMenu GetMenu() {
			return gameMenu;
		}

		public void ToggleUnitStats() {
			toggleUnitStatsMethodInfo.Invoke (battleMode, new object[] { });
		}

		public HandManager GetHandManager() {
			return (HandManager)typeof(BattleMode).GetField ("handManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
		}

		public bool InControlOfBoard() {
			return controlBoard;
		}
		public bool InControlOfHand() {
			return !controlBoard;
		}
		public void TakeControlOfBoard() {
			controlBoard = true;
			// Check if the currently selected card (if we have one) is a creature or a structure.
			// If so, limit the current/last board position to just the left side of the board.
			CardView card = handManager.GetSelectedCard ();
			if (card != null) {
				CardType.Kind type = card.getCardInfo ().getPieceKind ();
				if (type == CardType.Kind.CREATURE || type == CardType.Kind.STRUCTURE) {
					ConstrainBoardPosition(false);
				}
			}
			TileOver ();
			handManager.SetCardsGrayedOut (true);
		}
		public void TakeControlOfHand() {
			controlBoard = false;
			DeselectAllTiles ();
			battleMode.HideCardView ();
			handManager.SetCardsGrayedOut (false);
		}

		public void MoveOnBoard(int x, int y) {
			// Are we allowed to move on the right board (disabled for placing units / structures).
			bool allowRightBoardMovement = true;

			// Get the selected card.
			CardView card = handManager.GetSelectedCard ();
			if (card != null) {
				// Get the selected card type.
				CardType.Kind type = card.getCardInfo ().getPieceKind ();
				if (type == CardType.Kind.CREATURE || type == CardType.Kind.STRUCTURE) {
					allowRightBoardMovement = false;
				}
			} else {
				// Check if the currently active ability equals 'move', if so limit the board movement to just 'our' side.
				string activeAbilityId = (string)typeof(BattleMode).GetField ("activeAbilityId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
				if (activeAbilityId == "Move") {
					allowRightBoardMovement = false;
				}
			}

			TileOut (); // Fade out the last tile.
			// Add the displacement.
			tileColumn += x;
			tileRow += y;

			// Constrain the position within the board boundries and limit to just the left side if needed.
			ConstrainBoardPosition (allowRightBoardMovement);
			TileOver (); // Fade in the new tile.
		}

		public void ConstrainBoardPosition(bool allowRightBoardPosition) {
			// Make sure the row/column stays within the board.
			if (allowRightBoardPosition) {
				if (tileColumn < -3) tileColumn = -3;
			} else {
				if (tileColumn < 0) tileColumn = 0;
			}
			if (tileColumn > 2) tileColumn = 2;
			if (tileRow < 0) tileRow = 0;
			if (tileRow > 4) tileRow = 4;
		}

		public void TileClicked (HandManagerWrapper handManagerWrapper) {
			Tile tile = GetTile ();
			if (tile != null) {
				// Get the list of all valid tiles to do the currently active ability on (play scroll, move, etc.).
				List<List<Tile>> tileSelectionList = new List<List<Tile>> ((List<List<Tile>>)typeof(BattleMode).GetField ("tileSelectionList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode));
				bool didPlaceUnit = false;
				bool didActivateAbility = false;
				// Check if we are trying to play a card from hand on the battlefield (i.e. units / targeted spells / enchantments).
				if (handManager.GetSelectedCard () != null) {
					if (battleMode.isTileInList (tile, tileSelectionList)) {
						tileSelectionList.RemoveAt (0);
						if (tileSelectionList.Count == 0) {
							didPlaceUnit = true;
						}
					}
				} else {
					// Get the position of the currently active ability and check if it is the same as the currently selected tile.
					TilePosition activeAbilityPosition = (TilePosition)typeof(BattleMode).GetField ("activeAbilityPosition", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
					if (activeAbilityPosition.Equals (battleMode.getPosition (tile))) {
						GameObject cardRule = (GameObject)typeof(BattleMode).GetField ("cardRule", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
						if (cardRule != null) {
							CardView cardView = cardRule.GetComponent<CardView> ();
							if (cardView != null) {
								// Disect the scroll's activatable abilities from it. If it contains a non-move ability (i.e. summon wolf), activate it.
								ActiveAbility[] activeAbilities = cardView.getCardInfo ().getActiveAbilities ();
								if (activeAbilities != null) {
									for (int j = 0; j < activeAbilities.Length; j++) {
										ActiveAbility activeAbility = activeAbilities [j];
										if (!(activeAbility.name == "Move")) {
											TilePosition pos = (TilePosition)typeof(CardView).GetField ("pos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (cardView);
											if (pos != null) {
												battleMode.ActivateTriggeredAbility (activeAbility.id, pos);
												battleMode.HideCardView ();
												didActivateAbility = true;
											}
										}
									}
								}
							}
						}
					}
				}
				// Reserve the next card if we will place a unit on the board with this click.
				if (didPlaceUnit) {
					handManagerWrapper.ReserveNextCard ();
				}
				// 'Click' on the currently highlighted tile if we didn't activate a unit's activated ability.
				if (!didActivateAbility) {
					battleMode.tileClicked(tile);
				}
				// Return control back to the hand if we placed a unit.
				if (didPlaceUnit) {
					TakeControlOfHand ();
				}
			}
		}

		public bool UnitSelectedOnBoard() {
			if (handManager.GetSelectedCard () == null) {
				string activeAbilityId = (string)typeof(BattleMode).GetField ("activeAbilityId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
				if (activeAbilityId != string.Empty && activeAbilityId != null) {
					return true;
				}
			}
			return false;
		}

		public void ShowChat() {
			battleMode.GetType ().GetMethod ("setChatActive", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] {typeof(bool)}, null).Invoke (battleMode, new object[] {true});
			typeof(BattleMode).GetField ("chatLastMessageSentAtTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleMode, Time.time);
			typeof(BattleMode).GetField ("fadeChat", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleMode, true);
		}

		public void SendChatMessage(string message) {
			battleMode.GetType ().GetMethod ("sendBattleRequest", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (battleMode, new object[] {new GameChatMessageMessage (message) });
		}

		public bool TurnEnded() {
			TileColor leftColor = (battleMode.isLeftColor(TileColor.black)) ? TileColor.black : TileColor.white;
			TileColor activeColor = (TileColor)typeof(BattleMode).GetField ("activeColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
			return (activeColor != leftColor);
		}

		private void TileOver() {
			Tile t = GetTile ();

			if (t != null) {
				battleMode.tileOver (t.gameObject, tileRow, ConvertColumn());
			}
			// Custom colored tileOverlay (the little arrows inside a tile when you move a unit.
			GameObject tileOverlay = (GameObject)typeof(Tile).GetField ("tileOverlay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (t);
			tileOverlay.renderer.material.color = new Color(.3f, 1f, .3f, .6f);
		}
		private void TileOut() {
			Tile t = GetTile ();
			if (t != null) {
				battleMode.tileOut (t.gameObject, tileRow, ConvertColumn());
			}
		}

		public void DeselectAllTiles() {
			battleMode.HideCardView ();
			deselectAllTilesMethodInfo.Invoke (battleMode, new object[] { });
		}

		public Tile GetTile() {
			// Get the colors for the left and right side.
			TileColor leftColor = (battleMode.isLeftColor(TileColor.black)) ? TileColor.black : TileColor.white;
			TileColor rightColor = (leftColor == TileColor.black) ? TileColor.white : TileColor.black;
			TileColor color = (tileColumn > -1) ? leftColor : rightColor;
			return battleMode.getTile (color, tileRow, ConvertColumn());
		}
		private int ConvertColumn() {
			// Get the colors for the left and right side.
			TileColor leftColor = (battleMode.isLeftColor(TileColor.black)) ? TileColor.black : TileColor.white;
			TileColor rightColor = (leftColor == TileColor.black) ? TileColor.white : TileColor.black;
			TileColor color = (tileColumn > -1) ? leftColor : rightColor;
			// Convert the tile column to the right format (2 1 0 - 0 1 2).
			int convertedColumn = tileColumn;
			if (color == rightColor) {
				convertedColumn = Math.Abs (tileColumn + 1);
			}
			return convertedColumn;
		}
	}
}

