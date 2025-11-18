namespace MFUtility.Core.Bus;

internal record IpcMessage(string Scope, string MessageName, object[]? Args) {
		public string Scope { get; } = Scope;
		public string MessageName { get; } = MessageName;
		public object[]? Args { get; } = Args;
	}