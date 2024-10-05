using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DisplayCustomerMoney
{
    public class Patches
    {
        public static void ShowCustomersMoney()
        {
            if (!CSingleton<CGameManager>.Instance.m_IsGameLevel || CSingleton<InteractionPlayerController>.Instance == null || CSingleton<InteractionPlayerController>.Instance.IsInUIMode())
            {
                return;
            }

            List<Customer> customerList = CSingleton<CustomerManager>.Instance.m_CustomerList;
            List<PricePopupUI> pricePopupList = CSingleton<PricePopupSpawner>.Instance.m_PricePopupList;
            PricePopupSpawner popupSpawner = CSingleton<PricePopupSpawner>.Instance;

            for (int i = 0; i < customerList.Count; i++)
            {
                PricePopupUI pricePopup = null;

                // Check if there is any inactive popup available for reuse
                for (int j = 0; j < pricePopupList.Count; j++)
                {
                    if (!pricePopupList[j].gameObject.activeSelf)
                    {
                        pricePopup = pricePopupList[j];
                        break;
                    }
                }

                // If no inactive popup is found, instantiate a new one
                if (pricePopup == null)
                {
                    GameObject newPopupObject = GameObject.Instantiate(pricePopupList[0].gameObject);
                    pricePopup = newPopupObject.GetComponent<PricePopupUI>();
                    pricePopupList.Add(pricePopup);  // Add the new popup to the list
                }

                pricePopup.SetFollowTransform(customerList[i].transform, 2f);
                pricePopup.m_Transform.position = pricePopup.m_FollowTransform.position + Vector3.up * pricePopup.m_OffsetUp;
                pricePopup.m_Transform.LookAt(new Vector3(popupSpawner.m_Cam.position.x, popupSpawner.m_Cam.position.y, popupSpawner.m_Cam.position.z));
                pricePopup.m_Text.text = GameInstance.GetPriceString(customerList[i].m_MaxMoney, false, true, false, "F2");
                if (pricePopup.m_Text.text == null || pricePopup.m_Text.text == "") pricePopup.m_Text.text = customerList[i].m_MaxMoney.ToString();
                pricePopup.m_Text.color = Color.white;
                pricePopup.gameObject.SetActive(true);
            }
        }
    }
}
