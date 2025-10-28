using Newtonsoft.Json;

namespace MFUtility.Communication.Socket.Messages;

public class MessageParam {
    private string _valueString = "";
    [JsonProperty]   
    public string JValue {
        get => _valueString;
        set => _valueString = value;
    }
    public MessageParam() { }
    public MessageParam(object obj) => SetValue(obj);
    public MessageParam(string strJson, bool bJson) => _valueString = strJson;
    public void SetValue(object val) {
        _valueString = val == null ? "" : JsonConvert.SerializeObject(val);
    }

    public T GetValue<T>() {
        return string.IsNullOrEmpty(_valueString)
            ? default!
            : JsonConvert.DeserializeObject<T>(_valueString)!;
    }

    public string GetJson() => _valueString;
    public void SetJson(string strJson) => _valueString = strJson;

}