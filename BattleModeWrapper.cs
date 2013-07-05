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
		private bool controlBoard = false;
		private int tileRow = 2;
		private int tileColumn = 1;

		public BattleModeWrapper (BattleMode battleMode) {
			Console.WriteLine ("ControllerSupport: Creating BattleMode Wrapper.");
			this.battleMode = battleMode;
			this.handManager = GetHandManager ();
			cardClickedMethodInfo = battleMode.GetType().GetMethod("cardClicked", BindingFlags.NonPublic | BindingFlags.Instance);
			toggleUnitStatsMethodInfo = battleMode.GetType().GetMethod("toggleUnitStats", BindingFlags.NonPublic | BindingFlags.Instance);
			deselectAllTilesMethodInfo = battleMode.GetType ().GetMethod ("deselectAllTiles", BindingFlags.NonPublic | BindingFlags.Instance);
			gameMenu = (GUIBattleModeMenu)typeof(BattleMode).GetField ("menu", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
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
				CardType.Kind type = handManager.GetSelectedCard ().getCardInfo ().getPieceKind ();
				if (type == CardType.Kind.CREATURE || type == CardType.Kind.STRUCTURE) {
					allowRightBoardMovement = false;
				}
			}

			TileOut (); // Fade out the last tile.
			// Add the displacement.
			tileColumn += x;
			tileRow += y;
			// Make sure the row/column stays within the board.
			if (allowRightBoardMovement) {
				if (tileColumn < -3) tileColumn = -3;
			} else {
				if (tileColumn < 0) tileColumn = 0;
			}
			if (tileColumn > 2) tileColumn = 2;
			if (tileRow < 0) tileRow = 0;
			if (tileRow > 4) tileRow = 4;

			Console.WriteLine ("ControllerSupport: Moved to tile: [" + tileColumn + ", " + tileRow + "]");

			TileOver (); // Fade in the new tile.
		}

		public void TileClicked () {
			Tile t = GetTile ();
			if (t != null) {
				List<List<Tile>> tileSelectionList = new List<List<Tile>> ((List<List<Tile>>)typeof(BattleMode).GetField ("tileSelectionList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode));
				// Check if we are placing a unit or activating an ability.
				bool didPlaceUnit = false;
				bool didActivateAbility = false;
				if (handManager.GetSelectedCard () != null) {
					if (battleMode.isTileInList (t, tileSelectionList)) {
						Console.WriteLine ("ControllerSupport: TileClicked() tileSelectionList: " + tileSelectionList);
						tileSelectionList.RemoveAt (0);
						if (tileSelectionList.Count == 0) {
							Console.WriteLine ("ControllerSupport: TileClicked() tileSelectionList.Count == 0!");
							didPlaceUnit = true;
						}
					}
				} else {
					string activeAbilityId = (string)typeof(BattleMode).GetField ("activeAbilityId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
					Console.WriteLine ("ControllerSupport: TileClicked() This is called when no card is selected :D");
					if (activeAbilityId != string.Empty && activeAbilityId != null) {
						//tileSelectionList.RemoveAt (0);
						//if (tileSelectionList.Count == 0) {
							Console.WriteLine ("ControllerSupport: TileClicked() AAAAAAAAAAAAAAAAAAAAAAA"); // <- move

							// Check if the currently selected unit has an activatable ability and if so activate it.
							GameObject cardRule = (GameObject)typeof(BattleMode).GetField ("cardRule", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
							if (cardRule != null) {
								CardView cardView = cardRule.GetComponent<CardView> ();
								if (cardView != null) {
									ActiveAbility[] activeAbilities = cardView.getCardInfo().getActiveAbilities ();
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
							//





						//} else {
							Console.WriteLine ("ControllerSupport: TileClicked() BBBBBBBBBBBBBBBBBBBBBBB");
						//}
					} else {
						Console.WriteLine ("ControllerSupport: TileClicked() CCCCCCCCCCCCCCCCCCCCCCCCC"); // <- empty space, enemy unit, allied unit
					}
					// this should do the following:
					// check if we are selecting a unit
					// check if we previously selected a unit
					// check if we selected an empty tile
					// if we have no previously selected unit AND we selected an empty tile: battleMode.HideCardView ();
				}
				// 'Click' on the currently highlighted tile if we didn't activate a unit's activated ability.
				if (!didActivateAbility) {
					battleMode.tileClicked(t);
				}
				// Return control back to the hand if we placed a unit.
				if (didPlaceUnit) {
					TakeControlOfHand ();
				}
			}
		}

		public bool UnitSelectedOnBoard() {
			if (handManager.GetSelectedCard () == null) {
				List<List<Tile>> tileSelectionList = new List<List<Tile>> ((List<List<Tile>>)typeof(BattleMode).GetField ("tileSelectionList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode));
				string activeAbilityId = (string)typeof(BattleMode).GetField ("activeAbilityId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
				if (activeAbilityId != string.Empty && activeAbilityId != null) {
					tileSelectionList.RemoveAt (0);
					if (tileSelectionList.Count == 0) {
						Console.WriteLine ("ControllerSupport: UnitSelectedOnBoard() ******* THERE WAS A UNIT SELECTED ******"); // <- move
						return true;
					}
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
				battleMode.tileOver (t.gameObject, tileRow, tileColumn);
			}
		}
		private void TileOut() {
			Tile t = GetTile ();
			if (t != null) {
				battleMode.tileOut (t.gameObject, tileRow, tileColumn);
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
			// Convert the tile column to the right format (2 1 0 - 0 1 2).
			int convertedColumn = tileColumn;
			if (color == rightColor) {
				convertedColumn = Math.Abs (tileColumn + 1);
			}
			return battleMode.getTile (color, tileRow, convertedColumn);
		}
	}
}

