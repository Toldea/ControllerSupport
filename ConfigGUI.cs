using System;

namespace ControllerSupport {
	public class ConfigGUI : IOkCancelCallback, IOkStringCancelCallback {
		ConfigManager configManager;
		ControllerKeyBindings controllerBindings;

		public ConfigGUI (ConfigManager configManager, int version, ControllerKeyBindings controllerBindings) {
			this.configManager = configManager;
			this.controllerBindings = controllerBindings;

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
				controllerBindings.SetUsePS3 (true);
			}
		}
		public void PopupOk(String type) {
			if (type == "choose_controller") {
				configManager.SetUsePS3 (false);
				controllerBindings.SetUsePS3 (false);
			}
		}
		public void PopupOk(String type, String choice) {}
	}
}

