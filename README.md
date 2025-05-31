# EilansPlugins
给音游做的数据库

![.NET Framwork Version](https://shields.io/badge/.NET_Framework-4.8-blue)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 容器类
- ✅ 超NB优先列表
  ```csharp
  PriorityList list = new PriorityList((a, b) => a.CompareTo(b));
  ```
- ✅ Ease缓动
  ```csharp
  double x = Ease.GetEase(TransfromType.Sine, EaseType.In, 0.5);
  double y = Ease.InSine(0.5);
  ```
- ✅ 事件（包括曲线事件和速度事件）
  ```csharp
  //曲线事件
  CurveEvent curveEvent = new CurveEvent(0);

  // 从20秒切分事件
  curveEvent.Divede(20);

  // 切换缓动类型
  if (curveEvent[0] is EasingCurveEventPart easingCurveEventPart)
  {
      easingCurveEventPart.TransformType = TransfromType.Sine;
      easingCurveEventPart.EaseType = EaseType.In;
  }

  // 设置第一个事件区块的结束值以及下一个区块的起始值为200
  curveEvent[0].ValueEnd = 200;
  curveEvent[1].ValueStart = 200;

  // 获取事件第15秒返回的值
  curveEvent.GetValue(15);
  ```
  ```csharp
  //速度事件
  SpeedEvent speedEvent = new SpeedEvent(0);

  // 从20秒切分事件
  speedEvent.Divede(20);

  // 设置第一个事件区块的结束值以及下一个区块的起始值为200
  speedEvent[0].ValueEnd = 200;
  speedEvent[1].ValueStart = 200;

  // 获取事件第15秒返回的值（速度）
  speedEvent.GetValue(15);

  // 获取事件第十秒的位移
  speedEvent.GetDisplacement(30);
  ```
- ✅ BPM计算（变化值BPM，基于曲线事件）
  ```csharp
  TimeManager timeManager = new TimeManager(120);
  timeManager.BPMEvent.Divide(20);
  timeManager.BPMEvent[1].ValueStart = 200;
  double t = timeManager.GetBPM(25);
  ```
## Note辅助类
- ❌ 基类
- ❌ 衍生Note
## 计算器类
- ❌ 分数计算器
- ❌ ACC计算器
- ❌ 能力值计算器
