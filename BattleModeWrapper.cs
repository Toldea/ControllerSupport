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

		public BattleModeWrapper (BattleMode battleMode) {
			Console.WriteLine ("ControllerSupport: Creating BattleMode Wrapper.");
			Initialize (battleMode);
		}

		public void Validate(BattleMode battleMode) {
			if (this.battleMode == null || this.handManager == null) {
				Console.WriteLine ("ControllerSupport: BattleModeWrapper.Validate: BattleMode or HandManager are invalid, reinitializing..");
				Initialize (battleMode);
			}
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

		public void CardClicked(CardView cardView, int mouseButton) {
			cardClickedMethodInfo.Invoke (battleMode, new object[] { cardView, mouseButton });
		}

		public void EndTurnPressed() {
			battleMode.endturnPressed ();
		}

		public void ShowMenu() {
			gameMenu.toggleMenu ();
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

		public void TileClicked () {
			Tile tile = GetTile ();
			if (tile != null) {
				// Get the list of all valid tiles to do the currently active ability on (play scroll, move, etc.).
				List<List<Tile>> tileSelectionList = new List<List<Tile>> ((List<List<Tile>>)typeof(BattleMode).GetField ("tileSelectionList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode));
				bool didPlaceUnit = false;
				bool didActivateAbility = false;
				// Check if we are trying to play a card from hand on the battlefield (i.e. units / targeted spells / enchantments).
				if (handManager.GetSelectedCard () != null) {
					if (battleMode.isTileInList (tile, tileSelectionList)) {
						//Console.WriteLine ("ControllerSupport: TileClicked() tileSelectionList: " + tileSelectionList);
						tileSelectionList.RemoveAt (0);
						if (tileSelectionList.Count == 0) {
							//Console.WriteLine ("ControllerSupport: TileClicked() tileSelectionList.Count == 0!");
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
												//Console.WriteLine ("ControllerSupport: Activating Ability: " + activeAbility.description);
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
				//List<List<Tile>> tileSelectionList = new List<List<Tile>> ((List<List<Tile>>)typeof(BattleMode).GetField ("tileSelectionList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode));
				string activeAbilityId = (string)typeof(BattleMode).GetField ("activeAbilityId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
				if (activeAbilityId != string.Empty && activeAbilityId != null) {
					//tileSelectionList.RemoveAt (0);
					//if (tileSelectionList.Count == 0) {
						Console.WriteLine ("ControllerSupport: UnitSelectedOnBoard() ******* THERE WAS A UNIT SELECTED ******"); // <- move
						return true;
					//}
				}
			}
			Console.WriteLine ("ControllerSupport: UnitSelectedOnBoard() ******* THERE WAS NO!!! UNIT SELECTED ******"); // <- move
			return false;
		}

		public void SendChatMessage(string message) {
			battleMode.GetType ().GetMethod ("sendBattleRequest", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (battleMode, new object[] {new GameChatMessageMessage (message) });
		}

		private void TileOver() {
			Tile t = GetTile ();

			if (t != null) {
				battleMode.tileOver (t.gameObject, tileRow, ConvertColumn());
			}
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

		private Tile GetTile() {
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

