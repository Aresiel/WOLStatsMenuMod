Wizard of Legends StatsMenu Mod
===
A mod that adds a menu for your stats to Wizard of Legends. The menu opens by default alongside your inventory.

## Configuring
You can configure a keybind to open the menu independently from the inventory. Do this via the config file located at `GAME_DIR/BepInEx/config/dev.aresiel.wol.statsmenu.cfg`, you need to have ran the game at least once with the mod for this file to exist. The `ToggleMenu` value needs to be a key recognized by [`UnityEngine.Input.GetKeyDown`](https://docs.unity3d.com/ScriptReference/Input.GetKeyDown.html).
Example config that binds the `home` key:
```
[Keybindings]

## The key used to toggle the menu while the inventory is closed. Name of the key, as used by https://docs.unity3d.com/ScriptReference/Input.GetKeyDown.html
# Setting type: String
# Default value: 
ToggleMenu = home
```

## Description of stats
|Stat|Description|
--- | ---
|**[CRIT HEAL CHN]**|Chance of critical heal|
|**[HEAL AMOUNT]**|Amount you're healed, displayed as percentage of base value|
|**[EVADE CHANCE]**|Chance you'll evade an attack|
|**[EFF ARMOR]**|Effective armor, the armor stat with the damage taken modifier calculated in. `(1-(1-ArmorStat)/DamageModifier)`|
|**[SHIELD]**|Amount of shield you have|
|**[CRITICAL]**| CHN: Chance of critical hit, DMG: critical damage|
|**[COOLDOWN]**|Cooldown displayed in seconds and as percentage of base value|
|**[KNOCKBACK]**|TKN: How much knockback you take, displayed as percentage of base value, GVN: How much knockback you give, displayed as amount and percentage of base value|
|**[SPEED]**|Your movement speed, displayed as percentage of base value|
|**[STUN DURATION]**| How long you stun an opponent, displayed as percentage of base value|
|**[SIG DAMAGE]**|How much damage your signature deals, displayed as percentage of base value|
|**[SIG GAIN]**|How much signature charge you gain by attacking, displayed as percentage of base value|
|**[SIG DECAY]**|Passive: How much signature charge you lose while the signature isn't charged, Active: How much signature charge you lose while the signature is charged, both displayed as percentage of passive's base value|
|**[CURR GAIN]**|Gold: How much gold you gain, displayed as percentage of base value, Gems: How much extra gems you get, displayed as amount.|
