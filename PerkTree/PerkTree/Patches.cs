using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PerkTree
{
    [HarmonyPatch]
    public class Patches
    {
        private static bool IsButtonAdded = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_PhoneScreen), nameof(UI_PhoneScreen.OnOpenScreen))]
        public static void UI_PhoneScreen_OnOpenScreen_Postfix(ref UI_PhoneScreen __instance)
        {
            if (!Plugin.EnableMod.Value) return;

            if (!IsButtonAdded)
            {
                AddPerkButton(ref __instance);
                IsButtonAdded = true;
            }

            return;
        }

        public static void AddPerkButton(ref UI_PhoneScreen phoneScreen)
        {
            Plugin.L("Adding button to phone screen");
            List<Button> buttons = phoneScreen.m_PhoneButtonList;
            GameObject buttonObject = Object.Instantiate(buttons[0].gameObject);
            Vector3 basePosition = buttons[0].transform.parent.localPosition;
            Button perkButton = buttonObject.GetComponent<Button>();
            float xDelta = perkButton.transform.parent.transform.localPosition.x - buttons[1].transform.parent.transform.localPosition.x;
            float yDelta = perkButton.transform.parent.transform.localPosition.y - buttons[3].transform.parent.transform.localPosition.y;
            perkButton.transform.SetParent(buttons[0].transform.parent, false);
            perkButton.transform.parent.transform.localPosition = new Vector3(basePosition.x + xDelta, basePosition.y + yDelta, basePosition.z);

            buttons.Add(perkButton);

            Text buttonText = perkButton.GetComponentInChildren<Text>();

            if (buttonText != null)
            {
                buttonText.text = "Mod Button";
            }

            perkButton.onClick.RemoveAllListeners();
            perkButton.onClick.AddListener(() => OnPerkButtonClick());
            //perkButton.transform.position = buttons.Last().transform.position + new Vector3(0.1f, 0f, 0f);

        }

        public static void OnPerkButtonClick()
        {
            Plugin.L("Mod Button clicked!");
            // Your mod's functionality goes here
        }
    }
}
