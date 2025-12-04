MFUtility.Logging 提供：

🌈 流式 Fluent API 配置

📝 多日志管道：Console / File / JSON

📁 日期目录 / 自动文件分片

📦 异步写入

🔧 灵活可扩展的 Provider 模型

📌 可自定义格式（包含类名、方法、行号等）

🚀 安装
dotnet add package MFUtility.Logging


或 NuGet UI 搜索：MFUtility.Logging

🔥 快速开始

最小可运行示例：

LogManager.Configure()
    .WriteTo(c => {
        c.Console();
        c.File(f => f.UseDateFolder());
    })
    .Level(LogLevel.Debug)
    .Apply();

LogManager.Info("Hello world!");

🧱 Fluent 配置（推荐写法）

MFUtility.Logging 使用类似 Serilog 的 Fluent API：

LogManager.Configure()
    .WriteTo(c => {
        c.Console();

        c.File(f => f.UseDateFolder()
                     .SetPath("../runtime")
                     .Async()
        );

        c.JsonFile(j => j.InheritFromFile());
    })
    .Format(f => {
        f.IncludeMethodName();
        f.IncludeLineNumber();
    })
    .Level(LogLevel.Debug)
    .Apply();

🖥️ WriteTo：输出管道
✔ Console 输出
c.Console();
c.Console(color: true);

✔ File 文件日志
c.File(f => f
    .SetPath("logs")
    .UseDateFolder()
    .MaxFileSizeMB(10)
    .Async()
);


你也可以只启用文件（使用默认配置）：

c.File();

✔ JSON 文件日志
c.JsonFile(j => j
    .UseDateFolder()
    .SplitDaily()
    .Indented(false)
);


或启用 JSON 默认配置：

c.JsonFile();

🧩 Format：日志格式控制
.Format(f => {
    f.IncludeMethodName();
    f.IncludeLineNumber();
    f.IncludeAssembly();
    f.UseTimeFormat("yyyy-MM-dd HH:mm:ss");
    f.UseFieldBrackets();
});


支持字段：

时间戳

日志等级

类名

方法名

行号

程序集名

自定义括号

多字段排列顺序

📊 Level：最低日志级别
.Level(LogLevel.Debug)


支持：

Trace

Debug

Info

Warn

Error

Fatal

📝 在代码中写日志
LogManager.Debug("Debug message");
LogManager.Info("Something happened");
LogManager.Warn("Something looks wrong");
LogManager.Error("Oops!", ex);

📁 输出结构示例
/runtime/
    /2025/
        /02/
            /04/
                app.log
                app.json

💡 高级：自定义日志 Provider

你可以扩展自己的输出方式，例如数据库、HTTP、ElasticSearch。

public class MyProvider : ILogProvider {
    public void Write(LogEvent evt) {
        // custom write logic
    }
}


注册：

.WriteTo(c => {
    c.Provider(new MyProvider());
})

🧪 单元测试中使用
LogManager.Configure()
    .WriteTo(c => c.Console())
    .Level(LogLevel.Trace)
    .Apply();


支持手动收集日志事件等高级功能。

📦 配置结构（内部使用）

框架使用统一的 LogConfiguration 管理所有配置，不需要手动创建。

🔧 异常格式化行为

支持：

换行模式（默认）

单行压缩模式（ExceptionNewLine = false）

🧩 Caller 信息（类名、方法、行号）

框架自动捕获：

文件

方法名

行号

程序集

类名

不需要用户手写。

📚 示例项目

你可以创建一个完整示例：

ConsoleApp
 └── Program.cs