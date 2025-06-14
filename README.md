# EilansPlugins  
专为音乐游戏开发设计的高效数据库与工具库

[![.NET Framework Version](https://shields.io/badge/.NET_Framework-4.8-blue)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

EilansPlugins 提供了一套完整的解决方案，用于音乐游戏中常见需求的开发组件：
📈 实现动画过渡的缓动函数  
⏱️ 处理BPM变化的时间计算  
🔁 控制动态变化的事件曲线系统  

---

## ✨ 核心组件

### 1. 优先列表（PriorityList）
结合列表随机访问和链表高效插入的优势，实现时间复杂度优化的优先级管理系统：

```csharp
// 创建具有自定义比较器的优先列表
PriorityList list = new PriorityList((a, b) => a.CompareTo(b));

// 插入元素
list.Add(5);  
list.Add(3);  
list.Add(8);

// 有序访问：输出 3,5,8
foreach(var item in list) {
    Console.WriteLine(item);
}
```

---

### 2. 缓动函数（Ease）
提供16种缓动类型，实现动画过渡效果

```csharp
// 通用调用方式（指定缓动类型和变换方式）
double x = Ease.GetEase(TransformType.Sine, EaseType.In, 0.5);

// 基本方法（预定义常用缓动）
double y = Ease.InSine(0.5);      // Sine-in 缓动
double z = Ease.OutElastic(0.7);  // Elastic-out 缓动
```

---

### 3. 事件曲线系统（CurveEventList）
动态管理时间轴上的数值变化事件，同时也有线性速度事件，可以计算和自动缓存位移

```csharp
// 创建事件系统（起始值=0）
CurveEventList eventList = new CurveEventList(0);

// 切分时间点创建新事件
eventList.Split(5, 10, 15, 20);  // 在5s,10s,15s,20s分割时间，即创建事件点

// 配置第一个事件
eventList.SetValueEnd(0, 30);   // 结束值=30
eventList.SetTimeEnd(0, 9);     // 结束时间=9s

// 修改缓动类型
if (eventList[0] is EasingCurveEvent e) 
    e.TransformType = TransformType.Back;

// 查询任意时刻的值
Console.WriteLine(eventList.GetValue(7.5)); // 计算过渡值
```

---

### 4. BPM管理器（TimeManager）
处理动态BPM变化，实现复杂节拍映射：

```csharp
// 初始化（起始BPM=120）
TimeManager timeManager = new TimeManager(120);

// 在20s处添加BPM变化点
timeManager.BPMEvent.Split(20);
timeManager.BPMEvent.SetValueStart(0, 200); // 20s时BPM变为200

// 计算时间点对应的节拍位置
double beatPosition = timeManager.GetBeat(25);
// ← 25秒时的精确节拍计数

// 查询BPM变化
double currentBPM = timeManager.GetBPM(18); // 18秒时的BPM值

// 时间取模
double time = timeManager.RoundBeatTime(2.2, 4);
```

---

## 🚀 应用场景
- 节奏游戏事件系统
- UI元素动画过渡
- 节拍映射
