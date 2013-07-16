using System;

namespace ControllerSupport {
	public class ConfigGUI : IOkCancelCallback, IOkStringCancelCallback {
		ConfigManager configManager;

		public ConfigGUI (ConfigManager configManager, int version) {
			this.configManager = configManager;

			// TEMP ALWAYS SHOW CONTROLS POPUP AT START!
			//ShowControlScheme ();

			if (configManager.IsNewInstall ()) {
				if (OsSpec.getOS () == OSType.OSX) {
					App.Popups.ShowOkCancel (this, "choose_controller", "Welcome to ControllerSupport", "What Controller would you like to use?", "Xbox 360", "PS3");
				} else {
					App.Popups.ShowOk(null, "new_install", "Welcome to ControllerSupport", "First time?", "Ok");
				}
			} else if (configManager.IsNewVersion (version)) {
				Console.WriteLine ("ControllerSupport: ConfigManager: New version detected!");
			} else {
				Console.WriteLine ("ControllerSupport: ConfigManager: Version up to date!");
			}
		}

		public void PopupCancel(String type) {
			if (type == "choose_controller") {
				configManager.SetUsePS3 (true);
				ShowControlScheme ();
			}
		}
		public void PopupOk(String type) {
			if (type == "choose_controller") {
				configManager.SetUsePS3 (false);
				ShowControlScheme ();
			}
		}
		public void PopupOk(String type, String choice) {}

		public void ShowControlScheme() {
			App.Popups.ShowScrollText (this, "control_scheme", "Controls", 
			                           "[Main Menu]\n" +
			                           "Left Stick / (OSX)DPad: Move\n" +
			                           "A: Accept\n" +
			                           "B: Cancel\n" +
			                           "Y: View match invite\n" +
			                           "LT/RT: Switch between menus,\n" +
			                           "Start: Quit the game\n" +
			                           "\n[Game]\n" +
			                           "Left Stick / (OSX)DPad: Move through cards / Move on board\n" +
			                           "\nA: Accept / Play\n" +
			                           "B: Cancel\n" +
			                           "X: Cycle through recently played scrolls\n" +
			                           "Y: End turn\n" +
			                           "\nLT + A: Sacrifice for Growth\n" +
			                           "LT + X: Sacrifice for Order\n" +
			                           "LT + Y: Sacrifice for Energy\n" +
			                           "LT + B: Sacrifce for Scrolls\n" +
			                           "RT: Magnify selected Scroll\n" +
			                           "\nLeft Stick Click: Take control of board\n" +
			                           "Right Stick Click: Toggle show stats\n" +
			                           "\nBack: Show chat\n" +
			                           "Back + A: \'Hello and good luck.\'\n" +
			                           "Back + B: \'Good Game.\'\n" +
			                           "Back + X: \'Nice play!\'\n" +
			                           "Back + Y: \':)\'\n"
			                          , "Ok");
		}
	}
}

