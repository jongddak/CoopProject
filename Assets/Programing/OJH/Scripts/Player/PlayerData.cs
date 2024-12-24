using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum E_Item { Coin, DinoBlood, BoneCrystal, DinoStone, Stone, Length }

// 여러 컨텐츠쪽에서 직접적으로 바로 필요한 것들만 최소한으로 저장. 
[System.Serializable]
public class PlayerData : MonoBehaviour
{
    [SerializeField] private string _playerName;

    public string PlayerName { get { return _playerName; } set { _playerName = value; } }


    [SerializeField] private string _exitTime; // 게임종료시간 -> 추후 방치형보상쪽에서 사용할 예정.

    public string ExitTime { get { return _exitTime; } set { _exitTime = value; } }

    [SerializeField] private int[] _items;

    public int[] Items { get { return _items; } private set { } }

    [SerializeField] private bool[] _isStageClear;   //스테이지 클리어 여부

    public bool[] IsStageClear { get { return _isStageClear; } private set { } }

    [SerializeField] private List<PlayerUnitData> _unitDatas;

    public List<PlayerUnitData> UnitDatas { get { return _unitDatas; } private set { } }


}
