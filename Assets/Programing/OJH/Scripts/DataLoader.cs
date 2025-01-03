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


public class DataLoader : MonoBehaviour
{

    [SerializeField] private string _uID;

    //팔로우 리셋 시간
    [SerializeField] private int _resetFollowTime;

    //팔로우 origin 값
    [SerializeField] private int _originFollowTime;

    [ContextMenu("LoadTest")]
    public void Test()
    {
        DatabaseReference root = BackendManager.Database.RootReference.Child("UserData").Child(_uID);

        Debug.Log(root);

        root.KeepSynced(true);
  
        root.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("GetValueAsync encountered an error: " + task.Exception);
                return;
            }

            DataSnapshot snapShot = task.Result;

            string json = snapShot.GetRawJsonValue();
            Debug.Log(json);
            PlayerDataManager.Instance.PlayerData = JsonUtility.FromJson<PlayerData>(json);

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
