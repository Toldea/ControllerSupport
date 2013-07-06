using System;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class LobbyMenuWrapper : IOkCancelCallback, IOkStringCancelCallback {
		private LobbyMenu lobbyMenu;
		private MethodInfo fadeOutSceneMethodInfo;
		private string[] menuSceneNames = new string[] {"_HomeScreen", "_Lobby", "_DeckBuilderView", "_Store", "_Settings", "_Profile"};
		private string lastScene = "_HomeScreen";

		public LobbyMenuWrapper () {
			lobbyMenu = App.LobbyMenu;
			fadeOutSceneMethodInfo = lobbyMenu.GetType ().GetMethod ("fadeOutScene", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void PopupCancel(String type) {
		}

		public void PopupOk(String type) {
			if (type == "exit") {
				// Check if we are queued, if so leave the queue.
				if (App.Communicator.IsQueued) {
					App.Communicator.sendRequest (new ExitMultiPlayerQueueMessage ());
				}
				// Exit the application.
				Application.Quit ();
			}
		}

		public void PopupOk(String type, String choice) {
		}

		public void HandleInput(string inputType) {
			//int currentSceneIndex = GetCurrentSceneIndex (GetCurrentSceneName());
			switch (inputType) {
			case "NextScene":
				OpenScene (GetNextScene (GetCurrentSceneName()));
				break;
			case "PreviousScene":
				OpenScene (GetPreviousScene (GetCurrentSceneName()));
				break;
			case "Accept":
				if (App.Popups.IsShowingPopup ()) {
					string popupType = (string)typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (App.Popups);
					App.Popups.RequestPopupClose ();
					PopupOk (popupType);
				}
				break;
			case "Cancel":
				// Close any active popups if present, else show a popup for quitting the game.
				if (App.Popups.IsShowingPopup ()) {
					App.Popups.RequestPopupClose ();
				} else {
					App.Popups.ShowOkCancel (this, "exit", "Quitting Scrolls", "Are you sure you want to quit Scrolls?", "Quit", "Cancel");
				}
				break;
			case "Right":
				break;
			case "Left":
				break;
			case "Up":
				break;
			case "Down":
				break;
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

		private string GetCurrentSceneName() {
			string currentScene = (string)typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (lobbyMenu);
			if (currentScene == null || currentScene == string.Empty) {
				if (lastScene != null && lastScene != string.Empty) {
					return lastScene;
				} else {
					return "_Lobby";
				}
			} else {
				return currentScene;
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

