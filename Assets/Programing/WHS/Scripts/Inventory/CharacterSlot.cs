using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlot : UIBInder
{
    private PlayerUnitData _unitData;
    private GameObject _characterPanel;

    [SerializeField] private AudioClip _buttonClip;

    private void Awake()
    {
        BindAll();
        AddEvent("Character(Clone)", EventType.Click, OnClick);

        Transform parent = GameObject.Find("MainPanel").transform;
        _characterPanel = parent.Find("CharacterPanel").gameObject;
    }

    // 인벤토리에 캐릭터 세팅 - 이미지, 이름, 레벨 ( 레어도, 포지션 등 )
    public void SetCharacter(PlayerUnitData newUnitData)
    {
        _unitData = newUnitData;

        Dictionary<int, Dictionary<string, string>> characterData = CsvDataManager.Instance.DataLists[(int)E_CsvData.Character];

        // 이름
        if (characterData.TryGetValue(_unitData.UnitId, out var data))
        {
            GetUI<TextMeshProUGUI>("NameText").text = data["Name"];
        }
        else
        {
            GetUI<TextMeshProUGUI>("NameText").text = _unitData.UnitId.ToString();
        }

        // 레벨
        GetUI<TextMeshProUGUI>("LevelText").text = _unitData.UnitLevel.ToString();

        // 레어도
        if (int.TryParse(data["Rarity"], out int rarity))
        {
            UpdateStar(rarity);
        }

        // 원소 속성 이미지
        if (int.TryParse(data["ElementID"], out int elementId))
        {
            string elementPath = $"Element/element_{elementId}";
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

        // 초상화 이미지
        string portraitPath = $"Portrait/portrait_{_unitData.UnitId}";
        if (portraitPath != null)
        {
            GetUI<Image>("CharacterImage").sprite = Resources.Load<Sprite>(portraitPath);
        }
        else
        {
            Debug.LogWarning($"이미지를 찾을 수 없음 {portraitPath}");
        }
    }

    // 클릭 시 ( 캐릭터 정보 출력, 추가 UI )
    private void OnClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(_buttonClip);

        BackButtonManager.Instance.OpenPanel(_characterPanel);

        _characterPanel.GetComponent<CharacterPanel>().UpdateCharacterInfo(_unitData);
    }

    public PlayerUnitData GetCharacter()
    {
        return _unitData;
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
                    starImage.sprite = Resources.Load<Sprite>("Element/icon_star");
                }
                else
                {
                    starImage.gameObject.SetActive(false);
                }
            }
        }
    }
}
