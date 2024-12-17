using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public enum CurrencyType
{
    DinoStone,      // 가챠 재화 ( 젬, 청휘석 )
    Coin,           // 공통 재화 ( 골드, 크레딧 )
    DinoBlood,      // 캐릭터 레벨업 메인 재료 ( 경험치, 보고서, 혼 )
    BoneCrystal,    // 캐릭터 혹은 장비 강화 ( 강화석, 망치 )
}

[System.Serializable]
public class Currency
{
    public CurrencyType type;   // 재화 타입
    public int amount;          // 보유량
    public Sprite icon;         // UI에 표시될 아이콘
}

[System.Serializable]
public class Character
{
    public string name;         // 캐릭터 이름
    public int level;           // 캐릭터 레벨
    public Sprite image;        // 캐릭터 이미지
    // 추가 속성
}

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    private void Awake()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }

    // 재화 리스트
    public List<Currency> currencies = new List<Currency>();

    // 캐릭터 리스트
    public List<Character> characters = new List<Character>();

    // 재화 추가하기
    public void AddCurrency(CurrencyType type, int amount)
    {
        Currency currency = currencies.Find(x => x.type == type);
        if(currency != null)
        {
            // 재화 보유량 증가
            currency.amount += amount;
            Debug.Log($"{type} {amount} 추가됨. 보유 {currency.amount}");
        }
    }

    // 재화 소모하기
    public bool SpendCurrency(CurrencyType type, int amount)
    {
        Currency currency = currencies.Find(x => x.type == type);

        if(currency != null && currency.amount >= amount)
        {
            currency.amount -= amount;
            Debug.Log($"{type} 재화 소모 : {amount}, 잔여 {currency.amount}");
            return true;
        }

        return false;
    }

    // 현재 보유량 출력
    public int GetCurrencyAmount(CurrencyType type)
    {
        Currency currency = currencies.Find(x => x.type == type);
        return currency != null ? currency.amount : 0;
    }
}
