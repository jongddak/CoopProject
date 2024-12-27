using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class DataLoader : MonoBehaviour
{

    [SerializeField] private string _uID;

    [ContextMenu("Test")]
    public void Test()
    {

        DatabaseReference root = BackendManager.Database.RootReference.Child("UserData").Child(_uID);


        root.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("GetValueAsync encountered an error: " + task.Exception);
                return;
            }

            DataSnapshot snapShot = task.Result;

            while (snapShot == null)
            {
                Debug.Log("snapshot null값임!");
            }

            // 이름 가져오기
            PlayerDataManager.Instance.PlayerData.PlayerName = snapShot.Child("_playerName").Value.ToString();

            //최근나간시간 가져오기
            PlayerDataManager.Instance.PlayerData.ExitTime = snapShot.Child("_exitTime").Value.ToString();

            // int형 배열 items 가져오기
            var itemChildren = snapShot.Child("_items").Children.ToList();
            CheckSnapSHot(itemChildren);


            itemChildren = itemChildren.OrderBy(item => TypeCastManager.Instance.TryParseInt(item.Key)).ToList();
            for (int i = 0; i < itemChildren.Count; i++)
            {
                PlayerDataManager.Instance.PlayerData.Items[i] = TypeCastManager.Instance.TryParseInt(itemChildren[i].Value.ToString());
            }


            // int형 배열 storedItem가져오기
            var storedItemChildren = snapShot.Child("_storedItems").Children.ToList();
            CheckSnapSHot(storedItemChildren);

            storedItemChildren = storedItemChildren.OrderBy(storedItem => TypeCastManager.Instance.TryParseInt(storedItem.Key)).ToList();
            for (int i = 0; i < storedItemChildren.Count; i++)
            {
                PlayerDataManager.Instance.PlayerData.StoredItems[i] = TypeCastManager.Instance.TryParseInt(storedItemChildren[i].Value.ToString());
            }


            // int형 배열 unitPos 가져오기
            var unitPosChildren = snapShot.Child("_unitPos").Children.ToList();
            CheckSnapSHot(unitPosChildren);

            unitPosChildren = unitPosChildren.OrderBy(unitPos => TypeCastManager.Instance.TryParseInt(unitPos.Key)).ToList();
            for (int i = 0; i < unitPosChildren.Count; i++)
            {
                PlayerDataManager.Instance.PlayerData.UnitPos[i] = TypeCastManager.Instance.TryParseInt(unitPosChildren[i].Value.ToString());
            }


            //bool형 배열 isStageClear가져오기
            var isStageClearChildren = snapShot.Child("_isStageClear").Children.ToList();
            CheckSnapSHot(isStageClearChildren);

            isStageClearChildren = isStageClearChildren.OrderBy(isStageClear => TypeCastManager.Instance.TryParseInt(isStageClear.Key)).ToList();
            for (int i = 0; i < isStageClearChildren.Count; i++)
            {
                PlayerDataManager.Instance.PlayerData.IsStageClear[i] = TypeCastManager.Instance.TryParseBool(isStageClearChildren[i].Value.ToString());
            }

            // UnitData정보들 가져오기
            var unitDataChildren = snapShot.Child("_unitDatas").Children.ToList();
            CheckSnapSHot(unitDataChildren);

            unitDataChildren = unitDataChildren.OrderBy(unitData => TypeCastManager.Instance.TryParseInt(unitData.Key)).ToList();

            foreach (var unitChild in unitDataChildren)
            {
                PlayerUnitData unitData = new PlayerUnitData
                {
                    UnitId = TypeCastManager.Instance.TryParseInt(unitChild.Child("_unitId").Value.ToString()),
                    UnitLevel = int.Parse(unitChild.Child("_unitLevel").Value.ToString()),
                };
                PlayerDataManager.Instance.PlayerData.UnitDatas.Add(unitData);
            }

        });

    }

    //Snapshot이 제대로 불러와졌는지 체크하는 함수 -> snapshot이 불러와지는데 지연시간이 약간 있는것으로 예상이 됨.
    private void CheckSnapSHot(List<DataSnapshot> snapshotChildren)
    {
        while (snapshotChildren == null || snapshotChildren.Count == 0)
        {
            Debug.Log("snapshot null값임!");
        }

        for(int i = 0; i < snapshotChildren.Count; i++)
        {
            Debug.Log(snapshotChildren[i].Key.ToString());
        }
    }
}
