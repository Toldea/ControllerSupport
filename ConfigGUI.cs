using System;
using UnityEngine;

namespace ControllerSupport {
	public class ConfigGUI : IOkCancelCallback, IOkStringCancelCallback {
		ConfigManager configManager;

		public ConfigGUI (ConfigManager configManager, int version) {
			this.configManager = configManager;

			// TEMP ALWAYS SHOW CONTROLS POPUP AT START!
			//ShowControlScheme ();

			if (configManager.IsNewInstall ()) {
				if (OsSpec.getOS () == OSType.OSX) {
					App.Popups.ShowOkCancel (this, "choose_controller", "Welcome to ControllerSupport", "Control the game and the Arena menu with your controller!\n(Other screens coming soon!)\n\nThis mod support PS3 controllers on OSX natively and Xbox 360 controllers with the Taggiebogle driver.\n\nWhat Controller would you like to use?", "Xbox 360", "PS3");
				} else {
					App.Popups.ShowOk(this, "new_install", "Welcome to ControllerSupport", "Control the game and the Arena menu with your controller!\n(Other screens coming soon!)\n\nThis mod supports Xbox 360 controllers natively and PS3 controlls through MotionJoy (set to Xbox emulator mode).", "Controls");
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
			} else if (type == "new_install") {
				ShowControlScheme ();
			}
		}
		public void PopupOk(String type) {
			if (type == "choose_controller") {
				configManager.SetUsePS3 (false);
				ShowControlScheme ();
			} else if (type == "new_install") {
				ShowControlScheme ();
			}
		}
		public void PopupOk(String type, String choice) {}

		public void ShowControlScheme() {
			if (OsSpec.getOS () == OSType.OSX) {
				App.Popups.ShowScrollText (this, "control_scheme", "Controls", 
				                           "[Main Menu]\n" +
				                           "Left Stick or DPad: Move\n" +
				                           "A: Accept\n" +
				                           "B: Cancel\n" +
				                           "Y: View match invite\n" +
				                           "LT and RT: Switch between menus,\n" +
				                           "Start: Quit the game\n" +
				                           "\n[Game]\n" +
				                           "Left Stick or DPad: Move through cards / Move on board\n" +
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
			} else {
				App.Popups.ShowScrollText (this, "control_scheme", "Controls", 
				                           "[Main Menu]\n" +
				                           "Left Stick: Move\n" +
				                           "A: Accept\n" +
				                           "B: Cancel\n" +
				                           "Y: View match invite\n" +
				                           "LT/RT: Switch between menus,\n" +
				                           "Start: Quit the game\n" +
				                           "\n[Game]\n" +
				                           "Left Stick: Move through cards / Move on board\n" +
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
}

