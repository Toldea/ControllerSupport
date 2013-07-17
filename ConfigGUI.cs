using System;
using UnityEngine;

namespace ControllerSupport {
	public class ConfigGUI : IOkCancelCallback, IOkStringCancelCallback {
		ConfigManager configManager;

		public ConfigGUI (ConfigManager configManager, int version) {
			this.configManager = configManager;

			if (configManager.IsNewInstall ()) {
				if (OsSpec.getOS () == OSType.OSX) {
					App.Popups.ShowOkCancel (this, "choose_controller", "Welcome to ControllerSupport", "Control the game and the Arena menu with your controller!\n\nThis mod support PS3 controllers on OSX natively and Xbox 360 controllers with the Taggiebogle driver.\n\nWhat Controller would you like to use?\n(Change this or review controls in the Settings or ingame menu)", "Xbox 360", "PS3");
				} else {
					App.Popups.ShowOk(this, "new_install", "Welcome to ControllerSupport", "Control the game and the Arena menu with your controller!\n\nThis mod supports Xbox 360 controllers natively and PS3 controllers through MotionJoy (set to Xbox Emulator mode).\n\nReview the controls at any time in the Settings or ingame menu.", "Controls");
				}
			} else if (configManager.IsNewVersion (version)) {
				//Console.WriteLine ("ControllerSupport: ConfigManager: New version detected!");
			} else {
				//Console.WriteLine ("ControllerSupport: ConfigManager: Version up to date!");
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
				if (configManager.UsingPS3 ()) {
					App.Popups.ShowScrollText (null, "control_scheme", "Controls", 
					                           "[Main Menu]\n" +
					                           "Left Stick or DPad: Move\n" +
					                           "X: Accept\n" +
					                           "Circle: Cancel\n" +
					                           "Triangle: View match invite\n" +
					                           "L1 or R1: Switch between menus\n" +
					                           "Start: Quit the game\n" +
					                           "\n[Game]\n" +
					                           "Left Stick or DPad: Move through cards / Move on board\n" +
					                           "\nX: Accept / Play\n" +
					                           "Circle: Cancel\n" +
					                           "Square: Cycle through recently played scrolls\n" +
					                           "Triangle: End turn\n" +
					                           "\nL1 + X: Sacrifice for Growth\n" +
					                           "L1 + Square: Sacrifice for Order\n" +
					                           "L1 + Triangle: Sacrifice for Energy\n" +
					                           "L1 + Circle: Sacrifce for Scrolls\n" +
					                           "R1: Magnify selected Scroll\n" +
					                           "\nLeft Stick Click: Take control of the board\n" +
					                           "Right Stick Click: Toggle show stats\n" +
					                           "\nStart: Show menu\n" +
					                           "Select: Show chat\n" +
					                           "Select + X: \'Hello and good luck.\'\n" +
					                           "Select + Circle: \'Good Game.\'\n" +
					                           "Select + Square: \'Nice play!\'\n" +
					                           "Select + Triangle: \':)\'\n"
					                           , "Ok");
				} else {
					App.Popups.ShowScrollText (null, "control_scheme", "Controls", 
					                           "[Main Menu]\n" +
					                           "Left Stick or DPad: Move\n" +
					                           "A: Accept\n" +
					                           "B: Cancel\n" +
					                           "Y: View match invite\n" +
					                           "LB or RB: Switch between menus\n" +
					                           "Start: Quit the game\n" +
					                           "\n[Game]\n" +
					                           "Left Stick or DPad: Move through cards / Move on board\n" +
					                           "\nA: Accept / Play\n" +
					                           "B: Cancel\n" +
					                           "X: Cycle through recently played scrolls\n" +
					                           "Y: End turn\n" +
					                           "\nLB + A: Sacrifice for Growth\n" +
					                           "LB + X: Sacrifice for Order\n" +
					                           "LB + Y: Sacrifice for Energy\n" +
					                           "LB + B: Sacrifce for Scrolls\n" +
					                           "RB: Magnify selected Scroll\n" +
					                           "\nLeft Stick Click: Take control of the board\n" +
					                           "Right Stick Click: Toggle show stats\n" +
					                           "\nStart: Show menu\n" +
					                           "Back: Show chat\n" +
					                           "Back + A: \'Hello and good luck.\'\n" +
					                           "Back + B: \'Good Game.\'\n" +
					                           "Back + X: \'Nice play!\'\n" +
					                           "Back + Y: \':)\'\n"
					                           , "Ok");
				}
			} else {
				App.Popups.ShowScrollText (null, "control_scheme", "Controls", 
				                           "[Main Menu]\n" +
				                           "Left Stick: Move\n" +
				                           "A: Accept\n" +
				                           "B: Cancel\n" +
				                           "Y: View match invite\n" +
				                           "LB or RB: Switch between menus\n" +
				                           "Start: Quit the game\n" +
				                           "\n[Game]\n" +
				                           "Left Stick: Move through cards / Move on board\n" +
				                           "\nA: Accept / Play\n" +
				                           "B: Cancel\n" +
				                           "X: Cycle through recently played scrolls\n" +
				                           "Y: End turn\n" +
				                           "\nLB + A: Sacrifice for Growth\n" +
				                           "LB + X: Sacrifice for Order\n" +
				                           "LB + Y: Sacrifice for Energy\n" +
				                           "LB + B: Sacrifce for Scrolls\n" +
				                           "RB: Magnify selected Scroll\n" +
				                           "\nLeft Stick Click: Take control of the board\n" +
				                           "Right Stick Click: Toggle show stats\n" +
				                           "\nStart: Show menu\n" +
				                           "Back: Show chat\n" +
				                           "Back + A: \'Hello and good luck.\'\n" +
				                           "Back + B: \'Good Game.\'\n" +
				                           "Back + X: \'Nice play!\'\n" +
				                           "Back + Y: \':)\'\n"
				                           , "Ok");
			}
		}
	}
}

