# POPYBot — QQ 机器人 .NET SDK

基于 .NET 10 / C# 13 的 QQ 频道机器人开发框架，支持插件化扩展，接口对齐 [QQ Bot API](https://bot.q.qq.com/wiki/)。

---

## 目录

- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [配置文件](#配置文件)
- [插件开发指南](#插件开发指南)
  - [插件生命周期](#插件生命周期)
  - [第一个插件](#第一个插件)
  - [Hook 事件系统](#hook-事件系统)
  - [消息模型](#消息模型)
  - [调用 API](#调用-api)
  - [高级用法](#高级用法)
- [Intents 配置](#intents-配置)
- [日志与调试](#日志与调试)

---

## 快速开始

### 1. 获取凭证

前往 [QQ 机器人开放平台](https://q.qq.com/) 创建机器人，获取 `AppId` 和 `Secret`。

### 2. 配置 config.json

在可执行程序同目录下创建 `config.json`：

```json
{
  "AppId": "你的AppId",
  "Secret": "你的Secret",
  "IsSandbox": false,
  "Timeout": 5,
  "LogLevel": "Debug",
  "Intents": "PublicGuildMessages, DirectMessage, Interaction",
  "PluginsPath": "plugins"
}
```

### 3. 放置插件

将编译好的插件 DLL 放入 `plugins/` 文件夹（程序首次启动会自动创建）。

### 4. 运行

```bash
dotnet run --project Console
```

---

## 项目结构

```
POPYBot/
├── Console/              # 控制台宿主程序入口
│   └── Program.cs        # 启动引导、配置加载
├── Core/                 # 核心 SDK 库
│   ├── BotWebSocket.cs   # WebSocket 网关客户端
│   ├── BotHttp.cs        # HTTP API 封装
│   ├── BotAPI.cs         # REST API（频道/消息/成员等）
│   ├── Client.cs         # 机器人主客户端
│   ├── BotToken.cs       # Token 管理
│   ├── ConfigManager.cs  # 配置管理器
│   ├── Flags.cs          # Intents 枚举定义
│   ├── Plugins/          # 插件系统
│   │   ├── IBotPlugin.cs       # 插件接口定义
│   │   ├── PluginLoader.cs     # DLL 加载器
│   │   └── PluginHookManager.cs # Hook 管理
│   ├── Types/            # 网关数据类型
│   └── Models/           # 事件数据模型
│       ├── Message.cs    # 消息模型
│       ├── Guild.cs      # 频道模型
│       ├── Member.cs     # 成员模型
│       ├── DirectMessage.cs  # 私信/群消息/C2C
│       ├── Interaction.cs    # 互动
│       └── ...
└── Plugins/
    └── SamplePlugin/     # 示例插件
        └── GreetingPlugin.cs
```

---

## 配置文件

`config.json` 所有字段说明：

| 字段 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `AppId` | string | - | 机器人 AppId（必填） |
| `Secret` | string | - | 机器人密钥（必填） |
| `IsSandbox` | bool | false | 是否使用沙箱环境 |
| `Timeout` | int | 5 | HTTP 请求超时（秒） |
| `LogLevel` | string | Information | 日志级别：Debug / Information / Warning / Error |
| `Intents` | string/number | - | 订阅的 Intents，见[下方](#intents-配置) |
| `PluginsPath` | string | "plugins" | 插件目录（相对于可执行程序） |
| `Extra` | object | - | 自定义扩展字段，可供插件读取 |

---

## 插件开发指南

POPYBot 的插件系统基于 **DLL 动态加载 + Hook 注册** 模式。每个插件是一个独立的 .NET 类库项目，编译为 DLL 后放入 `plugins/` 目录即可自动加载。

### 插件生命周期

```
发现 DLL → 加载程序集 → 实例化 IBotPlugin
         → InitializeAsync(context)  ← 注册 Hook
         → [运行中，Hook 被触发]
         → ShutdownAsync()           ← 清理资源
         → 卸载程序集
```

### 第一个插件

#### 1. 创建插件项目

```bash
# 在 Plugins 目录下创建类库项目
dotnet new classlib -n HelloPlugin -o Plugins/HelloPlugin
```

然后编辑 `Plugins/HelloPlugin/HelloPlugin.csproj`，添加对 Core 项目的引用（Core 由宿主加载，设为 `Private="false"`）：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AssemblyName>HelloPlugin</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- 引用 Core 项目；Private=false 表示不复制 Core.dll 到插件输出 -->
    <ProjectReference Include="..\..\Core\POPYBot.Core.csproj" Private="false" />
  </ItemGroup>

</Project>
```

> **注意**：`Plugins/Directory.Build.props` 已统一设置 `TargetFramework`、`ImplicitUsings`、`Nullable`，且 **默认关闭了依赖 DLL 复制**。插件项目只需配置 `AssemblyName` 和 `ProjectReference` 即可。

#### 2. 使用外部 NuGet 依赖（可选）

如果插件需要宿主未加载的第三方库，有两种方式：

**方式 A：嵌入为资源（推荐）**

```xml
<!-- 在插件 .csproj 中添加 -->
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>

<!-- 编译后将依赖 DLL 嵌入为托管资源 -->
<Target Name="EmbedDeps" AfterTargets="Build">
  <ItemGroup>
    <DepFiles Include="$(OutputPath)*.dll" 
              Exclude="$(OutputPath)$(AssemblyName).dll;$(OutputPath)POPYBot.Core.*;$(OutputPath)System.*;$(OutputPath)Microsoft.*" />
  </ItemGroup>
  <Copy SourceFiles="@(DepFiles)" DestinationFolder="$(IntermediateOutputPath)embed\" SkipUnchangedFiles="true" />
  <ItemGroup>
    <EmbeddedResource Include="$(IntermediateOutputPath)embed\*.dll" />
  </ItemGroup>
</Target>
```

`PluginLoadContext` 会自动从插件 DLL 的嵌入资源中加载依赖程序集。

**方式 B：ILPack 合并**

使用 [dotnet-ilrepack](https://github.com/gluck/il-repack) 工具将依赖 DLL 合并到插件 DLL：

```bash
dotnet tool install -g dotnet-ilrepack
dotnet ilrepack /out:merged/HelloPlugin.dll bin/Release/net10.0/HelloPlugin.dll bin/Release/net10.0/Newtonsoft.Json.dll
```

无论哪种方式，输出都是**单个 DLL 文件**。

#### 3. 实现 IBotPlugin

```csharp
using POPYBot;
using POPYBot.Models;
using POPYBot.Plugins;

public class HelloPlugin : IBotPlugin
{
    public string Name => "HelloPlugin";
    public string Version => "1.0.0";

    private IPluginContext? _ctx;

    public Task InitializeAsync(IPluginContext context)
    {
        _ctx = context;

        // 注册 @消息 Hook
        context.Hooks.On(HookEvents.AtMessageCreate, async (Message message) =>
        {
            if (message.Content == "/hello")
            {
                await message.Reply(content: "你好世界！");
            }
        });

        Logger.LogInfo($"[{Name}] 插件初始化完成");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        Logger.LogInfo($"[{Name}] 插件已关闭");
        return Task.CompletedTask;
    }
}
```

#### 4. 编译并部署

```bash
dotnet build -c Release
```

编译后 `bin/Release/net10.0/` 下只有 **3 个文件**：
- `HelloPlugin.dll` — 插件本体（仅此一个 DLL，含嵌入依赖）
- `HelloPlugin.deps.json` — 依赖清单（调试用）
- `HelloPlugin.pdb` — 调试符号

将 `HelloPlugin.dll` 复制到宿主程序的 `plugins/` 目录即可。宿主会自动扫描并加载。

---

### Hook 事件系统

所有可用事件常量定义在 `HookEvents` 类中，传入的数据模型如下：

#### 频道事件

| HookEvents 常量 | 数据模型 | 说明 |
|---|---|---|
| `GuildCreate` | `Guild` | 机器人加入新频道 |
| `GuildUpdate` | `Guild` | 频道信息更新 |
| `GuildDelete` | `Guild` | 机器人退出频道 |
| `ChannelCreate` | `Channel` | 子频道创建 |
| `ChannelUpdate` | `Channel` | 子频道更新 |
| `ChannelDelete` | `Channel` | 子频道删除 |

#### 成员事件

| HookEvents 常量 | 数据模型 | 说明 |
|---|---|---|
| `GuildMemberAdd` | `Member` | 成员加入频道 |
| `GuildMemberUpdate` | `Member` | 成员信息更新 |
| `GuildMemberRemove` | `Member` | 成员退出频道 |

#### 消息事件

| HookEvents 常量 | 数据模型 | 说明 |
|---|---|---|
| `AtMessageCreate` | `Message` | 公域 @机器人消息 |
| `MessageCreate` | `Message` | 频道消息（仅私域） |
| `MessageDelete` | `Message` | 频道消息删除 |
| `PublicMessageDelete` | `Message` | 公域消息删除 |
| `DirectMessageCreate` | `DirectMessage` | 私信消息 |
| `DirectMessageDelete` | `DirectMessage` | 私信撤回 |

#### 群 / C2C 消息

| HookEvents 常量 | 数据模型 | 说明 |
|---|---|---|
| `GroupAtMessageCreate` | `GroupMessage` | 群 @机器人消息 |
| `GroupMessageCreate` | `GroupMessage` | 群聊全量消息（需申请权限） |
| `C2CMessageCreate` | `C2CMessage` | 单聊消息 |
| `GroupAddRobot` | `GroupManageEvent` | 机器人加入群 |
| `GroupDelRobot` | `GroupManageEvent` | 机器人退出群 |
| `GroupMemberAdd` | `GroupManageEvent` | 群成员加入 |
| `GroupMemberRemove` | `GroupManageEvent` | 群成员移出 |
| `FriendAdd` | `C2CManageEvent` | 好友添加 |
| `FriendDel` | `C2CManageEvent` | 好友删除 |

#### 互动 & 表态

| HookEvents 常量 | 数据模型 | 说明 |
|---|---|---|
| `InteractionCreate` | `Interaction` | 互动事件（按钮点击等） |
| `MessageReactionAdd` | `Reaction` | 添加表情表态 |
| `MessageReactionRemove` | `Reaction` | 取消表情表态 |

#### 审核 & 音频 & 论坛

| HookEvents 常量 | 数据模型 | 说明 |
|---|---|---|
| `MessageAuditPass` | `MessageAudit` | 消息审核通过 |
| `MessageAuditReject` | `MessageAudit` | 消息审核不通过 |
| `AudioStart` | `Audio` | 音频开始 |
| `AudioFinish` | `Audio` | 音频结束 |
| `AudioOnMic` | `Audio` | 上麦 |
| `AudioOffMic` | `Audio` | 下麦 |
| `ForumThreadCreate/Update/Delete` | `Thread` | 论坛帖子事件 |
| `ForumPostCreate/Delete` | `Dictionary` | 论坛回帖事件 |
| `OpenForumThreadCreate` 等 | `OpenThread` | 开放论坛事件 |
| `AudioOrLiveChannelMemberEnter/Exit` | `PublicAudio` | 音视频频道进出 |

#### 生命周期事件

| HookEvents 常量 | 参数 | 说明 |
|---|---|---|
| `Ready` | 无 | 机器人上线就绪 |
| `Resumed` | 无 | 重连恢复 |

---

### 消息模型

`Message` 是使用最频繁的模型，主要属性：

```csharp
public class Message
{
    public string? Content;          // 消息文本
    public MessageUser Author;       // 发送者（Id, Username, Avatar）
    public string? ChannelId;        // 子频道 ID
    public string? GuildId;          // 频道 ID
    public string? Id;               // 消息 ID
    public List<Attachment> Attachments;  // 附件列表
    public MessageMember? Member;    // 成员信息（Nick, Roles）

    // 快捷回复方法
    public Task<object?> Reply(
        string? content = null,
        string? image = null,
        object? embed = null,
        object? ark = null,
        object? markdown = null,
        object? keyboard = null);
}
```

#### 消息类型对照

| 场景 | 事件 | 数据模型 | 快捷回复 |
|---|---|---|---|
| 频道公域 @消息 | `AtMessageCreate` | `Message` | `.Reply()` |
| 频道私域消息 | `MessageCreate` | `Message` | `.Reply()` |
| 私信 | `DirectMessageCreate` | `DirectMessage` | `.Reply()` |
| 群消息 | `GroupAtMessageCreate` | `GroupMessage` | `.Reply()` |
| 单聊 | `C2CMessageCreate` | `C2CMessage` | `.Reply()` |

---

### 调用 API

通过 `IPluginContext.Api` 可以调用所有 QQ Bot REST API：

```csharp
public Task InitializeAsync(IPluginContext context)
{
    var api = context.Api;

    // 获取频道信息
    context.Hooks.On(HookEvents.GuildCreate, async (Guild guild) =>
    {
        var channels = await api.GetChannelsAsync<JsonElement>(guild.Id!);
        Logger.LogInfo($"频道 {guild.Name} 有子频道");
    });

    // 发送主动消息（无需消息上下文）
    // await api.PostMessageAsync(channelId, msgId, content: "主动消息");

    // 获取成员信息
    // var member = await api.GetGuildMemberAsync<T>(guildId, userId);
}
```

主要 API 方法：

| 方法 | 说明 |
|---|---|
| `GetGuildAsync<T>(id)` | 获取频道信息 |
| `GetChannelsAsync<T>(id)` | 获取子频道列表 |
| `GetChannelAsync<T>(id)` | 获取子频道详情 |
| `GetGuildMemberAsync<T>(guildId, userId)` | 获取成员信息 |
| `GetGuildMembersAsync<T>(guildId)` | 获取成员列表 |
| `PostMessageAsync(channelId, msgId, ...)` | 发送频道消息 |
| `PostDmsAsync(guildId, msgId, ...)` | 发送私信 |
| `PostGroupMessageAsync(openid, msgId, ...)` | 发送群消息 |
| `PostC2CMessageAsync(openid, msgId, ...)` | 发送单聊消息 |
| `PutReactionAsync(channelId, msgId, type, id)` | 添加表情表态 |
| `DeleteReactionAsync(channelId, msgId, type, id)` | 删除表情表态 |
| `PutInteractionAsync(id, payload)` | 回应互动 |

---

### 高级用法

#### 读取自定义配置

在 `config.json` 中使用 `Extra` 字段传递插件自定义配置：

```json
{
  "Extra": {
    "welcomeMessage": "欢迎加入频道！",
    "adminIds": ["12345", "67890"]
  }
}
```

插件中通过 `ConfigManager` 读取：

```csharp
var config = ConfigManager.Load();
if (config.Extra?.TryGetValue("welcomeMessage", out var msg) == true)
{
    var welcomeMsg = msg.ToString();
}
```

#### 注销 Hook

```csharp
private Delegate? _handler;

public Task InitializeAsync(IPluginContext context)
{
    _handler = async (Message message) => { /* ... */ };
    context.Hooks.On(HookEvents.AtMessageCreate, _handler);
    return Task.CompletedTask;
}

// 需要时注销
public void DisableFeature()
{
    _ctx?.Hooks.Off(HookEvents.AtMessageCreate, _handler!);
}
```

#### 使用自定义 Http 请求

```csharp
// 插件的 InitializeAsync 中
var httpClient = new HttpClient();
// 可使用 api 中包含的认证信息自行发起请求
```

#### 插件间通信

通过静态事件或共享服务实现：

```csharp
// 插件 A
public static event Action<string>? OnBroadcast;

// 插件 B
PluginA.OnBroadcast += msg => Logger.LogInfo($"收到广播: {msg}");
```

---

## Intents 配置

Intents 控制机器人订阅哪些事件。在 `config.json` 中支持两种写法：

### 字符串（推荐）

```json
{
  "Intents": "Guilds, GuildMembers, PublicGuildMessages, DirectMessage, Interaction"
}
```

### 数字

```json
{
  "Intents": 1107296256
}
```

### 所有可用的 Intents 枚举值

| 枚举值 | 位 | 订阅事件 |
|---|---|---|
| `Guilds` | 0 | 频道创建/更新/删除 |
| `GuildMembers` | 1 | 成员加入/更新/退出 |
| `GuildMessages` | 9 | 频道消息（仅私域） |
| `GuildMessageReactions` | 10 | 表情表态 |
| `DirectMessage` | 12 | 私信 |
| `Interaction` | 26 | 互动事件 |
| `MessageAudit` | 27 | 消息审核 |
| `Forums` | 28 | 论坛事件（仅私域） |
| `AudioAction` | 29 | 音频事件 |
| `PublicGuildMessages` | 30 | 公域频道消息 |
| `PublicMessages` | 25 | 群/C2C 消息 |
| `AudioOrLiveChannelMember` | 19 | 音视频频道进出 |
| `OpenForumEvent` | 18 | 开放论坛事件 |

预定义组合：
- `All`：所有事件
- `Default`：除私域消息和论坛外的所有事件

---

## 日志与调试

### 日志级别

在 `config.json` 中设置 `LogLevel`：

```json
{
  "LogLevel": "Debug"
}
```

| 级别 | 内容 |
|---|---|
| `Debug` | 所有 WebSocket 收发数据、API 请求详情 |
| `Information` | 连接状态、插件加载、事件分发 |
| `Warning` | 超时重试、配置警告 |
| `Error` | 连接错误、异常堆栈 |

### 生产部署

```bash
# 发布为独立可执行文件
dotnet publish Console -c Release -r win-x64 --self-contained -o ./publish

# 或在 Linux 上
dotnet publish Console -c Release -r linux-x64 --self-contained -o ./publish
```

将 `publish/` 目录拷贝到服务器，配合 `config.json` 和 `plugins/` 目录运行即可。