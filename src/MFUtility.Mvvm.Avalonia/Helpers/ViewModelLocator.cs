namespace MFUtility.Mvvm.Avalonia.Helpers;

public static class ViewModelLocator {
	public static Type? FindViewModelForView(Type viewType) {
		var viewName = viewType.Name; // 不用 FullName
		var vmName = viewName.Replace("View", "ViewModel");

		// 1. 同命名空间优先查找（兼容你现有逻辑）
		var fullViewName = viewType.FullName!;
		var vmFullNameGuess = fullViewName.Replace("View", "ViewModel");

		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
			try {
				var vmType = asm.GetType(vmFullNameGuess);
				if (vmType != null)
					return vmType;
			} catch { }
		}

		// 2. 命名空间不一致：从所有程序集查找类型名匹配的 ViewModel
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
			Type? vmType = null;
			try {
				vmType = asm
				         .GetTypes()
				         .FirstOrDefault(t =>
					                         t.IsClass &&
					                         !t.IsAbstract &&
					                         t.Name == vmName);
			} catch { }

			if (vmType != null)
				return vmType;
		}

		return null;
	}
}