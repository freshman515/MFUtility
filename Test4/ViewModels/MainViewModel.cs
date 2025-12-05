using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Logging;
using MFUtility.Logging.Enums;

namespace Test4.ViewModels;

public partial class MainViewModel :ObservableObject{
	public MainViewModel() {
		LogManager.Configure()
			.WriteTo(w => {
				w.Console();
				w.File(f => {
					f.MaxFileSizeMB(10)
						.UseAppFolder()
						.UseSolutionPath()
						.Async()
						.UseDateFolder();
				});
				w.JsonFile(j => j.InheritFromFile()
					           .Indented(true)
					           .UseJsonArrayFile()
				);
			})
			.Format(f => {
				f.Include(i => {
					i.ClassName()
						.Assembly()
						.LineNumber();
				});
				f.Style(s => {
					s.TimeFormat("yyyy-MM-dd HH:mm:ss")
						.UseFieldTag(false);
				});
				f.OrderDefault();
			})
			.Level(LogLevel.Debug)
			.Apply();
	}
	[RelayCommand]
	private void Log() {
		LogManager.Info("Test4");
	}
}