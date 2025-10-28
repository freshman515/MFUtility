using Newtonsoft.Json;

namespace MFUtility.Communication.Socket.Messages {
	public class SocketMessage {
		public SocketMessage(string sender, string target, params object[] parameters) {
			Sender = sender;
			Target = target;
			foreach (var param in parameters)
				Parameters.Add(new MessageParam(param));
		}
		public SocketMessage() {
			
		}
		public string Sender { get; set; } = "";
		public string Target { get; set; } = ""; // 目标
		public List<MessageParam> Parameters { get; set; } = new();
		public DateTime Timestamp { get; set; } = DateTime.Now;
		public static string Serialize(SocketMessage msg) {
			return JsonConvert.SerializeObject(msg);
		}
		public static SocketMessage? Deserialize(string json) {
			return JsonConvert.DeserializeObject<SocketMessage>(json);
		}
		public void Add(object value) => Parameters.Add(new MessageParam(value));
		public void Add(object value, int index) => Parameters.Insert(index, new MessageParam(value));
		public T? Get<T>(int index) {
			if (index >= 0 && index < Parameters.Count)
				return Parameters[index].GetValue<T>();
			return default;
		}
	}
}
