策略角色扮演游戏启发式智能体工具.... 目前开发用于完成学年毕设，还需进行多线程优化。并将在未来正式开源（虽然没什么用，更多是给自己用的工具，不是什么大不了的东西）
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
