using System;
using System.Reflection;

namespace ControllerSupport {
	public class LobbyMenuWrapper {
		private LobbyMenu lobbyMenu;
		private MethodInfo fadeOutSceneMethodInfo;
		private string[] menuSceneNames = new string[] {"_HomeScreen", "_Lobby", "_DeckBuilderView", "_Store", "_Settings", "_Profile"};
		private string lastScene = "_HomeScreen";

		public LobbyMenuWrapper () {
			lobbyMenu = App.LobbyMenu;
			fadeOutSceneMethodInfo = lobbyMenu.GetType ().GetMethod ("fadeOutScene", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void OpenNextScene() {
			OpenScene (GetNextScene (GetCurrentSceneName()));
		}
		public void OpenPreviousScene() {
			OpenScene (GetPreviousScene (GetCurrentSceneName()));
		}

		private void OpenScene(string sceneName) {
			if (sceneName != null && sceneName != string.Empty) {
				typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (lobbyMenu, sceneName);
				App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
				fadeOutSceneMethodInfo.Invoke (lobbyMenu, new object[] { });
				lastScene = sceneName;
			} else {
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

