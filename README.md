# EilansPlugins
给音游做的数据库

![.NET Framwork Version](https://shields.io/badge/.NET_Framework-4.8-blue)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 优先列表
```csharp
PriorityList list = new PriorityList((a, b) => a.CompareTo(b));
```
## Ease缓动
```csharp
double x = Ease.GetEase(TransfromType.Sine, EaseType.In, 0.5);
double y = Ease.InSine(0.5);
```
## 事件
```csharp
//曲线事件集合
CurveEventCollection curveEventCollection = new CurveEventCollection(0);

// 从20秒切分事件集合
curveEventCollection.Divide(20);

// 切换缓动类型
if (curveEventCollection[0] is EasingCurveEvent easingCurveEvent)
{
    easingCurveEvent.TransformType = TransfromType.Sine;
    easingCurveEvent.EaseType = EaseType.In;
}

// 设置第一个事件的结束值以及下一个区块的起始值为200
curveEventCollection[0].ValueEnd = 200;
curveEventCollection[1].ValueStart = 200;

// 获取事件集合第15秒返回的值
curveEventCollection.GetValue(15);
```
```csharp
//速度事件集合
SpeedEventCollection speedEventCollection = new SpeedEventCollection(0);

// 从20秒切分事件集合
speedEventCollection.Divide(20);

// 设置第一个事件的结束值以及下一个区块的起始值为200
speedEventCollection.SetValueEnd(0, 200);
speedEventCollection.SetValueStart(1, 200);
// 不要像CurveEvent那样直接修改SpeedEvent的值，除非你知道你在做什么！

// 获取事件集合第15秒返回的值（速度）
speedEventCollection.GetValue(15);

// 获取事件集合第十秒的位移
speedEventCollection.GetDisplacement(30);
```
## BPM计算
```csharp
TimeManager timeManager = new TimeManager(120);
timeManager.BPMEvent.Divide(20);
timeManager.BPMEvent[1].ValueStart = 200;
double t = timeManager.GetBPM(25);
