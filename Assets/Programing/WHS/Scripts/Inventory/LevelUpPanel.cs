using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpPanel : UIBInder
{
    private PlayerUnitData targetCharacter;
    private int maxLevelUp;
    private int curLevelUp;
    private const int MAXLEVEL = 30;

    private Dictionary<int, Dictionary<string, string>> levelUpData;

    private struct RequiredItems
    {
        public int coin;
        public int dinoBlood;
        public int boneCrystal;
    }

    private void Awake()
    {
        BindAll();
        AddEvent("LevelUpConfirm", EventType.Click, OnConfirmButtonClick);
        GetUI<Slider>("LevelUpSlider").onValueChanged.AddListener(OnSliderValueChanged);
        AddEvent("DecreaseButton", EventType.Click, OnDecreaseButtonClick);
        AddEvent("IncreaseButton", EventType.Click, OnIncreaseButtonClick);

        levelUpData = CsvDataManager.Instance.DataLists[(int)E_CsvData.CharacterLevelUp];
        Debug.Log($"levelUpData count: {levelUpData.Count}");
        foreach (var key in levelUpData.Keys)
        {
            Debug.Log($"Key: {key}, Value: {levelUpData[key]["500"]}, {levelUpData[key]["501"]}, {levelUpData[key]["502"]}");
        }
    }

    private void OnEnable()
    {
        if (PlayerDataManager.Instance.PlayerData.OnItemChanged == null)
        {
            PlayerDataManager.Instance.PlayerData.OnItemChanged = new UnityAction<int>[System.Enum.GetValues(typeof(E_Item)).Length];
        }

        PlayerDataManager.Instance.PlayerData.OnItemChanged[(int)E_Item.Coin] += UpdateCoinText;
        PlayerDataManager.Instance.PlayerData.OnItemChanged[(int)E_Item.DinoBlood] += UpdateDinoBloodText;
        PlayerDataManager.Instance.PlayerData.OnItemChanged[(int)E_Item.BoneCrystal] += UpdateBoneCrystalText;
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.PlayerData != null)
        {
            PlayerDataManager.Instance.PlayerData.OnItemChanged[(int)E_Item.Coin] -= UpdateCoinText;
            PlayerDataManager.Instance.PlayerData.OnItemChanged[(int)E_Item.DinoBlood] -= UpdateDinoBloodText;
            PlayerDataManager.Instance.PlayerData.OnItemChanged[(int)E_Item.BoneCrystal] -= UpdateBoneCrystalText;
        }
    }

    public void Init(PlayerUnitData character)
    {
        targetCharacter = character;
        CalculateMaxLevelUp();

        Slider slider = GetUI<Slider>("LevelUpSlider");
        slider.minValue = 1;
        slider.maxValue = maxLevelUp;
        slider.value = 1;
        curLevelUp = 1;

        UpdateUI();
    }

    // 슬라이더 값 갱신
    private void OnSliderValueChanged(float value)
    {
        curLevelUp = Mathf.RoundToInt(value);
        UpdateUI();
    }

    // 재화소모량 갱신
    private void UpdateUI()
    {
        RequiredItems items = CalculateRequiredItems(curLevelUp);

        int notEnoughCoin = Mathf.Max(0, items.coin - PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.Coin]);
        int notEnoughDinoBlood = Mathf.Max(0, items.dinoBlood - PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.DinoBlood]);
        int notEnoughBoneCrystal = Mathf.Max(0, items.boneCrystal - PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.BoneCrystal]);

        bool canLevelUp = (notEnoughCoin == 0 && notEnoughDinoBlood == 0 && notEnoughBoneCrystal == 0);
        Debug.Log($"{notEnoughCoin}, {notEnoughDinoBlood}, {notEnoughBoneCrystal}");

        GetUI<Button>("DecreaseButton").interactable = (curLevelUp > 1);

        if (canLevelUp)
        {
            GetUI<Button>("DecreaseButton").interactable = true;
            GetUI<Button>("IncreaseButton").interactable = true;
            GetUI<TextMeshProUGUI>("CoinText").text = $"Coin : {items.coin}";
            GetUI<TextMeshProUGUI>("DinoBloodText").text = $"DinoBlood : {items.dinoBlood}";
            GetUI<TextMeshProUGUI>("BoneCrystalText").text = $"BoneCrystal : {items.boneCrystal}";
        }
        else
        {
            GetUI<Button>("DecreaseButton").interactable = false;
            GetUI<Button>("IncreaseButton").interactable = false;
            GetUI<TextMeshProUGUI>("CoinText").text = $"Coin {notEnoughCoin} 부족";
            GetUI<TextMeshProUGUI>("DinoBloodText").text = $"DinoBlood {notEnoughDinoBlood} 부족";
            GetUI<TextMeshProUGUI>("BoneCrystalText").text = $"BoneCrystal {notEnoughBoneCrystal} 부족";
        }

        if (targetCharacter.UnitLevel + curLevelUp > MAXLEVEL)
        {
            GetUI<TextMeshProUGUI>("LevelText").text = $"Lv.{targetCharacter.UnitLevel} -> Lv.{MAXLEVEL} (MAX)";
        }
        else
        {
            GetUI<TextMeshProUGUI>("LevelText").text = $"Lv.{targetCharacter.UnitLevel} -> Lv.{targetCharacter.UnitLevel + curLevelUp}";
        }

        GetUI<Button>("LevelUpConfirm").interactable = canLevelUp;
    }

    // 레벨업 할 최대치
    private void CalculateMaxLevelUp()
    {
        maxLevelUp = 0;
        while (CanLevelUp(maxLevelUp + 1) && (targetCharacter.UnitLevel + maxLevelUp + 1) <= MAXLEVEL)
        {
            maxLevelUp++;
        }
    }

    // 레벨업 가능 여부
    private bool CanLevelUp(int level)
    {
        if (targetCharacter.UnitLevel + level > MAXLEVEL)
        {
            return false;
        }

        RequiredItems items = CalculateRequiredItems(level);

        // 레벨업에 충분한 아이템 보유중일때 true
        return PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.Coin] >= items.coin &&
               PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.DinoBlood] >= items.dinoBlood &&
               PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.BoneCrystal] >= items.boneCrystal;
    }

    // 요구 재화량 계산
    private RequiredItems CalculateRequiredItems(int level)
    {
        RequiredItems items = new RequiredItems();
        int rarity = GetRarity(); // 캐릭터의 레어도 가져와야함, 일단 4

        for (int i = 0; i < level; i++)
        {
            int curLevel = targetCharacter.UnitLevel + i;
            int levelUpId = FindLevelUpId(rarity, curLevel);

            if (curLevel > MAXLEVEL)
            {
                break;
            }

            if (levelUpData.TryGetValue(levelUpId, out Dictionary<string, string> data))
            {
                if (int.TryParse(data["500"], out int coin))
                {
                    items.coin += coin;
                }
                if (int.TryParse(data["501"], out int dinoBlood))
                {
                    items.dinoBlood += dinoBlood;
                }
                if (data.ContainsKey("502") && int.TryParse(data["502"], out int boneCrystal))
                {
                    items.boneCrystal += boneCrystal;
                }
            }
            else
            {
                Debug.LogError($"레벨업 데이터를 찾을 수 없습니다. LevelUpID: {levelUpId}");
                return new RequiredItems();
            }
        }
        return items;
    }

    private int GetRarity()
    {
        // 캐릭터의 레어도 받아와야함
        // TODO : UnitID에 따라 레어도를 찾아와야 할텐데 일단 4
        return 4;
    }

    private int FindLevelUpId(int rarity, int level)
    {
        if(level > MAXLEVEL)
        {
            return -1;
        }

        foreach(var entry in levelUpData)
        {
            // 레어도와 레벨이 같을 때의 키 = LevelUpID 받아오기
            if (int.Parse(entry.Value["Rarity"]) == rarity &&
                int.Parse(entry.Value["Level"]) == level)
            {
                return entry.Key;
            }
        }

        Debug.Log($"LevelUpId 찾지 못함 Rarity {rarity}, Level {level}");

        return -1;
    }

    // 레벨업 버튼
    private void OnConfirmButtonClick(PointerEventData eventData)
    {
        if(targetCharacter.UnitLevel + curLevelUp > MAXLEVEL)
        {
            return;
        }

        RequiredItems items = CalculateRequiredItems(curLevelUp);

        // 아이템이 충분한지 확인하고 레벨업 진행
        if (PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.Coin] >= items.coin &&
            PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.DinoBlood] >= items.dinoBlood &&
            PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.BoneCrystal] >= items.boneCrystal)
        {
            for (int i = 0; i < curLevelUp; i++)
            {
                LevelUp(targetCharacter);
            }

            // UI 및 데이터베이스 업데이트
            UpdateItemsData(items);
            UpdateCharacters(targetCharacter);
            UpdateLevelData(targetCharacter);

            gameObject.SetActive(false);
        }
    }

    // 레벨업
    private bool LevelUp(PlayerUnitData character)
    {
        RequiredItems items = CalculateRequiredItems(1); // 단일 레벨업에 대한 아이템 계산

        if (items.coin == 0 && items.dinoBlood == 0 && items.boneCrystal == 0)
        {
            Debug.Log("요구 아이템 정보를 찾을 수 없습니다.");
            return false;
        }

        PlayerDataManager.Instance.PlayerData.SetItem((int)E_Item.Coin, PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.Coin] - items.coin);
        PlayerDataManager.Instance.PlayerData.SetItem((int)E_Item.DinoBlood, PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.DinoBlood] - items.dinoBlood);
        PlayerDataManager.Instance.PlayerData.SetItem((int)E_Item.BoneCrystal, PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.BoneCrystal] - items.boneCrystal);

        character.UnitLevel++;

        // 레벨업 후 PlayerDataManager 업데이트
        for (int i = 0; i < PlayerDataManager.Instance.PlayerData.UnitDatas.Count; i++)
        {
            if (PlayerDataManager.Instance.PlayerData.UnitDatas[i].UnitId == character.UnitId)
            {
                PlayerDataManager.Instance.PlayerData.UnitDatas[i] = character;
                break;
            }
        }

        Debug.Log($"{character.UnitId} 레벨업 {character.UnitLevel}");

        return true;
    }

    // db에 소모한 아이템 갱신
    private void UpdateItemsData(RequiredItems items)
    {
        string userId = BackendManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef = BackendManager.Database.RootReference.Child("UserData").Child(userId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            ["_items/0"] = PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.Coin],
            ["_items/1"] = PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.DinoBlood],
            ["_items/2"] = PlayerDataManager.Instance.PlayerData.Items[(int)E_Item.BoneCrystal]
        };

        userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"아이템 업데이트 실패 {task.Exception}");
            }
            if (task.IsCanceled)
            {
                Debug.Log($"아이템 업데이트 중단됨 {task.Exception}");
            }

            Debug.Log("소모한 아이템 갱신");
        });
    }

    // db에 레벨업 데이터 갱신
    private void UpdateLevelData(PlayerUnitData character)
    {
        string userID = BackendManager.Auth.CurrentUser.UserId;
        DatabaseReference characterRef = BackendManager.Database.RootReference
            .Child("UserData").Child(userID).Child("_unitDatas");

        characterRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("캐릭터 데이터 로딩 중 오류 발생: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            foreach (var childSnapshot in snapshot.Children)
            {
                if (int.Parse(childSnapshot.Child("_unitId").Value.ToString()) == character.UnitId)
                {
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        ["_unitLevel"] = character.UnitLevel
                    };

                    childSnapshot.Reference.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log($"캐릭터 ID {character.UnitId}의 레벨이 {character.UnitLevel}로 업데이트됨");
                        }
                        else
                        {
                            Debug.LogError($"캐릭터 ID {character.UnitId} 업데이트 실패: " + updateTask.Exception);
                        }
                    });
                    break;
                }
            }
        });
    }

    // UI 갱신
    private void UpdateCharacters(PlayerUnitData character)
    {
        // 캐릭터 정보에 보여지는 레벨 갱신
        CharacterPanel characterPanel = FindObjectOfType<CharacterPanel>();
        if (characterPanel != null)
        {
            characterPanel.UpdateCharacterInfo(character);
        }

        // 인벤토리 슬롯에 보여지는 레벨 갱신
        InventoryPanel inventoryPanel = FindObjectOfType<InventoryPanel>();
        if (inventoryPanel != null)
        {
            inventoryPanel.UpdateCharacterUI(character);
        }

        // 아이템패널 갱신
        ItemPanel.Instance.UpdateItems();
    }

    private void OnDecreaseButtonClick(PointerEventData eventData)
    {
        if (GetUI<Button>("DecreaseButton").interactable == false)
        {
            return;

        }
        if (curLevelUp > 0)
        {
            curLevelUp--;
            UpdateUI();
            GetUI<Slider>("LevelUpSlider").value = curLevelUp;
        }

    }

    private void OnIncreaseButtonClick(PointerEventData eventData)
    {
        if (GetUI<Button>("IncreaseButton").interactable == false)
        {
            return;

        }
        if (curLevelUp < maxLevelUp && (targetCharacter.UnitLevel + curLevelUp) + 1 <= MAXLEVEL)
        {
            curLevelUp++;
            UpdateUI();
            GetUI<Slider>("LevelUpSlider").value = curLevelUp;
        }
    }

    private void UpdateCoinText(int newValue)
    {
        GetUI<TextMeshProUGUI>("CoinText").text = $"Coin : {newValue}";
        CalculateMaxLevelUp();
        UpdateUI();
    }

    private void UpdateDinoBloodText(int newValue)
    {
        GetUI<TextMeshProUGUI>("DinoBloodText").text = $"DinoBlood : {newValue}";
        CalculateMaxLevelUp();
        UpdateUI();
    }

    private void UpdateBoneCrystalText(int newValue)
    {
        GetUI<TextMeshProUGUI>("BoneCrystalText").text = $"BoneCrystal : {newValue}";
        CalculateMaxLevelUp();
        UpdateUI();
    }
}
