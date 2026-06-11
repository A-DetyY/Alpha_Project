# 聊天对话界面设计规格

**日期：** 2026-06-11  
**项目：** YongGo Alpha (Unity 2022.3.56f1c1 / Android)  
**状态：** 已确认

---

## 概述

实现一个全屏聊天对话页面，用户可以输入文字消息，系统随机生成中文回复。界面采用 RPG 游戏深色风格，使用 Unity UGUI + TextMeshPro 构建。

---

## 技术栈

| 项目 | 选择 |
|------|------|
| UI 系统 | Unity UGUI (ScrollRect + VerticalLayoutGroup) |
| 文字渲染 | TextMeshPro 3.0.7，Dynamic Atlas 模式 |
| 字体 | 阿里巴巴普惠体 3.0 Light（`Assets/Res/Fonts/AlibabaPuHuiTi-3-45-Light.ttf`） |
| 目标平台 | Android 竖屏 |

---

## 视觉风格

- **背景：** 深色渐变（`#0d0a1e` → `#0a1628`）
- **主色调：** 紫色（`#6a3fb5`）+ 青蓝色（`#00b4dc`）
- **边框：** 半透明发光边框（紫色系）
- **系统消息气泡：** 半透明紫色背景 + 紫色边框，左侧显示
- **用户消息气泡：** 半透明青蓝色背景 + 青蓝色边框，右侧显示

---

## 界面布局

```
┌─────────────────────────────┐
│  ● 系 统 对 话          在线 │  ← 顶部标题栏（固定高度）
├─────────────────────────────┤
│                             │
│  [系] 欢迎来到系统终端...    │
│                             │
│          你好，我需要帮助 [我]│
│                             │
│  [系] 已收到请求，处理中...  │  ← 消息列表（可滚动，flex:1）
│                             │
│                             │
│                             │
├─────────────────────────────┤
│  [ 输入消息...        ] [发送]│  ← 底部输入栏（固定高度）
└─────────────────────────────┘
```

### 顶部标题栏
- 高度：固定 ~56dp
- 内容：青蓝色圆形状态指示点 + "系统对话"文字 + "在线"标签
- 背景：半透明紫色 + 底部发光边框线

### 消息列表
- 占满顶部栏与输入栏之间的所有空间（`Rect Transform` 拉伸填充）
- `ScrollRect`：垂直滚动，关闭水平滚动
- `Content`：挂载 `VerticalLayoutGroup`（从上到下排列，`Child Force Expand Width` 关闭）+ `ContentSizeFitter`（垂直方向 `Preferred Size`）
- 新消息添加后，自动通过 `ScrollRect.normalizedPosition = Vector2.zero` 滚到底部

### 消息气泡
- 每条消息是一个独立 Prefab，包含：
  - 头像图标（28×28 方形，圆角4px）：系统显示"系"，用户显示"我"
  - `TextMeshPro` 文字标签（自动换行，`Best Fit` 关闭）
  - 气泡背景（`Image` + 9-Slice 圆角图片，或纯色 + 圆角 Shader）
- 气泡最大宽度：屏幕宽度的 65%
- 系统消息：整体左对齐，头像在左，气泡在头像右侧
- 用户消息：整体右对齐，气泡在左，头像在气泡右侧

### 底部输入栏
- 高度：固定 ~64dp
- `TMP_InputField`：支持中文输入，`Content Type = Standard`，`Line Type = Single Line`
- 发送按钮：渐变背景（紫 → 青），点击触发发送逻辑

---

## 数据与逻辑

### 文件结构
```
Assets/
├── Res/
│   └── Fonts/
│       ├── AlibabaPuHuiTi-3-45-Light.ttf
│       └── AlibabaPuHuiTi-3-45-Light SDF.asset   ← TMP Font Asset
├── Scripts/
│   └── Chat/
│       ├── ChatManager.cs        ← UI 与交互逻辑
│       ├── MessageItem.cs        ← 单条消息组件
│       └── SystemResponder.cs    ← 系统角色回复逻辑（可扩展）
├── Prefabs/
│   └── Chat/
│       ├── MessageItem_System.prefab
│       └── MessageItem_User.prefab
└── Scenes/
    └── ChatScene.unity
```

### ChatManager 职责
1. 接收用户输入，创建用户消息项并添加到列表
2. 清空输入框，保持焦点
3. 调用 `SystemResponder.GetReply()` 获取回复内容，通过协程延迟后创建系统消息项
4. 每次添加消息后，调用 `Canvas.ForceUpdateCanvases()` 然后将 ScrollRect 滚动到底部

### MessageItem 职责
- 持有对 `TMP_Text`（消息内容）的引用
- 提供 `SetContent(string text)` 方法

### SystemResponder 职责
- 封装系统回复的生成逻辑，与 UI 完全解耦
- 当前实现：预设 10 条以上中文回复文本，`GetReply(string userMessage)` 随机返回一条
- 后续可在此脚本内替换为 API 调用、对话树、条件判断等逻辑，`ChatManager` 无需改动
- 回复延迟（0.5~1.5 秒随机）由 `ChatManager` 的协程控制，不在此脚本内

---

## 中文支持要点

- `TMP_InputField` 的 `fontAsset` 和所有 `TMP_Text` 组件均指向阿里巴巴普惠体 Font Asset
- Font Asset 的 `Atlas Population Mode` 设置为 `Dynamic`，运行时自动加载字形
- `TMP_InputField` 的 `Content Type` 设为 `Standard`（不限制输入字符类型）

---

## 不在本期范围内

- 消息时间戳
- 消息已读/未读状态
- 表情包、图片消息
- 消息本地持久化
- 打字动画效果（"..."）
- 对象池优化
