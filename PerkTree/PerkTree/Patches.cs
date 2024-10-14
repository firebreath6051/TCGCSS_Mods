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
                AddPerkButton(__instance);
                IsButtonAdded = true;
            }

            return;
        }

        public static void AddPerkButton(UI_PhoneScreen phoneScreen)
        {
            Plugin.L("Adding Perk Button to Phone Screen");

            // Step 1: Access the list of existing buttons
            List<Button> buttons = phoneScreen.m_PhoneButtonList;

            if (buttons == null || buttons.Count == 0)
            {
                Plugin.L("No buttons found in phoneScreen.m_PhoneButtonList.");
                return;
            }

            // Get the parent of the first button
            Transform buttonsParent = buttons[0].transform.parent;

            if (buttonsParent == null)
            {
                Plugin.L("Buttons parent container not found.");
                return;
            }

            // Step 2: Instantiate a new button using the first button as a template
            Button templateButton = buttons[0];

            // Clone the template button
            GameObject newButtonObject = Object.Instantiate(templateButton.gameObject, buttonsParent);
            newButtonObject.name = "PerkButton"; // Rename for clarity
            newButtonObject.SetActive(true); // Ensure the button is active

            // Step 3: Get the Button component
            Button newButton = newButtonObject.GetComponent<Button>();
            newButton.enabled = true;

            // Step 4: Configure the RectTransform
            RectTransform newButtonRect = newButtonObject.GetComponent<RectTransform>();
            RectTransform templateRect = templateButton.GetComponent<RectTransform>();

            // Match the size and anchors of the template
            newButtonRect.sizeDelta = templateRect.sizeDelta;
            newButtonRect.anchorMin = templateRect.anchorMin;
            newButtonRect.anchorMax = templateRect.anchorMax;
            newButtonRect.pivot = templateRect.pivot;

            // Position the new button appropriately
            // For example, position it below the last existing button
            RectTransform lastButtonRect = buttons[buttons.Count - 1].GetComponent<RectTransform>();
            newButtonRect.anchoredPosition = lastButtonRect.anchoredPosition - new Vector2(0, lastButtonRect.rect.height + 10); // Adjust spacing as needed

            // Step 5: Set up the button's visuals
            Text buttonText = newButtonObject.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "Mod Button";
            }
            else
            {
                // If using TextMeshProUGUI
                var tmpText = newButtonObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = "Mod Button";
                }
                else
                {
                    Plugin.L("Text component not found on the new button.");
                }
            }

            // Step 6: Add OnClick listener
            newButton.onClick.RemoveAllListeners(); // Remove existing listeners
            newButton.onClick.AddListener(OnPerkButtonClick); // Add your method

            // Step 7: Update Layout Groups (if applicable)
            LayoutGroup layoutGroup = buttonsParent.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsParent as RectTransform);
            }

            // Step 8: Add the new button to the phone screen's button list
            buttons.Add(newButton);

            // Log the successful addition
            Plugin.L("Perk Button added successfully.");
        }

        // Your method to handle button clicks
        private static void OnPerkButtonClick()
        {
            // Implement your functionality here
            Plugin.L("Perk Button clicked!");
        }
    }
}
