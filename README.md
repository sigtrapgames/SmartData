# SmartData for Unity
> A designer-friendly, editor-driven framework for connecting data and events.

Need a flexible way to connect everything in your game without singletons, hard-coding or overly-complex architecture? *SmartData* might be what you're looking for. It is based on Ryan Hipple's ([twitter](https://twitter.com/roboryantron) | [github](https://github.com/roboryantron)) amazing talk from Unite Austin 2017, which you are ***strongly*** recommended to watch before proceeding.

[![Game Architecture with Scriptable Objects](http://img.youtube.com/vi/raQ3iHhE_Kk/0.jpg)](http://www.youtube.com/watch?v=raQ3iHhE_Kk "Game Architecture with Scriptable Objects")

### INSTALLATION
* Grab *SmartData.unitypackage* from the [Releases page](https://github.com/sigtrapgames/SmartData/releases) for everything you need!
* To use the *git* repository, you'll need our events implementation, *Relay*<sup>[1]</sup>. [github.com/SixWays/Relay](https://github.com/SixWays/Relay)
* Requires Unity 2018.2 or above.

# What is SmartData?
Firstly, ***SmartData*** **is in beta**. With that out of the way, *SmartData* is a framework that uses `ScriptableObject`s to pass data around a game at the global level. Let's take a common example - showing the player's HP on the HUD. Let's assume the Player and the HUD are prefabs, dynamically instantiated at runtime, rather than placing them in each scene at edit-time and manually dragging references.

Note that the following pros and cons are entirely subjective. And there are many more methods we could use!

### Traditional method A - Singletons
Our `Player` class is a singleton. We instantiate the Player prefab, then the HUD prefab, and the HUD uses `Player.instance.hp`.

***Pros***: *Easy to code.*  
***Cons***: *Encourages spaghetti code. Rigid coupling between classes. HUD depends on the Player instance existing. Not exposed to designers.*

-----

### Traditional method B - Find reference at runtime
We don't like singletons, so instead the HUD searches for the Player using `GameObject.Find` in `Awake()`.

***Pros***: *Coupling is slightly less rigid.*  
***Cons***: *Player must be called the right thing. HUD depends on the Player instance existing. Not exposed to designers.*

-----

### SmartData method
We create a `FloatVar` asset called *PlayerHp*. In `Player` we declare a `FloatWriter` and in `HUD` a `FloatReader`.

<img src="https://i.imgur.com/7Pu4ueH.png" width="540" />
<img src="https://i.imgur.com/nzMGlvM.png" width="340" />

In-editor, each script now has a field in which we can drag-and-drop our *PlayerHp* asset. Now they both refer to the same data.

![](https://i.imgur.com/vD93Hr3.png)

`HUD` now has three ways to get the player's health:
* Get `playerHp.value` in an update loop - the *PlayerHp* asset value will stay up-to-date with `Player`
* Use the exposed `UnityEvent` in the editor and check *Auto Listen* - the event will fire when `Player` updates the value
* Use `playerHp.BindListener()` to register for the event in code when `Player` updates the value

***Pros***: *No code coupling. HUD can exist without Player (PlayerHp asset always exists). Exposed to designers.*  
***Cons***: *More complex.*

-----

While *SmartData* takes a little more setup, it's far more flexible, powerful, safe and designer-friendly - and after a couple of uses, that setup is easy.

Now imagine a real codebase. You have dozens of singletons, your code is spaghetti, you need a coder every time a designer wants to get any of that data, and unless you instantiate everything in your scene - *in the right order!* - your game explodes, making low-level testing and iteration hellish.

With *SmartData*, global data is just a set of assets. Designers can access it at will with a set of Components, modify its behaviour and select at edit-time what references point to what data. Coders still retain control of their code, specifying read-only or read/write access from a given reference, with attributes to control what options a designer sees in the editor.

*SmartData* supports any underlying data type via its code generator GUI. It comes with a selection of basic *SmartTypes* pre-built such as `float` and `string` amongst others.

![](https://pbs.twimg.com/media/DmQhR6lW4AAwtNx.jpg:small)

Note that any single *SmartType* only has **one** underlying data type. The multi-type editor above is simply for generating multiple *SmartTypes* at once for convenience.

### SmartEvents
`EventVars`, accessible via `EventListener` and `EventDispatcher` references, simply raise events without any data payload. These are particularly useful for game-level events, such as starting, pausing the game, a player dying etc.

### SmartComponents
Designers can use components which give code-free access to SmartObjects. Read/Listen components give a list of SmartRefs with UnityEvents. Write/Dispatch components give one SmartRef and the ability to set values and/or dispatch events from UnityEvents or other third-party design tools.

![Imgur](https://i.imgur.com/ud1BZgM.png)

### Debugging
*SmartData* includes a visual debugger graph which shows all the current connections between `SmartRef`s (`FloatReader`, `FloatWriter` etc) and `SmartObject`s (`FloatVar` etc)<sup>[2]</sup>. At runtime, it will also animate to show updates and events.

`SmartObject`s will also show a list of connections in the inspector, and at runtime a list of objects listening to events from code.

<img src="https://thumbs.gfycat.com/JoyousAnyAfricangroundhornbill-size_restricted.gif" width="550" />
<img src="https://i.imgur.com/WOMHGth.jpg" width="310" />

**NOTE**: Plain references to *SmartObjects* will not show up in the node graph. You must use *SmartRefs* - e.g. `FloatReader`, `FloatWriter`.

### Advanced Features
The above is just scratching the surface. *SmartData* has many other features which will gradually be documented in [the wiki](https://github.com/sigtrapgames/SmartData/wiki).
* *SmartSet* - List of data with OnAdd and OnRemove events
  * e.g. a list of all AIs - AIs add/remove themselves and the AI manager (and anything else) listens.
* *SmartMulti* - List of SmartVars created on demand
  * e.g. for local multiplayer games - PlayerHp[0-3]
* Decorators - Like components for *SmartObject* assets, these modify behaviour
  * e.g. for clamped/ranged values or even automatic network replication
* Code generation templates
  * Add your own templates to the Generate Types editor to generate custom code for each type.

# What is SmartData *not*?
* A visual scripting language / editor
* A coding-free solution
* Particularly good at runtime-instantiated *SmartObjects* - **yet** (see *What's Next*)
* The answer to all your problems<sup>[3]</sup>

# What's next?
The first focus will be more documentation and examples, since there's a lot here.

The next major planned feature is *SmartGroups* - a comprehensive solution for dynamically instantiating *SmartObjects*. Currently *SmartData* is all about edit-time data - make assets, link them by hand, profit. At runtime, you can create *SmartObjects* *in code*, and link to them *in code*, and destroy them *in code*, and unlink from them *in code*. You may have noticed a pattern.

For example, if you want an *OnHit* `EventVar` for every AI spawned, there's no easy edit-time way to specify what *will* be interested in these events, or how they will be indexed. *SmartGroups* aim to fix that. More later.

Note that *SmartMultis* are not intended to supply this functionality - they're a convenience for cases when there will be multiple long-lived instances of something. For example, with local multiplayer, your HUD elements can all reference a single "PlayerHp" `FloatMulti` asset by index. There's no good way to destroy one of the underlying `FloatVar`s and no way to automatically stop listening to it.

# Notes
* Namespaces are generated for each underlying type. For example, `SmartData.SmartFloat.FloatReader`.
* As mentioned, you'll need `Relay` in your project to use `SmartData`.
* If you use Odin Inspector, you must disable it for the SmartData namespace.
* Userspace code (generated classes etc) is nice and neat. The underlying hierarchy though is crazy complex, largely due to Unity's serialization quirks. It looks horrible, but it's all there for a reason. Sorry in advance.

# Credits
Developed and maintained by Luke Thompson ([@six_ways](https://twitter.com/six_ways) | [github](https://github.com/SixWays))  
SmartGraph developed by Luca Mefisto ([@LucaMefisto](https://twitter.com/LucaMefisto) | [github](https://github.com/MephestoKhaan)) based on his [UnityEventVisualizer](https://github.com/MephestoKhaan/UnityEventVisualizer)

## Acknowledgements
*SmartData*, as previously mentioned, owes its existence and inspiration to Ryan Hipple (see above)  
Development assistance, feedback and ideas provided by Eric Provencher ([@prvncher](https://twitter.com/prvncher) | [github](https://github.com/provencher)) and Dustin Chertoff ([@FearlessVR](https://twitter.com/fearlessvr) | [github](https://github.com/delphinius81))


### Footnotes
[1] *Relay* is not linked as a submodule for three main reasons. Firstly, you're fairly likely to be using *SmartData* as a submodule itself, and sub-submodules tend to confuse lots of *git* clients. Secondly, if you're already using *Relay* (you should be, but we're biased) it could get messy. Finally, *Relay* is pretty stable now so you should be able to grab a zip and forget about it.  
[2] The graph is not editable!  
[3] but only because nothing ever can be