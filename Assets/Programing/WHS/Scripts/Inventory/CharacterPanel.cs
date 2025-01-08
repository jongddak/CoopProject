using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPanel : UIBInder
{
    private PlayerUnitData curCharacter;
    private GameObject levelUpPanel;

    private Dictionary<int, Dictionary<string, string>> characterData;
    private Dictionary<int, Dictionary<string, string>> skillData;

    private SceneChanger _sceneChanger;

    private int index;
    private List<PlayerUnitData> characterList;

    private void Awake()
    {
        BindAll();
        AddEvent("LevelUpButton", EventType.Click, OnLevelUpButtonClick);
        AddEvent("HomeButton", EventType.Click, GoLobby);
        AddEvent("PreviousCharacterButton", EventType.Click, PreviousButton);
        AddEvent("NextCharacterButton", EventType.Click, NextButton);

        Transform parent = GameObject.Find("CharacterPanel").transform;
        levelUpPanel = parent.Find("LevelUpPanel").gameObject;

        characterData = CsvDataManager.Instance.DataLists[(int)E_CsvData.Character];
        skillData = CsvDataManager.Instance.DataLists[(int)E_CsvData.CharacterSkill];

        _sceneChanger = FindObjectOfType<SceneChanger>();

        characterList = PlayerDataManager.Instance.PlayerData.UnitDatas;

    }

    private void Start()
    {
        UpdateCharacterInfo(curCharacter);
    }

    // 캐릭터 정보 갱신
    public void UpdateCharacterInfo(PlayerUnitData character)
    {
        curCharacter = character;
        index = characterList.FindIndex(c => c.UnitId == character.UnitId);
        Debug.Log($"{index} 현재 인덱스");

        int level = character.UnitLevel;
        if (characterData.TryGetValue(character.UnitId, out var data))
        {
            // 캐릭터 이미지
            string portraitPath = $"Portrait/portrait_{character.UnitId}";
            if (portraitPath != null)
            {
                GetUI<Image>("CharacterImage").sprite = Resources.Load<Sprite>(portraitPath);
            }

            // 레벨, 이름
            GetUI<TextMeshProUGUI>("LevelText").text = character.UnitLevel.ToString();
            GetUI<TextMeshProUGUI>("NameText").text = data["Name"];

            // 속성 아이콘 이미지 
            if (int.TryParse(data["ElementID"], out int elementId))
            {
                string elementPath = $"UI/element_{elementId}";
                Sprite elementSprite = Resources.Load<Sprite>(elementPath);
                if (elementSprite != null)
                {
                    GetUI<Image>("ElementImage").sprite = elementSprite;
                }
                else
                {
                    Debug.LogWarning($"이미지를 찾을 수 없음: {elementPath}");
                }
            }

            // 레어도에 따라 별 개수 ~5개 출력
            if (int.TryParse(data["Rarity"], out int rarity))
            {
                UpdateStar(rarity);
            }

            // TODO : 스킬 정보 가져오기
            UpdateSkill(character.UnitId);

            // TODO : 레벨에 따라 증가한 스탯
            GetUI<TextMeshProUGUI>("HPText").text = "HP : " + CalculateStat(int.Parse(data["BaseHp"]), level);
            GetUI<TextMeshProUGUI>("AttackText").text = "Atk : " + CalculateStat(int.Parse(data["BaseATK"]), level);
            GetUI<TextMeshProUGUI>("DefText").text = "Def : " + CalculateStat(int.Parse(data["BaseDef"]), level);

            GetUI<Button>("LevelUpButton").interactable = (character.UnitLevel < 30);

            UpdateCharacterData(character);
        }
    }

    // DB에 캐릭터 정보 갱신
    private void UpdateCharacterData(PlayerUnitData character)
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
                        ["_unitLevel"] = character.UnitLevel,
                    };

                    childSnapshot.Reference.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log($"캐릭터 ID {character.UnitId}의 데이터가 업데이트됨");
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

    private void OnLevelUpButtonClick(PointerEventData eventData)
    {
        if (curCharacter != null && curCharacter.UnitLevel < 30)
        {
            levelUpPanel.gameObject.SetActive(true);
            BackButtonManager.Instance.AddBackAction(ClosePanel);

            LevelUpPanel levelUp = levelUpPanel.GetComponent<LevelUpPanel>();
            levelUp.Init(curCharacter);
        }
    }

    public void ClosePanel()
    {
        levelUpPanel.SetActive(false);
    }

    // 레벨당 스탯 계산
    private int CalculateStat(int baseStat, int level)
    {
        // TODO : Character 시트에서 "Increase"에 따라 해당하는 배율만큼 레벨마다 합증가

        if (!characterData.TryGetValue(curCharacter.UnitId, out var data))
        {
            Debug.LogError($"캐릭터 데이터를 찾을 수 없음 {curCharacter.UnitId}");
            return baseStat;
        }

        if (!int.TryParse(data["Increase"], out int increase))
        {
            Debug.LogError($"Increase 값 찾을 수 없음 {curCharacter.UnitId}");
            return baseStat;
        }

        int levelIncrease = level - 1;
        float totalIncrease = 1 + (increase * levelIncrease / 100f); // 1.n배 

        return Mathf.FloorToInt(baseStat * totalIncrease);
    }

    private void UpdateSkill(int unitId)
    {
        foreach (var value in skillData.Values)
        {
            if (int.Parse(value["CharID"]) == unitId)
            {
                GetUI<TextMeshProUGUI>("SkillNameText").text = value["SkillName"];
                GetUI<TextMeshProUGUI>("CoolDownText").text = $"쿨타임: {value["Cooldown"]}초";
                GetUI<TextMeshProUGUI>("SkillDescriptionText").text = value["SkillDescription"];
                return;
            }
        }

        Debug.LogWarning($"스킬 정보를 찾을 수 없음: CharID {unitId}");
    }

    private void PreviousButton(PointerEventData eventData)
    {
        // 이전 캐릭터 정보로 이동
        if (index > 0)
        {
            index--;
        }
        else
        {
            index = characterList.Count - 1;
        }

        UpdateCharacterInfo(characterList[index]);
    }

    private void NextButton(PointerEventData eventData)
    {
        // 다음 캐릭터 정보로 이동
        if (index < characterList.Count - 1)
        {
            index++;
        }
        else
        {
            index = 0;
        }

        UpdateCharacterInfo(characterList[index]);
    }

    private void UpdateStar(int rarity)
    {
        // rarity에 따라 별 개수를 출력
        for (int i = 0; i < 5; i++)
        {
            Image starImage = GetUI<Image>($"Star_{i + 1}");
            if (starImage != null)
            {
                if (i < rarity)
                {
                    starImage.gameObject.SetActive(true);
                    starImage.sprite = Resources.Load<Sprite>("UI/icon_star");
                }
                else
                {
                    starImage.gameObject.SetActive(false);
                }
            }
        }
    }

    private void GoLobby(PointerEventData eventData)
    {
        _sceneChanger.CanChangeSceen = true;
        _sceneChanger.ChangeScene("Lobby_OJH");
    }
}
