策略角色扮演游戏启发式智能体工具.... 目前开发用于完成学年毕设，还需进行多线程优化。并将在未来正式开源（虽然没什么用，更多是给自己用的工具，不是什么大不了的东西，对找游戏工作有点摆烂了，就做做）
Tactic RPG -TRPG/SRPG Heuristic Agent Tools.... Currently under development for completion of the final-year project, requiring further multithreading optimisation. 
Will be formally open-sourced in the future (though it's not particularly useful, more like the toolkit for myself).

该工具旨在实现基于网格战术行动的智能评分代理——其行为表现基于项目游戏机制设计，但更贴近三款经典游戏：《最终幻想战略版》《皇家骑士团重生》及《三角战略》。
This tool are aim to focus on implementing a scoring intelligent agent for grid-based tactics action - the agent behavior performance is grounded in the project's game mechanics design, 
yet draws closer to three classic titles: Final Fantasy Tactics, Orge Reborn, and Triangle Strategy.

此项目被实现在3维网格数据上，但地图在设计上仍需完善，目前只是支持int 数值的y轴高度，所有其余在设计上也是如此。同时值得注意的是地图的数据需要使用本项目内置的简陋地图编辑器配合tile工具进行配置同时生成地图数据文件。
This project are implement in the three dimensional grid system, while the map design requires further refinement, as it currently only supports integer values for the y-axis height, 
and all design aspects follow this constraint. It is also worth noting that map data must be configured using the project's rudimentary built-in map editor in conjunction with the 
tile tool to generate the map data files.

<img width="497" height="417" alt="image" src="https://github.com/user-attachments/assets/e937aaee-1af9-4e8b-8380-c3d6b64286ee" />

该项目利用了通过定制地图编辑工具生成的预制JSON地图数据，用于加载特定地图并释放多余的地图。
The project utilized the prefabricated JSON map data that was crafted through a customized map editor tool to load the specific map also release unnecessary maps.

# Game Mechanics (游戏机制)

## Character Tactic Timeline (角色战术时间轴）
这是一种独立设计的游戏机制，用于管理和确定角色行动的回合顺序。
It is an independently designed game mechanic used to manage and determine the turn sequence of character actions. 

<img width="940" height="900" alt="image" src="https://github.com/user-attachments/assets/c0a4de30-c9fa-40c3-b66f-44d1525c027b" />

上述图像为CT时间线的逻辑流程图。相较于常规速度决定时间线的机制，该机制引入了新要素：CT疲劳惩罚。角色行动顺序的排序完全取决于角色速度，直至所有角色行动序列分配完毕。因此，CT时间线的设计允许角色在每次排序中拥有多个行动序列。最终结果以CT回合呈现，每个回合包含多个角色的行动轮次。
The above-mentioned image is a logic flow chart of the CT Timeline. Compared to the general speed-determined timeline mechanics, this mechanic introduces a new element: CT fatigue penalties. The character sequence sorting is determined by the character’s speed until all character sequences have been assigned. Thus, the design of the CT Timeline allows a character to have multiple action sequences in each CT Timeline sorting. The results are represented as CT rounds, with each round including multiple character action turns.

<img width="940" height="814" alt="image" src="https://github.com/user-attachments/assets/f58dd90e-f93a-4420-b5f9-027b8de94699" />

除CT时间轴排序角色行动回合的基本原理外，整体时间轴机制还会协同UI系统生成并管理当前行动回合。在获取参战角色后，CT时间轴将生成并预测未来数轮的角色行动回合。若角色脱离战斗或速度发生变化，当前及未来的行动序列将与UI系统同步调整。同时，CT时间轴作为持续扩展的机制，将贯穿整个战斗直至结束。
In addition to the CT Timeline principle of sorting character action turns, the overall timeline mechanism also works together with the UI to generate and manage the current action turns. After acquiring the participating characters, the CT Timeline will generate and predict several upcoming rounds of character action turns. If a character leaves the battle or their speed changes, the current and future action sequences, along with the UI, will be adjusted accordingly. At the same time, the CT Timeline is an extended mechanism that continues to expand until the battle ends.

主要表现计算流程如下：
The main calculation process in below figures:
<img width="570" height="712" alt="image" src="https://github.com/user-attachments/assets/3edcb173-d05c-43f4-93c0-36c4d550092f" />

# Skill Mechanics (技能机制）
为确保战术多样性，技能机制的设计很大程度上决定了战术AI。
To ensure tactical diversity, the design of skill mechanics largely determines an agent's tactical approach.

## Skill Defination (技能定义)
### Projectile and non-projectile (投射物与非投射物）
投射物：技能可能被沿其飞行路径布置的地形特征或单位阻挡。这包括墙壁、地形起伏，以及根据技能碰撞逻辑判定为友军或敌军的单位。Projectile: Skills can be blocked by terrain features or units positioned along their path. This includes walls, elevation changes, and both allied and enemy units, depending on the skill's collision logic.
非投射类：技能直接施加效果于目标，无需模拟飞行或碰撞。Non-projectile: Skills apply effects directly to targets without simulating travel or collision.

### Skill range （技能范围）
技能范围与战术瞄准镜机制相关联。在定义技能范围和遮挡范围时，实际范围将体现战术范围的特性：既从角色原点延伸直到可及范围，又从角色原点开始执行遮挡计算。通过这种方式实现技能范围的限制。
The skill range is associated with the tactics scope mechanics. When defining skill range and obstruction range, the actual scope will reflect the characteristics of the tactical scope, extending the scope from the character's origin point while performing occlusion calculations starting from the character's origin point. Utilize this approach to limit the skill range.

### Skill Conditions (技能条件）
技能条件代表施放技能的要求。在技能设计中，该条件被设定为消耗MP点数，但根据游戏设计，这并非唯一可接受的条件。
Skill conditions represent the requirements for casting skills. In skill design, this condition is set as consuming MP points, but according to game design, this is not the only acceptable condition.

