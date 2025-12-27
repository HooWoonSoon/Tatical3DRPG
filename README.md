# Unity Tactic RPG Heuristic Artificial Intelligence Agent (Unity引擎战术启发式角色智能体）

Notice: Art asset is copyright from SE

Unity version: 6000.0.30f1

The editor extension employs IMGUI rather than the UI Toolkit. (Everyone better use UI toolkit in your project)

<img width="1492" height="788" alt="image" src="https://github.com/user-attachments/assets/41498334-798a-42e1-827f-48f234b7e545" />


Tactic RPG -TRPG/SRPG Heuristic Agent Tools.... Currently under development for completion of the final-year project, requiring further multithreading optimisation. 
Will be formally open-sourced in the future (though it's not particularly useful, more like the toolkit for myself).
策略角色扮演游戏启发式智能体工具.... 目前开发用于完成学年毕设，还需进行多线程优化。并将在未来正式开源（虽然没什么用，更多是给自己用的工具，不是什么大不了的东西）

This tool are aim to focus on implementing a scoring intelligent agent for grid-based tactics action - the agent behavior performance is grounded in the project's game mechanics design, 
yet draws closer to three classic titles: Final Fantasy Tactics, Orge Reborn, and Triangle Strategy.
该工具旨在实现基于网格战术行动的智能评分代理——其行为表现基于项目游戏机制设计，但更贴近三款经典游戏：《最终幻想战略版》《皇家骑士团重生》及《三角战略》。

This project are implement in the three dimensional grid system, while the map design requires further refinement, as it currently only supports integer values for the y-axis height, 
and all design aspects follow this constraint. It is also worth noting that map data must be configured using the project's rudimentary built-in map editor in conjunction with the 
tile tool to generate the map data files.
此项目被实现在3维网格数据上，但地图在设计上仍需完善，目前只是支持int 数值的y轴高度，所有其余在设计上也是如此。同时值得注意的是地图的数据需要使用本项目内置的简陋地图编辑器配合tile工具进行配置同时生成地图数据文件。

## Tactics Map Editor - Not fully Encapsulate (战术地图编辑器 - 不完全封装）

<img width="500" height="731" alt="image" src="https://github.com/user-attachments/assets/8fba9c06-d0e4-4896-ac71-ab76189dc841" />

The project utilized the prefabricated JSON map data that was crafted through a customized map editor tool to load the specific map also release unnecessary maps.
该项目利用了通过定制地图编辑工具生成的预制JSON地图数据，用于加载特定地图并释放多余的地图。

<img width="497" height="417" alt="image" src="https://github.com/user-attachments/assets/e937aaee-1af9-4e8b-8380-c3d6b64286ee" />

# Game Mechanics (游戏机制)

## Character Tactic Timeline (角色战术时间轴）
It is an independently designed game mechanic used to manage and determine the turn sequence of character actions. 
这是一种独立设计的游戏机制，用于管理和确定角色行动的回合顺序。

<img width="940" height="900" alt="image" src="https://github.com/user-attachments/assets/c0a4de30-c9fa-40c3-b66f-44d1525c027b" />

The above-mentioned image is a logic flow chart of the CT Timeline. Compared to the general speed-determined timeline mechanics, this mechanic introduces a new element: CT fatigue penalties. The character sequence sorting is determined by the character’s speed until all character sequences have been assigned. Thus, the design of the CT Timeline allows a character to have multiple action sequences in each CT Timeline sorting. The results are represented as CT rounds, with each round including multiple character action turns.
上述图像为CT时间线的逻辑流程图。相较于常规速度决定时间线的机制，该机制引入了新要素：CT疲劳惩罚。角色行动顺序的排序完全取决于角色速度，直至所有角色行动序列分配完毕。因此，CT时间线的设计允许角色在每次排序中拥有多个行动序列。最终结果以CT回合呈现，每个回合包含多个角色的行动轮次。

<img width="940" height="814" alt="image" src="https://github.com/user-attachments/assets/f58dd90e-f93a-4420-b5f9-027b8de94699" />

In addition to the CT Timeline principle of sorting character action turns, the overall timeline mechanism also works together with the UI to generate and manage the current action turns. After acquiring the participating characters, the CT Timeline will generate and predict several upcoming rounds of character action turns. If a character leaves the battle or their speed changes, the current and future action sequences, along with the UI, will be adjusted accordingly. At the same time, the CT Timeline is an extended mechanism that continues to expand until the battle ends.
除CT时间轴排序角色行动回合的基本原理外，整体时间轴机制还会协同UI系统生成并管理当前行动回合。在获取参战角色后，CT时间轴将生成并预测未来数轮的角色行动回合。若角色脱离战斗或速度发生变化，当前及未来的行动序列将与UI系统同步调整。同时，CT时间轴作为持续扩展的机制，将贯穿整个战斗直至结束。

主要表现计算流程如下：
The main calculation process in below figures:

<img width="570" height="712" alt="image" src="https://github.com/user-attachments/assets/3edcb173-d05c-43f4-93c0-36c4d550092f" />

# Skill Mechanics (技能机制）
To ensure tactical diversity, the design of skill mechanics largely determines an agent's tactical approach.
为确保战术多样性，技能机制的设计很大程度上决定了战术AI。

## Skill Defination (技能定义)
### Projectile and non-projectile (投射物与非投射物）
Projectile: Skills can be blocked by terrain features or units positioned along their path. This includes walls, elevation changes, and both allied and enemy units, depending on the skill's collision logic. 投射物：技能可能被沿其飞行路径布置的地形特征或单位阻挡。这包括墙壁、地形起伏，以及根据技能碰撞逻辑判定为友军或敌军的单位。

Non-projectile: Skills apply effects directly to targets without simulating travel or collision. 非投射类：技能直接施加效果于目标，无需模拟飞行或碰撞。

## Skill range （技能范围）
The skill range is associated with the tactics scope mechanics. When defining skill range and obstruction range, the actual scope will reflect the characteristics of the tactical scope, extending the scope from the character's origin point while performing occlusion calculations starting from the character's origin point. Utilize this approach to limit the skill range.
技能范围与战术瞄准镜机制相关联。在定义技能范围和遮挡范围时，实际范围将体现战术范围的特性：既从角色原点延伸直到可及范围，又从角色原点开始执行遮挡计算。通过这种方式实现技能范围的限制。

## Skill Conditions (技能条件）
Skill conditions represent the requirements for casting skills. In skill design, this condition is set as consuming MP points, but according to game design, this is not the only acceptable condition.
技能条件代表施放技能的要求。在技能设计中，该条件被设定为消耗MP点数，但根据游戏设计，这并非唯一可接受的条件。

# TRPG Agent (战术RPG代理）
The TRPG Agent design principles adhere to decentralized, turn-based tactics, maintaining strong alignment with the project's default combat mechanics. This system will be tightly integrated with map spaces, teams, characters, skills, and combat systems. The TRPG agent is designed as an intelligent agent capable of adapting to spatial, character, skill, and team dynamics, rather than being simply rule-based.
TRPG智能代理的设计原则遵循去中心化、回合制战术，与项目默认的战斗机制高度契合。该系统将与地图空间、团队、角色、技能及战斗系统深度集成。TRPG智能代理被设计为具备空间感知、角色识别、技能判断及团队协作能力的智能体，而非单纯基于规则运作。

## Structure（结构）
The design of reliable intelligent agents and scoring mechanisms is inherently complex and prone to errors, particularly when involving extensive “if-else” conditional statements and loop structures. Spaghetti code frequently emerges during the development of scoring agents. Consequently, this implementation employs interface design and specific methodologies to circumvent these issue.
可靠智能代理与评分机制的设计本质上复杂且易出错，尤其涉及大量“if-else”条件语句和循环结构时。评分代理开发过程中常出现意面代码。因此本实现采用接口设计与特定方法论来规避该问题。

<img width="495" height="469" alt="image" src="https://github.com/user-attachments/assets/837cb2bf-eb8f-4907-9412-7526a4a72b2d" />

## Score Evaluation (评分评估)
The agent's decision-making is governed by distinct evaluations, with certain evaluations being suppressed based on the agent's current circumstances. In the current agent assessment, three distinct evaluation principles can be distinguished, which include the assessment of purely cast skill actions. Move cast skill action or purely a movement action. Except for the above three evaluations, the orientation evaluation will certainly be executed last.
智能体的决策受不同评估机制的约束，其中某些评估会根据智能体当前状态被抑制。在当前智能体评估中，可区分出三种独立的评估原则，包括纯技能施放动作的评估、移动技能施放动作或纯移动动作的评估。除上述三种评估外，定向评估必定在最后执行。

<img width="544" height="834" alt="image" src="https://github.com/user-attachments/assets/e76d3a85-fe88-4a10-9616-e0c8f4838537" />

## Rules (规则）
Rules are the subclass composition of evaluations, and the most crucial and logical representation. The rules are also embedded as a sub-item within other rules, although these rules do not strictly prohibit the sum of sub-items and parent items from exceeding the parent's total score. However, the current formulation of all rules constrains the sub-category rule's score to the parent category's total score.
规则是评估项的子类组合，也是最关键且最符合逻辑的呈现形式。规则也可嵌套为其他规则的子项，尽管这些规则并不严格禁止子项与父项之和超过父项的总分。然而，当前所有规则的制定都限制了子类规则的得分不得超过父类别的总分。

### Target Rule (目标规则)
The target rule would be based on the agent evaluating which unit is most appropriate to the closest. The target is always changed based on the decision-maker current frontline index and target value. The target could be the opposites and teammates.
目标规则基于智能体评估哪个单位最适合最近距离作战。目标始终根据决策者当前的前线指数和目标值进行调整。目标可能包括敌方单位和队友单位。

### Move Target Rule (移动目标规则)
The move target rule aims to find the most suitable, shortest, and safest route to the vicinity of the current target character. Based on the character frontline index, if the index is higher, the decision-maker is more low sensitivity for the danger path.
移动目标规则旨在寻找通往当前目标角色周边区域最合适、最短且最安全的路线。根据角色前线指数，若指数越高，决策者对危险路径的敏感度越低。

### Skill Harm Rule (技能伤害规则)
This rule is used to calculate the most appropriate skill ability in the current circumstances. This rule employs a calculation method that utilises the formula for the lowest consumption and highest yield.
本规则用于计算当前情境下最适宜的技能能力。该规则采用最低消耗与最高产出公式的计算方法。

### Skill Treat Rule (技能治疗规则)
此规则是技能评估的另一种变体，专门适用于治疗技能。该划分背后的核心原则是赋予智能体更直观且易于调试的行为倾向。例如，智能体可能表现出更倾向于采取攻击性行动，而非使用治疗技能。
This rule is another variant of skill assessment that applies specifically to treat skills. The primary principle behind this division is to grant the agent more intuitive and debug-friendly behavioural tendencies. For instance, the agent may exhibit a preference for offensive actions over employing treat skills.

Risk Move Rule (风险移动规则）
A method for determining the safest positioning rules, which evaluates the positions of teammates and enemies to ensure unit safety while preventing units from straying too far from the group.
一种确定最安全站位规则的方法，通过评估队友与敌人的位置来保障单位安全，同时防止单位偏离团队过远。

•	Risk Move Harm Rule (风险移动伤害规则）
A variant of the risk move rule, primarily integrating the skill selection from the harm rule, while ensuring the safety of the chosen movement location.
风险移动规则的变体，主要整合伤害规则的技能选择机制，同时确保所选移动位置的安全性。

•	Risk Move Treat Rule (风险移动治疗规则）
A variant of the risk move rule, primarily integrating the skill selection from the treat rule, while ensuring the safety of the chosen movement location.
风险移动规则的变体，主要整合治疗规则的技能选择机制，同时确保所选移动位置的安全性。

•	Fatal Hit Rule (致命打击规则）
This rule typically functions as a subclass alongside the skill harm rule, merely treating the ability to eliminate targets as an additional scoring component.
该规则通常作为伤害规则的子类运作，仅将消灭目标的能力视为额外计分要素。

•	Defence Back Rule (背部防御规则）
Adjust the orientation appropriately based on the terrain and character's range.
参考地形与角色的范围调整合适的朝向。

# Sundry (杂项）
## Tactics Character Editor (战术角色编辑器)
The Tactical Role Editor is a tool combining character data definition generation and character AI debugging. This project enables the inspection of AI behaviour scores, thereby facilitating expansion and debugging.
战术角色编辑器是组合了角色数据定义生成与AI调试的工具，此项目允许查看AI行为的积分，从而帮助进行扩展与调试

<img width="1002" height="955" alt="image" src="https://github.com/user-attachments/assets/b392616b-8b63-4dbd-900d-e4f1a34b1dd6" />
<img width="1000" height="962" alt="image" src="https://github.com/user-attachments/assets/f9fc7447-8bbd-4f1f-a14d-5b7a20f65afb" />
<img width="1001" height="952" alt="image" src="https://github.com/user-attachments/assets/f2e4e4c6-2dbd-4090-9086-f05e8ecddef1" />

## Skill Editor (技能编辑器)
The Skill Editor facilitates intuitive configuration of skills and traits, currently supporting projectiles and non-projectile abilities, though without visual effects.
技能编辑器可帮助直观的进行技能与特性的配置，目前支持投射物与非投射物，但无特效。

<img width="1000" height="922" alt="image" src="https://github.com/user-attachments/assets/cd512539-7ece-4b7a-9062-b11ad5806881" />

# Screenshot

<img width="940" height="438" alt="image" src="https://github.com/user-attachments/assets/11fcaac4-d9e3-4552-b191-84e1972df70a" />
<img width="940" height="528" alt="image" src="https://github.com/user-attachments/assets/0f1ff1bb-0144-4b39-8ab1-95f8a9e38312" />
<img width="940" height="496" alt="image" src="https://github.com/user-attachments/assets/935de1ac-b0ed-4fcb-94d1-a43587a91b19" />
<img width="940" height="527" alt="image" src="https://github.com/user-attachments/assets/b58ec9b2-6c6e-4439-8934-ba77f30004e9" />

无视下方垃圾话

想证明自己，结果就想多了，谁在乎呢。生活中，不认识者的批评和他人的轻蔑，侮辱倒是收到很多。不属于任何群体，也没资格归属任何群体。无视无关者，至少如今只需要提防骗子就好了。
