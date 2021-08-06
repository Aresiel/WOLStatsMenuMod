using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;

namespace StatsMenu
{
    [BepInPlugin("dev.aresiel.wol.statsmenu", "Stats Menu", "4.0.0.0")]
    public class StatsMenu : BaseUnityPlugin
    {
        public static bool toggleBool = true;
        public static ConfigEntry<string> toggleKey;

        void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(StatsMenu));
            toggleKey = Config.Bind("Keybindings", "ToggleMenu", "", "The key used to toggle the menu while the inventory is closed. Name of the key, as used by https://docs.unity3d.com/ScriptReference/Input.GetKeyDown.html");
        }

        public static Player.SkillState currentlySelected(Player player, Player.SkillState fallback)
        {
            int playerDigit = Array.FindIndex(GameController.activePlayers, p => p == player);
            LowerHUD hud;
            if (playerDigit == 0) hud = GameUI.P1Hud;
            else hud = GameUI.P1Hud;

            var inventoryMenu = Traverse.Create(hud).Field("inventoryMenu").GetValue() as InventoryMenu;
            var equipMenu = inventoryMenu.equipMenu;
            var currentlySelectedIndex = Traverse.Create(equipMenu).Field("navigationIndex").GetValue<int>();
            Player.SkillState currentSelectedSkill;
            try
            {
                currentSelectedSkill = player.assignedSkills[currentlySelectedIndex] ?? fallback;
            }
            catch (IndexOutOfRangeException)
            {
                currentSelectedSkill = fallback;
            }
            return currentSelectedSkill;
        }

        public static string FetchStatsMessage(Player player)
        {
            StringBuilder output = new StringBuilder();
            var health = player.health;
            string selectedSkill = currentlySelected(player, player.GetBasicSkill()).name;
            string signatureSkill = player.GetSignatureSkill().name;
            int playerDigit = Array.FindIndex(GameController.activePlayers, p => p == player);
            StatData currentSkillStats = StatManager.GetStatData("Skills", "Player" + playerDigit.ToString(), selectedSkill);
            StatData baseCurrentSkillStats = StatManager.GetStatData("Skills", "Player", selectedSkill);
            StatData signatureSkillStats = StatManager.GetStatData("Skills", "Player" + playerDigit.ToString(), signatureSkill);
            StatData baseSignatureSkillStats = StatManager.GetStatData("Skills", "Player", signatureSkill);

            output.AppendLine("[Damage]");
            float damageRatio = currentSkillStats.ApplyVarStatMod<float>("damage", 1000)/1000;
            output.AppendLine("<color=#606060>" + (currentSkillStats.GetValue<int>("damage") == 0 ? "None" : currentSkillStats.GetValue<int>("damage") + " (" + Percentify(damageRatio) + ")") + "</color>");

            output.AppendLine("[Crit Heal CHN]");
            output.AppendLine("<color=#606060>" + Percentify(health.critHealModStat.CurrentValue) + "</color>");

            output.AppendLine("[Heal Amount]");
            output.AppendLine("<color=#606060>" + Percentify(health.healModifierStat.CurrentValue) + "</color>");

            output.AppendLine("[Evade Chance]");
            output.AppendLine("<color=#606060>" + Percentify(health.evadeStat.CurrentValue) + "</color>");

            output.AppendLine("[EFF Armor]");
            output.AppendLine("<color=#606060>" + Percentify(1-((1-health.armorStat.CurrentValue) * health.damageTakenStat.CurrentValue)) + "</color>");

            output.AppendLine("[Shield]");
            output.AppendLine("<color=#606060>" + Math.Round(health.shieldStat.CurrentValue, 2) + "</color>");

            output.AppendLine("[Critical]");
            output.AppendLine("<color=#606060>" + "CHN: " + Percentify(currentSkillStats.GetValue<float>("criticalHitChance")) + "</color>");
            output.AppendLine("<color=#606060>" + "DMG: " + Percentify(currentSkillStats.GetValue<float>("criticalDamageModifier")) + "</color>");

            output.AppendLine("[Cooldown]");
            float cooldownRatio = currentSkillStats.GetValue<float>("cooldown") / baseCurrentSkillStats.GetValue<float>("cooldown");
            output.AppendLine("<color=#606060>" + (currentSkillStats.GetValue<float>("cooldown") == 0 ? "None" : Math.Round(currentSkillStats.GetValue<float>("cooldown")) + "s" + " (" + Percentify(cooldownRatio) + ")") + "</color>");

            output.AppendLine("[Knockback]");
            output.AppendLine("<color=#606060>" + "TKN: " + Percentify(health.knockbackModifierStat.CurrentValue) + "</color>");
            float knockbackGivenRatio = currentSkillStats.GetValue<float>("knockbackMultiplier") / baseCurrentSkillStats.GetValue<float>("knockbackMultiplier");
            output.AppendLine("<color=#606060>" + "GVN: " + (float.IsNaN(knockbackGivenRatio) ? "None" : Math.Round(currentSkillStats.GetValue<float>("knockbackMultiplier"), 2) + " (" +  Percentify(knockbackGivenRatio) + ")" ) + "</color>");

            output.AppendLine("[Speed]");
            output.AppendLine("<color=#606060>" + Percentify(player.movement.moveSpeedStat.CurrentValue/player.movement.moveSpeedStat.BaseValue) + "</color>");

            output.AppendLine("[Stun Duration]");
            output.AppendLine("<color=#606060>" + Percentify(currentSkillStats.GetValue<float>("hitStunDurationModifier")) + "</color>");

            output.AppendLine("[Sig Damage]");
            output.AppendLine("<color=#606060>" + Percentify(signatureSkillStats.GetValue<float>("overdriveDamageMultiplier")) + "</color>");

            output.AppendLine("[Sig Gain]");
            float sigGainRatio = signatureSkillStats.GetValue<float>("overdriveProgressMultiplier") / baseSignatureSkillStats.GetValue<float>("overdriveProgressMultiplier");
            output.AppendLine("<color=#606060>" + (float.IsNaN(sigGainRatio) ? "None" : Percentify(sigGainRatio)) + "</color>");

            output.AppendLine("[Sig Decay]");
            output.AppendLine("<color=#606060>" + "Passive: " + Percentify(player.overdriveBuildDecayRate.CurrentValue/player.overdriveBuildDecayRate.BaseValue) + "</color>");
            output.AppendLine("<color=#606060>" + "Active: " + Percentify(player.overdriveActiveDecayRate.CurrentValue / player.overdriveBuildDecayRate.BaseValue) + "</color>");

            output.AppendLine("[Curr Gain]");
            output.AppendLine("<color=#606060>" + "Gold: " + Percentify(LootManager.globalGoldModifier.CurrentValue) + "</color>");
            output.AppendLine("<color=#606060>" + "Gems: " + Math.Round(LootManager.globalPlatModifier.CurrentValue, 2) + "</color>");

            return output.ToString();
        }

        public static string Percentify(float input)
        {
            return (Math.Round(input * 100, 2)).ToString() + "%";
        }

        [HarmonyPatch(typeof(LowerHUD), "Update")]
        [HarmonyPostfix]
        static void UpdateMenu(LowerHUD __instance)
        {
            GameObject hubObj = Traverse.Create(__instance).Field("hudObj").GetValue() as GameObject;
            Transform hudTrans = hubObj.transform;
            Transform statsMenu = hudTrans.Find("StatsMenu");
            if (statsMenu == null) return;
            if (statsMenu.gameObject.activeInHierarchy)
            {
                Player player = Traverse.Create(hudTrans.Find("CooldownUI").GetComponent<CooldownUI>()).Field("player").GetValue() as Player;
                statsMenu.Find("Content").GetComponent<Text>().text = StatsMenu.FetchStatsMessage(player);
            }
            if (StatsMenu.toggleKey.Value != "" && Input.GetKeyDown(StatsMenu.toggleKey.Value))
            {
                statsMenu.gameObject.SetActive(StatsMenu.toggleBool);
                toggleBool = !toggleBool;
            }
        }

        [HarmonyPatch(typeof(LowerHUD), "SetEquipMenuStatus")]
        [HarmonyPostfix]
        static void ToggleMenu(bool givenStatus, LowerHUD __instance)
        {
            GameObject hubObj = Traverse.Create(__instance).Field("hudObj").GetValue() as GameObject;
            Transform hudTrans = hubObj.transform;
            Transform statsMenuTransform = hudTrans.Find("StatsMenu");
            Player player = Traverse.Create(hudTrans.Find("CooldownUI").GetComponent<CooldownUI>()).Field("player").GetValue() as Player;

            if (statsMenuTransform == null)
            {

                var munroSmall = ChaosBundle.Get<Font>("Assets/Fonts/MunroSmall.ttf");
                var statsMenuBuilder = new GameObject("StatsMenu");

                // Outer Box
                var outerBoxRectTransform = statsMenuBuilder.AddComponent<RectTransform>();
                var outerBoxRenderer = statsMenuBuilder.AddComponent<CanvasRenderer>();

                var outerBoxImage = statsMenuBuilder.AddComponent<Image>();
                outerBoxImage.sprite = IconManager.GetCDBorder(0, true);
                outerBoxImage.color = new Color(0.110f, 0.153f, 0.192f, 0.941f);
                outerBoxImage.type = Image.Type.Sliced;

                outerBoxRectTransform.anchorMin = new Vector2(0.1f, 0);
                outerBoxRectTransform.anchorMax = new Vector2(0.4f, 0.5f);

                int playerDigit = Array.FindIndex(GameController.activePlayers, p => p == player);
                if(playerDigit == 0)
                {
                    outerBoxRectTransform.offsetMin = new Vector2(-45.71f, 10.8f);
                    outerBoxRectTransform.offsetMax = new Vector2(-142.95f, 121.5f);
                } else
                {
                    outerBoxRectTransform.offsetMin = new Vector2(383, 10.8f);
                    outerBoxRectTransform.offsetMax = new Vector2(286, 121.5f);
                }
                

                //Title
                var titleObj = new GameObject("Title");
                titleObj.transform.SetParent(statsMenuBuilder.transform);
                var titleCanvasRenderer = titleObj.AddComponent<CanvasRenderer>();
                var titleRectTransform = titleObj.AddComponent<RectTransform>();
                var titleOutline = titleObj.AddComponent<Outline>();
                var titleText = titleObj.AddComponent<Text>();

                titleText.text = "- Stats -";
                titleText.color = new Color(1, 1, 1, 0.8f);
                titleText.font = munroSmall;
                titleText.fontSize = 10;
                titleText.fontStyle = FontStyle.Normal;

                titleOutline.effectDistance = new Vector2(0.5f, 0.5f);

                titleRectTransform.anchorMin = new Vector2(0, 0);
                titleRectTransform.anchorMax = new Vector2(0, 1);
                titleRectTransform.offsetMin = new Vector2(7, -4);
                titleRectTransform.offsetMax = new Vector2(200, -4);

                // Content
                var contentObj = new GameObject("Content");
                contentObj.transform.SetParent(statsMenuBuilder.transform);
                var contentCanvasRenderer = contentObj.AddComponent<CanvasRenderer>();
                var contentRectTransform = contentObj.AddComponent<RectTransform>();
                var contentText = contentObj.AddComponent<Text>();

                contentText.font = munroSmall;
                contentText.fontSize = 6;
                contentText.fontStyle = FontStyle.Normal;
                contentText.color = new Color(0.7176471f, 0.7529412f, 0.7647059f, 1);
                contentText.supportRichText = true;

                contentRectTransform.anchorMin = new Vector2(0.5f, 0);
                contentRectTransform.anchorMax = new Vector2(0.5f, 1);
                contentRectTransform.offsetMin = new Vector2(-19, 0);
                contentRectTransform.offsetMax = new Vector2(23, -14);

                //Instantiating
                statsMenuTransform = Instantiate(statsMenuBuilder, hudTrans).transform;
                statsMenuTransform.name = statsMenuBuilder.name;
            }

            if (givenStatus)
            {
                statsMenuTransform.Find("Content").GetComponent<Text>().text = StatsMenu.FetchStatsMessage(player);
                statsMenuTransform.gameObject.SetActive(true);
                toggleBool = false;

            } else
            {
                statsMenuTransform.gameObject.SetActive(false);
                toggleBool = true;
            }
        }
    }
}
