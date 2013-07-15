using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class PopupsWrapper {
		private Popups popups;
		private MethodInfo HidePopupMethodInfo;
		private PopupType currentPopupType;
		private int selectedIndex = -1;
		private GUISkin buttonHighlightedUISkin;
		private Vector3 oldMousePosition;
		private bool shouldDrawSeletionIndicator = false;

		public PopupsWrapper (Popups popups) {
			this.popups = popups;
			HidePopupMethodInfo = popups.GetType().GetMethod("HidePopup", BindingFlags.NonPublic | BindingFlags.Instance);
			CreateGUISkins ();
		}

		private void CreateGUISkins() {
			// Clone the regular button ui skin and make the .box.normal we will draw look like the .button.hover.
			GUISkin regularUISkin = (GUISkin)Resources.Load ("_GUISkins/RegularUI");
			buttonHighlightedUISkin = (GUISkin)GameObject.Instantiate (regularUISkin);
			buttonHighlightedUISkin.box.font = regularUISkin.button.font;
			Console.WriteLine ("ControllerSupport: DefaultFont: " + buttonHighlightedUISkin.box.font.name);
			buttonHighlightedUISkin.box.fontStyle = regularUISkin.button.fontStyle;
			buttonHighlightedUISkin.box.fontSize = regularUISkin.button.fontSize;
			buttonHighlightedUISkin.box.normal.textColor = regularUISkin.button.hover.textColor;
			buttonHighlightedUISkin.box.normal.background = regularUISkin.button.hover.background;
			buttonHighlightedUISkin.box.alignment = regularUISkin.button.alignment;
		}

		public void OnGUI() {
			DrawPopupHoverIndicator ();

			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (this.popups);

			if (currentPopupType != PopupType.NONE) {
				if (selectedIndex == -1) {
					selectedIndex = 0;
					if (!Input.GetMouseButtonUp(0)) {
						shouldDrawSeletionIndicator = true;
					}
				}
			} else {
				selectedIndex = -1;
			}
		}

		private void DrawPopupHoverIndicator() {
			// Hide the selection indicator when the mouse moved.
			if (oldMousePosition != Input.mousePosition) {
				oldMousePosition = Input.mousePosition;
				shouldDrawSeletionIndicator = false;
			}

			if (shouldDrawSeletionIndicator && selectedIndex > -1) {
				currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
				string okText = (string)typeof(Popups).GetField ("okText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
				string cancelText = (string)typeof(Popups).GetField ("cancelText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);

				if (this.currentPopupType != PopupType.NONE) {
					GUI.depth = 4;
					GUI.skin = buttonHighlightedUISkin;
					int fontSize = GUI.skin.button.fontSize;
					GUI.skin.box.fontSize = 10 + Screen.height / 72;
					float num = (float)Screen.height * 0.03f;
					Rect rect;
					if (currentPopupType == PopupType.DECK_SELECTOR) {
						rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.45f, (float)Screen.height * 0.255f, (float)Screen.height * 0.9f, (float)Screen.height * 0.49f);
					} else {
						if (currentPopupType == PopupType.TOWER_CHALLENGE_SELECTOR) {
							rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.5f, (float)Screen.height * 0.1f, (float)Screen.height * 1f, (float)Screen.height * 0.8f);
						} else {
							if (currentPopupType == PopupType.SCROLL_TEXT) {
								rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.5f, (float)Screen.height * 0.1f, (float)Screen.height * 1f, (float)Screen.height * 0.8f);
							} else {
								rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.35f, (float)Screen.height * 0.3f, (float)Screen.height * 0.7f, (float)Screen.height * 0.4f);
							}
						}
					}
					Rect rect2 = new Rect (rect.x + num, rect.y + num, rect.width - 2f * num, rect.height - 2f * num);
					//float num2 = (float)Screen.height * 0.03f;
					//Rect r = new Rect (rect2.xMax - num2, rect2.y, num2, num2);
					//float num4 = (float)Screen.height * 0.055f;
					//float height2 = rect2.height * ((this.currentPopupType != PopupType.SAVE_DECK && currentPopupType != PopupType.PURCHASE_PASSWORD_ENTRY) ? ((currentPopupType != PopupType.INFO_PROGCLOSE) ? 0.6f : 0.8f) : 0.35f);
					int fontSize3 = GUI.skin.label.fontSize;
					GUI.skin.label.fontSize = fontSize3;
					if (currentPopupType == PopupType.OK || currentPopupType == PopupType.SCROLL_TEXT) {
						Rect position2 = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.1f, rect2.yMax - (float)Screen.height * 0.05f, (float)Screen.height * 0.2f, (float)Screen.height * 0.05f);
						GUI.Box (position2, okText);
					} else {
						if (currentPopupType == PopupType.OK_CANCEL || currentPopupType == PopupType.SAVE_DECK) {// || currentPopupType == PopupType.PURCHASE_PASSWORD_ENTRY) {
							Rect r2 = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.21f, rect2.yMax - (float)Screen.height * 0.05f, (float)Screen.height * 0.2f, (float)Screen.height * 0.05f);
							Rect r3 = new Rect ((float)Screen.width * 0.5f + (float)Screen.height * 0.01f, rect2.yMax - (float)Screen.height * 0.05f, (float)Screen.height * 0.2f, (float)Screen.height * 0.05f);
							if (selectedIndex == 0) {
								GUI.Box (r2, okText);
							} else if (selectedIndex == 1) {
								GUI.Box (r3, cancelText);
							}
						} else {
							if (currentPopupType == PopupType.MULTIBUTTON) {
								string[] buttonList = (string[])typeof(Popups).GetField ("buttonList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
								float num5 = (float)Screen.height * 0.05f;
								float num6 = num5 + (float)Screen.height * 0.02f;
								Rect r4 = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.1f, rect2.y + rect2.height * 0.55f - ((float)buttonList.Length / 2f - (float)selectedIndex) * num6 + (num6 - num5), (float)Screen.height * 0.2f, num5);
								GUI.Box (r4, buttonList [selectedIndex]);
							}
						}
					}
					if (currentPopupType != PopupType.INFO_PROGCLOSE && currentPopupType != PopupType.OK && currentPopupType != PopupType.SCROLL_TEXT) {
						/*
						GUI.skin = this.closeButtonSkin;
						if (this.GUIButton (r, string.Empty)) {
							this.HidePopup ();
							this.cancelCallback.PopupCancel (this.popupType);
						}
						GUI.skin = this.regularUISkin;
						*/
					}
					if (currentPopupType == PopupType.DECK_SELECTOR) {
						DrawDeckSelectorHoverIndicator (rect2);
					} else if (currentPopupType == PopupType.TOWER_CHALLENGE_SELECTOR) {
						DrawTowerChallengeSelectorHoverIndicator(rect2);
					}
					/*
					if (currentPopupType == PopupType.JOIN_ROOM) {
					} else {
						if (currentPopupType == PopupType.SAVE_DECK || currentPopupType == PopupType.PURCHASE_PASSWORD_ENTRY) {
						} else {
							if (currentPopupType == PopupType.DECK_SELECTOR) {
								this.DrawDeckSelector (rect2);
							} else {
								if (currentPopupType == PopupType.SHARD_PURCHASE_ONE) {
								} else {
									if (currentPopupType == PopupType.SHARD_PURCHASE_TWO) {
									} else {
										if (currentPopupType == PopupType.TOWER_CHALLENGE_SELECTOR) {
											this.DrawTowerChallengeSelector (rect2);
										}
									}
								}
							}
						}
					}*/
					GUI.skin.button.fontSize = fontSize;
				}
			}
		}

		private void DrawDeckSelectorHoverIndicator(Rect popupInner) {
			List<DeckInfo> deckList = (List<DeckInfo>)typeof(Popups).GetField ("deckList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			bool showDeleteDeckIcon = (bool)typeof(Popups).GetField ("showDeleteDeckIcon", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			if (showDeleteDeckIcon || selectedIndex < 0 || selectedIndex >= deckList.Count) {
				return;
			}
			deckList.Sort (delegate (DeckInfo a, DeckInfo b) {
				if (a.valid && !b.valid) {
					return -1;
				}
				if (b.valid && !a.valid) {
					return 1;
				}
				if (a.timestamp > b.timestamp) {
					return -1;
				}
				if (b.timestamp > a.timestamp) {
					return 1;
				}
				return a.name.CompareTo (b.name);
			});
			Rect position = new Rect (popupInner.x, popupInner.y + popupInner.height * 0.15f, popupInner.width, popupInner.height * 0.85f);
			float num = (float)Screen.height * 0.015f;
			float num2 = (float)Screen.height * 0.07f;
			float num3 = num2 + num;
			Rect position2 = new Rect (position.x + 2f + num, position.y + 2f + num, position.width - 4f - 2f * num, position.height - 4f - 2f * num);
			float num4 = position2.width - 20f;
			int num5 = (deckList.Count % 2 != 0) ? (deckList.Count / 2 + 1) : (deckList.Count / 2);
			int fontSize = GUI.skin.label.fontSize;
			TextAnchor alignment = GUI.skin.label.alignment;
			bool wordWrap = GUI.skin.label.wordWrap;
			GUI.skin.label.wordWrap = false;
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
			Vector2 deckScroll = (Vector2)typeof(Popups).GetField ("deckScroll", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			deckScroll = GUI.BeginScrollView (position2, deckScroll, new Rect (0f, 0f, num4, (float)(num5 - 1) * num3 + num2));
			typeof(Popups).GetField ("deckScroll", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (this.popups, deckScroll);
			int i = selectedIndex / 2;
			int j = selectedIndex % 2;
			if (2 * i + j < deckList.Count) {
				DeckInfo deckInfo = deckList[(2 * i + j)];
				bool flag = deckInfo.valid;// || this.allowInvalidClicks;
				GUI.enabled = flag;
				Rect r = new Rect ((float)j * num4 / 2f, (float)i * num3, num4 / 2f - num, num2);
				//Rect rect = new Rect (r.xMax - (float)Screen.height * 0.025f, r.y + (float)Screen.height * 0.005f, (float)Screen.height * 0.02f, (float)Screen.height * 0.02f);
				/*
				if (this.showDeleteDeckIcon) {
					GUI.skin = this.emptySkin;
					if (this.GUIButton (rect, string.Empty)) {
						this.deckChosenCallback.PopupDeckDeleted (deckInfo);
					}
					GUI.skin = this.regularUISkin;
				}*/

				GUI.Box (r, string.Empty);
				string text = deckInfo.name;
				if (!deckInfo.valid) {
					text = "<color=#ee5533>[Illegal]</color> " + text;
				}
				GUI.skin.label.fontSize = 10 + Screen.height / 60;
				GUI.Label (new Rect (r.x + r.width * 0.25f, r.y + r.height * 0.05f, r.width * 0.66f, r.height * 0.6f), text);
				GUI.skin.label.fontSize = 8 + Screen.height / 80;
				GUI.Label (new Rect (r.x + r.width * 0.25f, r.y + r.height * 0.45f, r.width * 0.66f, r.height * 0.6f), deckInfo.updated);
				if (!flag) {
					GUI.color = new Color (1f, 1f, 1f, 0.5f);
				}
				GUI.DrawTexture (new Rect (r.x + r.height * 0.4f, r.y + r.height * 0.1f, r.height * 0.7f, r.height * 0.8f), ResourceManager.LoadTexture ("Arena/deck_icon"));
				if (deckInfo.resources != string.Empty) {
					string[] array = deckInfo.resources.Split (new char[] {
						','
					});
					for (int k = 0; k < array.Length; k++) {
						float num6 = r.height * 0.38f;
						float left = r.x + ((k < 2) ? 0f : (num6 * 0.85f * 73f / 72f)) + 3f;
						float num7 = r.y + ((k < 2) ? 0f : (num6 / 3f)) + 3f;
						GUI.DrawTexture (new Rect (left, num7 + num6 * 0.85f * (float)(k % 2), num6 * 73f / 72f, num6), ResourceManager.LoadTexture ("BattleUI/battlegui_icon_" + array [k]));
					}
				}
				/*
				GUI.color = Color.white;
				if (this.showDeleteDeckIcon) {
					GUI.skin = this.closeButtonSkin;
					if (GUI.Button (rect, string.Empty)) {
					}
					GUI.skin = this.regularUISkin;
				}*/
				GUI.enabled = true;
			}
			GUI.skin.label.fontSize = fontSize;
			GUI.skin.label.alignment = alignment;
			GUI.skin.label.wordWrap = wordWrap;
			GUI.EndScrollView ();
		}

		private void DrawTowerChallengeSelectorHoverIndicator (Rect popupInner) {
			TextAnchor alignment = GUI.skin.label.alignment;
			Rect rect = new Rect (popupInner.x, popupInner.y + popupInner.height * 0.15f, popupInner.width, popupInner.height * 0.85f);
			int fontSize = GUI.skin.label.fontSize;
			TextAnchor alignment2 = GUI.skin.label.alignment;
			bool wordWrap = GUI.skin.label.wordWrap;
			GUI.skin.label.wordWrap = false;
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			TowerLevels[] levels = App.TowerChallengeInfo.levels;
			if (levels == null) {
				GUI.skin.label.wordWrap = wordWrap;
				GUI.skin.label.alignment = alignment2;
				return;
			}
			Rect position = new Rect (rect.x + rect.width * 0.4f + 5f + 25f, rect.y, rect.width * 0.6f - 40f, rect.height);
			GUI.BeginGroup (position);
			GUI.Box (new Rect ((rect.width * 0.6f - 40f) / 2f - rect.width * 0.15f, rect.height - 65f, rect.width * 0.3f, 50f), "Play Trial");
			GUI.EndGroup ();

			GUI.skin.label.fontSize = fontSize;
			GUI.skin.label.normal.textColor = Color.white;
			GUI.skin.label.alignment = alignment;
			GUI.skin.label.wordWrap = wordWrap;
		}

		public void HandleInput(string inputType) {
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);

			switch (inputType) {
			case "Accept":
				if (currentPopupType == PopupType.OK) {
					CancelPopup ();
				} else if (currentPopupType == PopupType.OK_CANCEL) {
					if (selectedIndex == 0) {
						AcceptPopup ();
					} else if (selectedIndex == 1) {
						CancelPopup ();
					}
				} else if (currentPopupType == PopupType.MULTIBUTTON) {
					AcceptMultibuttonPopup ();
				} else if (currentPopupType == PopupType.DECK_SELECTOR) {
					AcceptDeckSelectorPopup ();
				} else if (currentPopupType == PopupType.TOWER_CHALLENGE_SELECTOR) {
					AcceptTowerChallengePopup ();
				}
				selectedIndex = -1;
				break;
			case "Cancel":
				CancelPopup ();
				selectedIndex = -1;
				break;
			case "Right":
				MoveSelectionIndicator (Movement.Right);
				break;
			case "Left":
				MoveSelectionIndicator (Movement.Left);
				break;
			case "Up":
				MoveSelectionIndicator (Movement.Up);
				break;
			case "Down":
				MoveSelectionIndicator (Movement.Down);
				break;
			}
		}

		public bool IsShowingPopup() {
			return popups.IsShowingPopup ();
		}

		private void AcceptPopup() {
			App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			HidePopupMethodInfo.Invoke (popups, new object[] { });
			string popupType = (string)typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			((IOkCallback)typeof(Popups).GetField ("okCallback", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups)).PopupOk(popupType);
		}
		private void AcceptMultibuttonPopup() {
			if (selectedIndex < 0) { 
				return;
			}
			string[] buttonList = (string[])typeof(Popups).GetField ("buttonList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			if (selectedIndex >= buttonList.Length) {
				return;
			}
			App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			HidePopupMethodInfo.Invoke (popups, new object[] { });
			string popupType = (string)typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			((IOkStringCallback)typeof(Popups).GetField ("okStringCallback", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups)).PopupOk(popupType, buttonList[selectedIndex]);
		}
		private void AcceptDeckSelectorPopup () {
			HidePopupMethodInfo.Invoke (popups, new object[] { });
			List<DeckInfo> deckList = (List<DeckInfo>)typeof(Popups).GetField ("deckList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			if (selectedIndex < 0 || selectedIndex >= deckList.Count) {
				return;
			}
			deckList.Sort (delegate (DeckInfo a, DeckInfo b) {
				if (a.valid && !b.valid) {
					return -1;
				}
				if (b.valid && !a.valid) {
					return 1;
				}
				if (a.timestamp > b.timestamp) {
					return -1;
				}
				if (b.timestamp > a.timestamp) {
					return 1;
				}
				return a.name.CompareTo (b.name);
			});
			DeckInfo deckInfo = deckList[selectedIndex];
			((IDeckCallback)typeof(Popups).GetField ("deckChosenCallback", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups)).PopupDeckChosen(deckInfo);
		}
		private void AcceptTowerChallengePopup() {
			HidePopupMethodInfo.Invoke (popups, new object[] { });
			App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
			TowerLevels[] levels = App.TowerChallengeInfo.levels;
			App.GameActionManager.PopupTowerChallengeChosen(levels [selectedIndex].id);
		}
		private void CancelPopup() {
			App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			HidePopupMethodInfo.Invoke (popups, new object[] { });
			string popupType = (string)typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			((ICancelCallback)typeof(Popups).GetField ("cancelCallback", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups)).PopupCancel(popupType);
		}

		private void MoveSelectionIndicator(Movement movement) {
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			shouldDrawSeletionIndicator = true;

			if (currentPopupType == PopupType.OK_CANCEL) {
				if (movement == Movement.Left) {
					selectedIndex = 0;
				} else if (movement == Movement.Right) {
					selectedIndex = 1;
				}
			} else if (currentPopupType == PopupType.MULTIBUTTON) {
				int numButtons = ((string[])typeof(Popups).GetField ("buttonList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups)).Length;
				if (movement == Movement.Up) {
					selectedIndex--;
					if (selectedIndex < 0) {
						selectedIndex = 0;
					} else if (selectedIndex >= numButtons) {
						selectedIndex = numButtons - 1;
					}
				} else if (movement == Movement.Down) {
					selectedIndex++;
					if (selectedIndex >= numButtons) {
						selectedIndex = numButtons - 1;
					}
				}
			} else if (currentPopupType == PopupType.DECK_SELECTOR) {
				List<DeckInfo> deckList = (List<DeckInfo>)typeof(Popups).GetField ("deckList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
				int listCount = deckList.Count;

				if (movement == Movement.Up) {
					if (selectedIndex - 2 >= 0) {
						selectedIndex -= 2;
					}
				} else if (movement == Movement.Down) {
					if (selectedIndex + 2 < listCount) {
						selectedIndex += 2;
					}
				} else if (movement == Movement.Left) {
					// We are on the right side if selected index currently is uneven.
					if (selectedIndex % 2 == 1 && selectedIndex > 0) {
						selectedIndex--;
					}
				} else if (movement == Movement.Right) {
					// We are on the left side if selected index currently is even.
					if (selectedIndex % 2 == 0 && selectedIndex < listCount - 1) {
						selectedIndex++;
					}
				}

				//Calculate the scroll view height.
				float scollViewHeight = Screen.height * 0.49f;
				float num = (float)Screen.height * 0.03f;
				scollViewHeight = scollViewHeight - 2f * num;
				scollViewHeight *= .85f;
				num = (float)Screen.height * 0.015f;
				scollViewHeight = scollViewHeight - 4f - 2f * num;
				// Calculate the total scroll size height.
				float num2 = (float)Screen.height * 0.07f;
				float num3 = num2 + num;
				int halfDeckLength = (deckList.Count % 2 != 0) ? (deckList.Count / 2 + 1) : (deckList.Count / 2);
				int deckListY = selectedIndex / 2;
				float scrollSizeHeight = (float)(halfDeckLength - 1) * num3 + num2;
				// Set the scroll position relative to the selected item.
				Vector2 deckScroll = (Vector2)typeof(Popups).GetField ("deckScroll", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
				deckScroll.y = (halfDeckLength - (halfDeckLength - deckListY)) * (scrollSizeHeight / halfDeckLength);
				deckScroll.y -= (scollViewHeight - num3);
				if (deckScroll.y < 0f) {
					deckScroll.y = 0f;
				}
				typeof(Popups).GetField ("deckScroll", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (this.popups, deckScroll);
			} else if (currentPopupType == PopupType.TOWER_CHALLENGE_SELECTOR) {
				TowerLevels[] levels = App.TowerChallengeInfo.levels;
				if (movement == Movement.Up) {
					if (selectedIndex > 0) {
						selectedIndex--;
					}
				} else if (movement == Movement.Down) {
					if (selectedIndex + 1 < levels.Length) {
						selectedIndex++;
					}
				}
				// Set the selectedIndex as the selectedChallengeID.
				typeof(Popups).GetField ("selectedChallengeID", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, selectedIndex);
				// Calculate the scroll view height.
				float scollViewHeight = Screen.height * 0.8f;
				float num = (float)Screen.height * 0.03f;
				scollViewHeight = scollViewHeight - 2f * num;
				scollViewHeight *= .85f;
				num = (float)Screen.height * 0.015f;
				scollViewHeight = scollViewHeight - 4f - 2f * num;
				// Calculate the total scroll size height.
				float scrollSizeHeight = levels.Length * 55;
				// Set the scroll position relative to the selected item.
				Vector2 deckScroll = (Vector2)typeof(Popups).GetField ("deckScroll", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
				deckScroll.y = (levels.Length - (levels.Length - selectedIndex)) * (scrollSizeHeight / levels.Length);
				deckScroll.y -= (scollViewHeight - 55f);
				if (deckScroll.y < 0f) {
					deckScroll.y = 0f;
				}
				typeof(Popups).GetField ("deckScroll", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (this.popups, deckScroll);
			}
		}

		private enum Movement {Up, Down, Left, Right};
		private enum PopupType {
			NONE,
			OK_CANCEL,
			OK,
			MULTIBUTTON,
			DECK_SELECTOR,
			SAVE_DECK,
			JOIN_ROOM,
			INFO_PROGCLOSE,
			SHARD_PURCHASE_ONE,
			SHARD_PURCHASE_TWO,
			TOWER_CHALLENGE_SELECTOR,
			GOLD_SHARDS_SELECT,
			PURCHASE_PASSWORD_ENTRY,
			SCROLL_TEXT
		}
	}
}

