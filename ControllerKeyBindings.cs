using System;

namespace ControllerSupport {
	public class ControllerKeyBindings {
		private OSType osType;
		public ControllerKeyBindings () {
			osType = OsSpec.getOS ();
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
			get { return (osType == OSType.Windows) ? "joystick button 0" : "joystick button 16" ; }
		}
		public string B {
			get { return (osType == OSType.Windows) ? "joystick button 1" : "joystick button 17" ; }
		}
		public string X {
			get { return (osType == OSType.Windows) ? "joystick button 2" : "joystick button 18" ; }
		}
		public string Y {
			get { return (osType == OSType.Windows) ? "joystick button 3" : "joystick button 19" ; }
		}
		public string LB {
			get { return (osType == OSType.Windows) ? "joystick button 4" : "joystick button 13" ; }
		}
		public string RB {
			get { return (osType == OSType.Windows) ? "joystick button 5" : "joystick button 14" ; }
		}
		//public string BACK {
		//	get { return (osType == OSType.Windows) ? "joystick button 6" : "joystick button UNKNOWN!!!" ; }
		//}
		public string START {
			get { return (osType == OSType.Windows) ? "joystick button 7" : "joystick button 9" ; }
		}
		public string LEFT_STICK_CLICK {
			get { return (osType == OSType.Windows) ? "joystick button 8" : "joystick button 11" ; }
		}
		public string RIGHT_STICK_CLICK {
			get { return (osType == OSType.Windows) ? "joystick button 9" : "joystick button 12" ; }
		}

		/**** OSX Only Buttons (DPad) ****/
		public string DPAD_UP {
			get { return (osType == OSType.Windows) ? "UNDEFINED" : "joystick button 5" ; }
		}
		public string DPAD_DOWN {
			get { return (osType == OSType.Windows) ? "UNDEFINED" : "joystick button 6" ; }
		}
		public string DPAD_LEFT {
			get { return (osType == OSType.Windows) ? "UNDEFINED" : "joystick button 7" ; }
		}
		public string DPAD_RIGHT {
			get { return (osType == OSType.Windows) ? "UNDEFINED" : "joystick button 8" ; }
		}
	}
}

