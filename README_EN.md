# AkiBTDSL Info

***Read this document in Chinese: [中文文档](./README.md)***

AkBTDSL is a domain-specific language designed for Behavior Tree [AkiBT](https://github.com/AkiKurisu/AkiBT) and supported by [AkiKurisu](https://space.bilibili.com/20472331).You can hot-update the Behavior Tree while the game is running, and users can write scripts without knowing the details of the nodes or the complete project.

## What is DSL

A Domain-Specific Language (DSL) is a computer language that's targeted to a particular kind of problem, rather than a general purpose language that's aimed at any kind of software problem.

[See Reference Article](https://martinfowler.com/dsl.html)

AkiBTDSL solved problem: Modify the behavior tree while the game is running or offline without providing project source code to support safe UGC functions

## Features
* Compile and decompile at any time during runtime and export BehaviorTreeSO in the Editor
* The compiler can be completely separated from the project, and the user does not need to know the details of the special nodes in the project, such as method implementation

## Advantage
Reduces output complexity and is suitable for writing large language models such as ChatGPT. You can embed node scripts into word vectors and let AI generate behavior trees according to your needs.

## Setup
1. Download [Release Package](https://github.com/AkiKurisu/AkiBTDSL/releases)
2. Using git URL to download package by Unity PackageManager ```https://github.com/AkiKurisu/AkiBTDSL.git```


## How To Use

  
<img src="Images/VM.png" />

1. Use AkiBTCompiler (Tools/AkiBT/AkiBT Compiler) to generate a TypeDictionary
2. Create a GameObject and mount the BehaviorTreeVM component
3. Drag textAsset wrote with AkiBTDSL to the inspector
4. Click Compile to compile a behavior tree or using  ```Compile(string vmCode)``` method in BehaviorTreeVM at runtime
5. Click Run to run directly.
6. Click Save to save the compiled behavior tree as BehaviorTreeSO
  
## Theory

Since the serialization of AkiBT depends on the serialization of ```UnityEngine.SerializeReferenceAttribute```, the hot update scheme is to imitate the format of the serialization and deserialize it into an AkiBT behavior tree.

``BehaviorTreeSerializeReferenceData`` is a Json format file serialized using ```UnityEngine.SerializeReferenceAttribute```, which additionally includes the SharedVariables of the AkiBT behavior tree, that is, shared variables, which are also serialized based on the above Attribute. So we can modify the deserialized result by modifying ``BehaviorTreeSerializeReferenceData``. But ``BehaviorTreeSerializeReferenceData`` has a difficulty in manual writing, the following is an example:
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

I intercepted some fragments of ``BehaviorTreeSerializeReferenceData``, and I can see that due to serialization using the SerializeReference method, the storage method is to store the rid at the reference location, and store the actual data in the unified references collection. This is very inconvenient to write manually, so I made a simple compiler to write scripts in a more natural language called AkiBTDSL.

The following is a behavior tree written using AkiBTDSL:
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

The above behavior tree is the patrol AI behavior tree in AkiBT Example, it will get a new position every 10 seconds and move to it, if the distance from the target point is less than 2, it will stop

## How to write

The main body of DSL can be divided into two parts, public variables and nodes. The declaration of public variables needs to specify the type, name and value.

If the type is wrapped with `&`, global variables will be bound at runtime, for example:

```
$Vector3$ TargetPosition (0,0,0)
```

If the variable type is Object (SharedObject), you can declare the type before declaring the value. For example:
```
Object navAgent "UnityEngine.AI.NavMeshAgent, UnityEngine.AIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" Null
```
Then the variable is set to global and has the ``NavMeshAgent`` type restriction.

For nodes, we will skip the Root node (because all behavior trees enter from the Root), and start writing directly from the Root's child nodes.


For a node, you need to declare its type (name index, can be modified by customizing TypeDictionary, see below)


For ordinary variables that do not use the default value of the node, you need to declare its name (or use AkiLabelAttribute to alter field's name) and add ':' to assign


For the shared variable in the node, if you don’t need to refer to the shared variable of the public variable, you can assign it directly, for example

```
TimeWait(waitTime:10)
```

For shared variables that need to be referenced, use the '=>' symbol plus the name of the public variable that needs to be referenced, for example
```
NavmeshSetDestination(destination=>myDestination)
```

## Custom Node Label

The compilation of AkiBTVM relies on the TypeDictionary generated in advance by AkiBTCompiler, a Json file used to search for node names and reflection information of actual C# classes such as Type, Assembly, NameSpace
Therefore, you can achieve more concise scripting by modifying the node names in TypeDictionary.

```
Vector3 玩家位置 (0,0,0)
序列 (子节点:[
    获取玩家位置 (位置=>玩家位置),
    移动至玩家(目标=>玩家位置)
])
```

## Limitation


1. he node must already exist in the project (so this solution is not code hot update)

2. Users still need to know the specific variable type and name of each node
