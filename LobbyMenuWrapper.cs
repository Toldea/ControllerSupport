using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class LobbyMenuWrapper : IOkCancelCallback, IOkStringCancelCallback {
		private LobbyMenu lobbyMenu;
		private MethodInfo fadeOutSceneMethodInfo;
		private string[] menuSceneNames = new string[] {"_HomeScreen", "_Lobby", "_DeckBuilderView", "_Store", "_Settings", "_Profile"};
		private string lastScene = "_HomeScreen";
		private List<GUISkin> GUISkins;
		private Vector3 oldMousePosition;
		private bool shouldDrawSeletionIndicator = false;
		private int selectedIndex = -1;
		private enum Movement {Up, Down, Left, Right};

		public LobbyMenuWrapper () {
			lobbyMenu = App.LobbyMenu;
			oldMousePosition = Input.mousePosition;
			fadeOutSceneMethodInfo = lobbyMenu.GetType ().GetMethod ("fadeOutScene", BindingFlags.NonPublic | BindingFlags.Instance);
			CreateGUISkins ();
		}

		private void CreateGUISkins() {
			this.GUISkins = new List<GUISkin> ();
			GUISkin gUISkin = ScriptableObject.CreateInstance<GUISkin> ();
			gUISkin.box.normal.background = ResourceManager.LoadTexture ("Arena/menu_sptut_mo");
			this.GUISkins.Add (gUISkin);
			GUISkin gUISkin2 = ScriptableObject.CreateInstance<GUISkin> ();
			gUISkin2.box.normal.background = ResourceManager.LoadTexture ("Arena/menu_spquick_mo");
			this.GUISkins.Add (gUISkin2);
			GUISkin gUISkin3 = ScriptableObject.CreateInstance<GUISkin> ();
			gUISkin3.box.normal.background = ResourceManager.LoadTexture ("Arena/menu_trials_mo");
			this.GUISkins.Add (gUISkin3);
			GUISkin gUISkin4 = ScriptableObject.CreateInstance<GUISkin> ();
			gUISkin4.box.normal.background = ResourceManager.LoadTexture ("Arena/menu_mpquick_mo");
			this.GUISkins.Add (gUISkin4);
			GUISkin gUISkin5 = ScriptableObject.CreateInstance<GUISkin> ();
			gUISkin5.box.normal.background = ResourceManager.LoadTexture ("Arena/menu_mpranked_mo");
			this.GUISkins.Add (gUISkin5);
		}

		public void OnGUI () {
			if (GetCurrentSceneIndex (GetCurrentSceneName()) == 1) {
				// Hide the selection indicator when the mouse moved.
				if (oldMousePosition != Input.mousePosition) {
					oldMousePosition = Input.mousePosition;
					shouldDrawSeletionIndicator = false;
				}

				// Draw the 'hovered' button image on the currently controller selected item.
				if (shouldDrawSeletionIndicator && selectedIndex > -1) {
					GUIStyle style = this.GUISkins[selectedIndex].box;

					MockupCalc mockupCalc = new MockupCalc (2048, 1536);
					float x = mockupCalc.X (1000f);
					float x2 = mockupCalc.X (1048f);
					float y2 = mockupCalc.Y (348f);
					float y3 = mockupCalc.Y (548f);
					float y4 = mockupCalc.Y (748f);

					switch (selectedIndex) {
					case 0:
						GUI.Box(getRectFor(0, x, y2, TextAlignment.Right),"",style);
						break;
					case 1:
						GUI.Box(getRectFor(1, x, y3, TextAlignment.Right),"",style);
						break;
					case 2:
						GUI.Box(getRectFor(2, x, y4, TextAlignment.Right),"",style);
						break;
					case 3:
						GUI.Box(getRectFor(3, x2, y2, TextAlignment.Left),"",style);
						break;
					case 4:
						GUI.Box(getRectFor(4, x2, y3, TextAlignment.Left),"",style);
						break;
					default:
						break;
					}
				}
			}
		}

		private Rect getRectFor (int index, float x, float y, TextAlignment align) {
			Texture2D background = this.GUISkins[0].box.normal.background;
			MockupCalc mockupCalc = new MockupCalc (2048, 1536);
			float width = (float)background.width;
			float height = (float)background.height;
			Rect result = mockupCalc.prAspectH (new Vector2 (x, y), width, height);
			if (align == TextAlignment.Center) {
				result.x -= result.width / 2f;
			}
			if (align == TextAlignment.Right) {
				result.x -= result.width;
			}
			return result;
		}

		public void PopupCancel(String type) {}
		public void PopupOk(String type) {}
		public void PopupOk(String type, String choice) {}

		public void HandleInput(string inputType) {
			int currentSceneIndex = GetCurrentSceneIndex (GetCurrentSceneName());

			// Arena/'Lobby' Scene specific Input Handling.
			if (currentSceneIndex == 1) {
				switch (inputType) {
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

			// Generic Input Handling.
			switch (inputType) {
			case "NextScene":
				OpenScene (GetNextScene (GetCurrentSceneName()));
				break;
			case "PreviousScene":
				OpenScene (GetPreviousScene (GetCurrentSceneName()));
				break;
			case "Accept":
				if (!App.Popups.IsShowingPopup () && currentSceneIndex == 1 && selectedIndex > -1) {
					ActivateSelectedAction ();
				}
				break;
			case "Start":
				// Close any active popups if present, else show a popup for quitting the game.
				if (!App.Popups.IsShowingPopup ()) {
					App.Popups.ShowOkCancel (lobbyMenu, "exit", "Quitting Scrolls", "Are you sure you want to quit Scrolls?", "Quit", "Cancel");
				} else {
				}
				break;
			case "ShowInvite":
				// This currently assumes there is only 1 invite, not totally sure when you would receive multiples.
				List<Invite> invites = (List<Invite>)typeof(InviteManager).GetField ("_invites", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (App.InviteManager);
				if (invites.Count > 0) {
					int i = 0;
					App.Popups.KillCurrentPopup ();
					for (int j = 0; j < invites.Count; j++) {
						invites[j].inviteActive = false;
					}
					invites[i].inviteActive = true;
					if (invites[i].message is GameMatchMessage) {
						string text = "ranked";
						if (invites[i].gameType == "MP_QUICKMATCH") {
							text = "quick";
						}
						App.Popups.ShowOkCancel (App.InviteManager, "joingame", "Join game", "A " + text + " match has been found", "Join", "Decline");
					} else {
						if (invites[i].message is GameChallengeMessage) {
							typeof(InviteManager).GetField ("challengeUserId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (App.InviteManager, invites[i].inviterInfo.from.id);
							App.Popups.ShowOkCancel (App.InviteManager, "challenge", "Challenge", invites[i].inviterInfo.from.name + " asked you to play a match", "Accept", "Decline");
						} else {
							if (invites[i].message is TradeInviteForwardMessage) {
								typeof(InviteManager).GetField ("tradeUserId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (App.InviteManager, invites[i].inviterInfo.from.id);
								App.Popups.ShowOkCancel (App.InviteManager, "trade", "Trade", invites[i].inviterInfo.from.name + " asked you to trade", "Accept", "Decline");
							}
						}
					}
				}
				break;
			}
		}

		public void RegisterCurrentScene() {
			lastScene = (string)typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (lobbyMenu);
		}

		private void MoveSelectionIndicator(Movement movement) {
			// Prevent moving the selection indicator when some popup is open.
			if (App.Popups.IsShowingPopup ()) {
				return;
			}
			shouldDrawSeletionIndicator = true;
			// If selectedIndex was invalid, set it to 0.
			if (selectedIndex < 0 || selectedIndex > 5) {
				selectedIndex = 0;
				return;
			} else {
				if (movement == Movement.Up) {
					if (selectedIndex != 0 && selectedIndex != 3) {
						selectedIndex--;
					}
				} else if (movement == Movement.Down) {
					if (selectedIndex != 2 && selectedIndex != 4) {
						selectedIndex++;
					}
				} else if (movement == Movement.Left) {
					if (selectedIndex > 2) {
						selectedIndex -= 3;
					}
				} else if (movement == Movement.Right) {
					if (selectedIndex < 2) {
						selectedIndex += 3;
					}
				}
			}
		}

		private void ActivateSelectedAction () {
			switch (selectedIndex) {
			case 0:
				//App.GameActionManager.StartGame (GameActionManager.StartType.START_TUTORIAL);
				App.Popups.ShowOk (this, "notutorial", "No controller support for the Tutorial", "Controller support for the tutorial is still being worked on!", "A: Ok");
				break;
			case 1:
				App.GameActionManager.StartGame (GameActionManager.StartType.START_SINGLEPLAYER_QUICK);
				break;
			case 2:
				App.Communicator.sendRequest (new GetTowerInfoMessage ());
				App.GameActionManager.StartGame (GameActionManager.StartType.START_TOWER_CHALLENGE);
				break;
			case 3:
				App.GameActionManager.StartGame (GameActionManager.StartType.START_MULTIPLAYER_QUICK);
				App.LobbyMenu.runQueueAnim ();
				break;
			case 4:
				App.GameActionManager.StartGame (GameActionManager.StartType.START_MULTIPLAYER_RANKED);
				App.LobbyMenu.runQueueAnim ();
				break;
			default:
				return;
			}
		}

		private void OpenScene(string sceneName) {
			if (sceneName != null && sceneName != string.Empty) {
				typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (lobbyMenu, sceneName);
				App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
				fadeOutSceneMethodInfo.Invoke (lobbyMenu, new object[] { });
				lastScene = sceneName;
			}
		}

		public void SetCurrentScene(string sceneName) {
			lastScene = sceneName;
		}

		private string GetNextScene(string currentSceneName) {
			int index = GetCurrentSceneIndex (currentSceneName);
			if (index < 0) {
				return "_Lobby";
			}
			index++;
			if (index >= menuSceneNames.Length) {
				index = 0;
			}
			return menuSceneNames [index];
		}
		private string GetPreviousScene(string currentSceneName) {
			int index = GetCurrentSceneIndex (currentSceneName);
			if (index >= menuSceneNames.Length) {
				return "_Lobby";
			}
			index--;
			if (index < 0) {
				index = menuSceneNames.Length-1;
			}
			return menuSceneNames [index];
		}

		public string GetCurrentSceneName() {
			if (lastScene != null && lastScene != string.Empty) {
				return lastScene;
			} else {
				return "_Lobby";
			}

		}
		private int GetCurrentSceneIndex(string sceneName) {
			if (sceneName == null || sceneName == string.Empty) {
				return -1;
			} else {
				int index = Array.FindIndex (menuSceneNames, row => row.Contains (sceneName)); 
				return index;
			}
		}
	}
}

