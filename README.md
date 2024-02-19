# AkiBTDSL 简介

***Read this document in English: [English Document](./README_EN.md)***

AkiBTDSL是[爱姬kurisu](https://space.bilibili.com/20472331)面向行为树[AkiBT](https://github.com/AkiKurisu/AkiBT)设计的领域特定语言。你可以在游戏运行时对行为树进行热更新,并且使用者无需了解结点的详细内容、无需完整项目便可以进行行为树编写。


## 什么是DSL

领域特定语言 (DSL) 是一种针对特定类型问题的计算机语言，而不是针对任何类型软件问题的通用语言。

[See Reference Article](https://martinfowler.com/dsl.html)

AkiBTDSL解决的问题：不提供项目源码的情况下在游戏运行时或离线对行为树进行修改，从而支持安全的UGC功能


## 特点
* 运行时随时进行编译和反编译，并且可以在Editor中导出BehaviorTreeSO
* 编译器可以完全与项目分离,使用者无需了解项目中特殊结点的详细内容例如方法实现

## 优势
降低输出复杂度，适合例如ChatGPT的大语言模型编写，你可以将结点的脚本做词向量嵌入，让AI根据你的需求生成行为树。

## 安装
1. Download [Release Package](https://github.com/AkiKurisu/AkiBTDSL/releases)
2. Using git URL to download package by Unity PackageManager ```https://github.com/AkiKurisu/AkiBTDSL.git```


## 使用方式

  
<img src="Images/VM.png" />

1. 使用AkiBTCompiler(Tools/AkiBT/AkiBT Compiler)生成一个TypeDictionary
2. 创建GameObject,挂载BehaviorTreeVM组件
3. 在Inspector中拖入写了AkiBTDSL的文本文件
4. 点击Compile编译为行为树或者在运行时使用BehaviorTreeVM的```Compile(string vmCode)```方法
5. 点击Run直接运行。
6. 点击Save将编译出的行为树保存为BehaviorTreeSO

## 原理

由于AkiBT的序列化依赖于```UnityEngine.SerializeReferenceAttribute```的序列化,热更新方案为模仿该序列化的格式从而反序列化为AkiBT行为树。

``BehaviorTreeSerializeReferenceData``即为使用```UnityEngine.SerializeReferenceAttribute```序列化后的Json格式文件,它额外包含了AkiBT行为树的SharedVariables即共享变量,同样也是基于上述Attribute进行序列化。因此我们可以通过修改``BehaviorTreeSerializeReferenceData``来修改反序列化后的结果。但``BehaviorTreeSerializeReferenceData``存在一个人工编写上的困难之处,以下是一个例子：

```
{
  "variables": [
    {
      "rid": 1000
    },
    {
      "rid": 1001
    }
  ],
  "root": {
    "rid": 1004
  },
  "references": {
    "version": 2,
    "RefIds": [
      {
        "rid": 1000,
        "type": {
          "class": "SharedVector3",
          "ns": "Kurisu.AkiBT",
          "asm": "Kurisu.AkiBT"
        },
        "data": {
          "isShared": false,
          "mName": "destination",
          "value": {
            "x": 0.0,
            "y": 0.0,
            "z": 0.0
          }
        }
      }
      ]
      }
}
      
```
我截取了``BehaviorTreeSerializeReferenceData``的部分片段,可以看到由于使用SerializeReference方式序列化，存储的方式为引用位置存储rid，在统一的references集合中存储实际的数据。这对于人工编写非常不便，因此我制作了一个简单的编译器从而可以使用一种更自然的语言来编写脚本即AkiBTDSL。

以下是使用AkiBTDSL编写的行为树：

```
Vector3 destination (0,0,0)
Vector3 myPos (0,0,0)
Float distance 1
Vector3 subtract (0,0,0)
Parallel(children:[
	Sequence(children:[
		Vector3Random(xRange:(-10,10),yRange:(0,0),zRange:(-10,10),operation:1,
		storeResult=>destination ),
		DebugLog(logText:"这是一段Log: Patrol获取了新位置"),
		TimeWait(waitTime:10)
	]),
	Sequence(children:[
		Sequence(children:[
			TransformGetPosition(storeResult=>myPos),
			 Vector3Operator(operation:1,firstVector3=>myPos,
				secondVector3=>destination,storeResult=>subtract),
			Vector3GetSqrMagnitude(vector3=>subtract,result=>distance)
		]),
		Selector(abortOnConditionChanged: false, children:[
			FloatComparison(evaluateOnRunning:false,float1=>distance,
				float2:4,operation:5,child:
				Sequence(abortOnConditionChanged:false,children:[
					NavmeshStopAgent(isStopped:false),
					NavmeshSetDestination(destination=>destination)
				])
			),
			NavmeshStopAgent(isStopped:true)
		])
	])
])


```
上述行为树为AkiBT Example中的巡逻AI行为树,它每10秒会获取一个新的位置并向其移动，如果距离目标点小于2则停止


## 如何编写

DSL主体可分为两部分即公共变量和结点,公共变量的申明需要指明类型、名称和值。

若类型增加了`&`包裹则在运行时会绑定全局变量，例如：

```
$Vector3$ TargetPosition (0,0,0)
```

若变量类型为Object即（SharedObject），则申明值前可以先申明类型例如：
```
Object navAgent "UnityEngine.AI.NavMeshAgent, UnityEngine.AIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" Null
```
则该变量设置为全局且具有``NavMeshAgent``类型限制。

结点我们会跳过Root结点（因为所有行为树都从Root进入），直接从Root的子结点开始编写。

对于结点，你需要申明其类型（名称索引,可以通过自定义TypeDictionary修改，见后文）

对于不使用结点默认值的普通变量，你需要申明其名称（可以使用AkiLabelAttribute进行名称替换）并添加':'后进行赋值

对于结点中的共享变量，如果不需要引用公共变量的共享变量则直接进行赋值，例如

```
TimeWait(waitTime:10)
```
对于需要引用的共享变量，则使用'=>'符号加上需要引用的公共变量名称，例如

```
NavmeshSetDestination(destination=>myDestination)
```

## 自定义结点名称

DSL的编译依赖于提前生成的TypeDictionary,一个Json文件用于搜索结点名称和实际C#类的反射信息例如Type、Assembly、NameSpace
因此你完全可以通过修改TypeDictionary中的结点名称实现更简洁的脚本编写，例如使用中文结点名称，也许会得到下面这样的结果。

```
Vector3 玩家位置 (0,0,0)
序列 (子节点:[
    获取玩家位置 (位置=>玩家位置),
    移动至玩家(目标=>玩家位置)
])
```


## 限制


1. 结点必须已经存在项目中(因此该方案并非代码热更新)


2. 使用者仍需了解各结点的具体变量类型和名称
