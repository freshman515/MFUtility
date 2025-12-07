using System.Diagnostics;

namespace MFUtility.Helpers;

public static class ProcessHelper {
	public static void RunCmd(string command, bool asAdmin = false) {
		var psi = new ProcessStartInfo("cmd.exe", "/c " + command) {
			CreateNoWindow = true,
			UseShellExecute = asAdmin,
			Verb = asAdmin ? "runas" : ""
		};
		Process.Start(psi);
	}

	public static bool IsRunning(string processName)
		=> Process.GetProcessesByName(processName).Any();
}