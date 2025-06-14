# EilansPlugins  
An efficient database and utility library specifically designed for music game development  

[![.NET Framework Version](https://shields.io/badge/.NET_Framework-4.8-blue)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

EilansPlugins provides a comprehensive solution for developing components required in music games:  

📈 Easing functions for animation transitions  
⏱️ Time calculations handling BPM changes  
🔁 Event curve system managing dynamic value changes  

[简体中文](README-zh_CN.md)

---

## ✨ Core Components  

### 1. Priority List  
Combines the advantages of list random access and linked list efficient insertion, implementing an optimized priority management system with superior time complexity:  

```csharp  
// Create priority list with custom comparer  
PriorityList list = new PriorityList((a, b) => a.CompareTo(b));  

// Insert elements  
list.Add(5);  
list.Add(3);  
list.Add(8);  

// Ordered access: Outputs 3,5,8  
foreach(var item in list) {  
    Console.WriteLine(item);  
}  
```  

---

### 2. Easing Functions (Ease)  
Provides 16 easing types for animation transitions:  

```csharp  
// Universal invocation (specify ease type and transform)  
double x = Ease.GetEase(TransformType.Sine, EaseType.In, 0.5);  

// Basic methods (predefined common eases)  
double y = Ease.InSine(0.5);      // Sine-in easing  
double z = Ease.OutElastic(0.7);  // Elastic-out easing  
```  

---

### 3. Event Curve System (CurveEventList)  
Dynamically manages value-change events on timelines, including linear velocity events that compute and automatically cache displacements:  

```csharp  
// Create event system (initial value=0)  
CurveEventList eventList = new CurveEventList(0);  

// Split timeline at points  
eventList.Split(5, 10, 15, 20);  // Creates events at 5s,10s,15s,20s  

// Configure first event  
eventList.SetValueEnd(0, 30);   // End Value = 30  
eventList.SetTimeEnd(0, 9);     // End Time = 9s  

// Modify easing type  
if (eventList[0] is EasingCurveEvent e)  
    e.TransformType = TransformType.Back;  

// Query value at any time  
Console.WriteLine(eventList.GetValue(7.5)); // Computes transition value  
```  

---

### 4. BPM Manager (TimeManager)  
Handles dynamic BPM changes and implements complex beat mapping:  

```csharp  
// Initialize (starting BPM=120)  
TimeManager timeManager = new TimeManager(120);  

// Add BPM change at 20s  
timeManager.BPMEvent.Split(20);  
timeManager.BPMEvent.SetValueStart(0, 200); // BPM becomes 200 at 20s  

// Calculate beat position from time  
double beatPosition = timeManager.GetBeat(25);  
// ← Precise beat count at 25 seconds  

// Query BPM changes  
double currentBPM = timeManager.GetBPM(18); // BPM value at 18s  

// Time modulo calculation  
double time = timeManager.RoundBeatTime(2.2, 4);  
```  

---

## 🚀 Application Scenarios  
- Rhythm game event systems  
- UI element animation transitions  
- Beat mapping
