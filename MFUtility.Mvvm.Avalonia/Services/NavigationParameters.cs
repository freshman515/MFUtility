namespace MFUtility.Mvvm.Avalonia.Services;

public class NavigationParameters : Dictionary<string, object?>
{
    public NavigationParameters() { }

    public NavigationParameters(string queryString)
    {
        // 支持 "?a=1&b=2"
        if (queryString.StartsWith("?"))
            queryString = queryString.Substring(1);

        var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in pairs)
        {
            var kv = p.Split('=');
            if (kv.Length == 2)
                this[kv[0]] = Uri.UnescapeDataString(kv[1]);
        }
    }

    public T Get<T>(string key)
    {
        if (TryGetValue(key, out var value) && value is T t)
            return t;

        return default!;
    }
}