using System;

namespace ControllerSupport {
	public class ControllerKeyBindings {
		private OSType osType;
		private bool useOSX_PS3 = false;
		public ControllerKeyBindings () {
			osType = OsSpec.getOS ();
		}
		public void SetUsePS3(bool usePS3) {
			useOSX_PS3 = usePS3;
		}

		/**** Axis Mappings ****/
		public string LEFT_STICK_HORIZONTAL_AXIS {
			get { return "Horizontal"; }
		}
		public string LEFT_STICK_VERTICAL_AXIS {
			get { return "Vertical"; }
		}

		/**** Button Mappings ****/
		public string A {
			get { 
				if (osType == OSType.Windows) {
					return "joystick button 0";
				} else {
					return (useOSX_PS3) ? "joystick button 14" : "joystick button 16" ; 
				}
			}
		}
		public string B {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 1";
				} else {
					return (useOSX_PS3) ? "joystick button 13" : "joystick button 17" ; 
				}
			}
		}
		public string X {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 2";
				} else {
					return (useOSX_PS3) ? "joystick button 15" : "joystick button 18" ; 
				}
			}
		}
		public string Y {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 3";
				} else {
					return (useOSX_PS3) ? "joystick button 12" : "joystick button 19" ; 
				}
			}
		}
		public string LB {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 4";
				} else {
					return (useOSX_PS3) ? "joystick button 10" : "joystick button 13" ; 
				}
			}
		}
		public string RB {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 5";
				} else {
					return (useOSX_PS3) ? "joystick button 11" : "joystick button 14" ; 
				}
			}
		}
		public string BACK {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 6";
				} else {
					return (useOSX_PS3) ? "joystick button 0" : "joystick button 10" ; 
				}
			}
		}
		public string START {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 7";
				} else {
					return (useOSX_PS3) ? "joystick button 3" : "joystick button 9" ; 
				}
			}
		}
		public string LEFT_STICK_CLICK {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 8";
				} else {
					return (useOSX_PS3) ? "joystick button 1" : "joystick button 11" ; 
				}
			}
		}
		public string RIGHT_STICK_CLICK {
			get {
				if (osType == OSType.Windows) {
					return "joystick button 9";
				} else {
					return (useOSX_PS3) ? "joystick button 2" : "joystick button 12" ; 
				}
			}
		}

		/**** OSX Only Buttons (DPad) ****/
		public string DPAD_UP {
			get {
				if (osType == OSType.Windows) {
					return "UNDEFINED";
				} else {
					return (useOSX_PS3) ? "joystick button 4" : "joystick button 5" ; 
				}
			}
		}
		public string DPAD_DOWN {
			get {
				if (osType == OSType.Windows) {
					return "UNDEFINED";
				} else {
					return (useOSX_PS3) ? "joystick button 6" : "joystick button 6" ; 
				}
			}
		}
		public string DPAD_LEFT {
			get {
				if (osType == OSType.Windows) {
					return "UNDEFINED";
				} else {
					return (useOSX_PS3) ? "joystick button 7" : "joystick button 7" ; 
				}
			}
		}
		public string DPAD_RIGHT {
			get {
				if (osType == OSType.Windows) {
					return "UNDEFINED";
				} else {
					return (useOSX_PS3) ? "joystick button 5" : "joystick button 8" ; 
				}
			}
		}
	}
}

