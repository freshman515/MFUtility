using System.Windows;
using MFUtility.Bases;
using MFUtility.Dialogs;

namespace Test1;

public partial class HintDialog :BaseDialog{
	public HintDialog() {
		InitializeComponent();
	}
	private void OnOk(object sender, RoutedEventArgs e) {
		DialogResult = true;
		BeginFadeOut();
	}
}