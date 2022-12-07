using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagramNet.Elements {

	/// <summary>
	/// This class helps with Controllers.
	/// </summary>
	internal static class ControllerHelper {
		//private ControllerHelper() {	}

		public static IMoveController? GetMoveController(this BaseElement el) {
			if (el is IControllable controllable) {
				IController ctrl = controllable.GetController();
				if (ctrl is IMoveController controller)
					return controller;
				else
					return null;
			} else
				return null;
		}

		public static IResizeController? GetResizeController(this BaseElement el) {
			if (el is IControllable controllable) {
				IController ctrl = controllable.GetController();
				return ctrl.GetResizeController();
			} else
				return null;
		}

		public static IResizeController? GetResizeController(this IController ctrl) {
			return ctrl as IResizeController;
		}

		public static ILabelController? GetLabelController(this BaseElement el) {
			if ((el is IControllable controllable) && (el is ILabelElement)) {
				IController ctrl = controllable.GetController();
				return ctrl.GetLabelController();
			} else
				return null;
		}

		public static ILabelController? GetLabelController(this IController ctrl) {
			return ctrl as ILabelController;
		}
	}
}
