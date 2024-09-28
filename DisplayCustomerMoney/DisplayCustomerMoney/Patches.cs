using HarmonyLib;
using UnityEngine;

namespace DisplayCustomerMoney
{
    [HarmonyPatch]
    public static class Patches
    {
        public static void ShowCustomersMoney()
        {
            if (!Plugin.EnableMod.Value) return;
            if (CSingleton<InteractionPlayerController>.Instance.IsInUIMode())
            {
                return;
            }
            for (int i = 0; i < CSingleton<CustomerManager>.Instance.m_CustomerList.Count; i++)
            {
                if (CSingleton<PricePopupSpawner>.Instance.m_PricePopupList.Count < CSingleton<CustomerManager>.Instance.m_CustomerList.Count)
                {
                    for (int j = 0; j < (CSingleton<CustomerManager>.Instance.m_CustomerList.Count - CSingleton<PricePopupSpawner>.Instance.m_PricePopupList.Count); j++)
                    {
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList.Add(new PricePopupUI());
                    }
                }
                for (int j = 0; j < CSingleton<PricePopupSpawner>.Instance.m_PricePopupList.Count; j++)
                {
                    if (!CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].gameObject.activeSelf)
                    {
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].SetFollowTransform(CSingleton<CustomerManager>.Instance.m_CustomerList[i].transform, 2f);
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].m_Transform.position = CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].m_FollowTransform.position + Vector3.up * CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].m_OffsetUp;
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].m_Transform.LookAt(new Vector3(CSingleton<PricePopupSpawner>.Instance.m_Cam.position.x, CSingleton<PricePopupSpawner>.Instance.m_Cam.position.y, CSingleton<PricePopupSpawner>.Instance.m_Cam.position.z));
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].m_Text.text = GameInstance.GetPriceString(CSingleton<CustomerManager>.Instance.m_CustomerList[i].m_MaxMoney, false, true, false, "F2");
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].m_Text.color = Color.white;
                        CSingleton<PricePopupSpawner>.Instance.m_PricePopupList[j].gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
}
