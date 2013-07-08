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
			buttonHighlightedUISkin.box.fontStyle = regularUISkin.button.fontStyle;
			buttonHighlightedUISkin.box.fontSize = regularUISkin.button.fontSize;
			buttonHighlightedUISkin.box.normal.textColor = regularUISkin.button.hover.textColor;
			buttonHighlightedUISkin.box.normal.background = regularUISkin.button.hover.background;
			buttonHighlightedUISkin.box.alignment = regularUISkin.button.alignment;
		}

		public void OnGUI() {
			herpderp ();

			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (this.popups);

			if (currentPopupType != PopupType.NONE) {
				if (selectedIndex == -1) {
					shouldDrawSeletionIndicator = true;
					selectedIndex = 0;
				}
			} else {
				selectedIndex = -1;
			}
		}

		private void herpderp() {
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
								Console.WriteLine ("ControllerSupport: PopupsWrapper: herpderp: ButtonList.Length: " + buttonList.Length + ", selectedIndex: " + selectedIndex);
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

		public void HandleInput(string inputType) {
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);

			switch (inputType) {
			case "Accept":
				if (currentPopupType == PopupType.OK_CANCEL) {
					if (selectedIndex == 0) {
						AcceptPopup ();
					} else if (selectedIndex == 1) {
						CancelPopup ();
					}
				} else if (currentPopupType == PopupType.MULTIBUTTON) {
					AcceptMultibuttonPopup ();
				}
				selectedIndex = -1;
				break;
			case "Cancel":
				//if (currentPopupType == PopupType.OK_CANCEL || currentPopupType == PopupType.SAVE_DECK || currentPopupType == PopupType.PURCHASE_PASSWORD_ENTRY) {
					CancelPopup ();
				//}
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
			Console.WriteLine ("ControllerSupport: PopupsWrapper: AcceptPopup called!");
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
			Console.WriteLine ("ControllerSupport: PopupsWrapper: AcceptMultibuttonPopup called!");
			App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
			currentPopupType = (PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			HidePopupMethodInfo.Invoke (popups, new object[] { });
			string popupType = (string)typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups);
			((IOkStringCallback)typeof(Popups).GetField ("okStringCallback", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (popups)).PopupOk(popupType, buttonList[selectedIndex]);
		}
		private void CancelPopup() {
			Console.WriteLine ("ControllerSupport: PopupsWrapper: CancelPopup called!");
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

