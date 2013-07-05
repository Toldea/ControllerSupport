using System;
using System.Reflection;

namespace ControllerSupport {
	public class EndGameScreenWrapper {
		private EndGameScreen endGameScreen;
		public EndGameScreenWrapper (EndGameScreen endGameScreen) {
			this.endGameScreen = endGameScreen;
		}
		public bool isActive() {
			return (endGameScreen.isInited () && !endGameScreen.isDone());
		}
		public bool isDone() {
			return endGameScreen.isDone ();
		}
		public void ExitScreen() {
			Console.WriteLine ("ControllerSupport: Exiting EndGame screen!");
			App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
			typeof(EndGameScreen).GetField ("done", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (endGameScreen, true);
			endGameScreen.GetType ().GetMethod ("GoToLobby", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (endGameScreen, new object[] { });
		}
	}
}

