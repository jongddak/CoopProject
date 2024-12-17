using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInventoryUI : MonoBehaviour
{
    public GameObject characterPrefab;  // ĳ���� ĭ ������
    public Transform content;           // ��ũ�Ѻ��� content

    private List<Character> characterList = new List<Character>();

    private void Start()
    {
        Sprite tyranoSprite = Resources.Load<Sprite>("1");
        Sprite yranoSprite = Resources.Load<Sprite>("2");
        Sprite ranoSprite = Resources.Load<Sprite>("3");
        Sprite anoSprite = Resources.Load<Sprite>("4");
        Sprite noSprite = Resources.Load<Sprite>("5");
        Sprite oSprite = Resources.Load<Sprite>("6");

        AddCharacter(new Character { Name = "tyrano", level = 10, image = tyranoSprite });
        AddCharacter(new Character { Name = "yrano", level = 50, image = yranoSprite });
        AddCharacter(new Character { Name = "rano", level = 33, image = ranoSprite });
        AddCharacter(new Character { Name = "ano", level = 44, image = anoSprite });
        AddCharacter(new Character { Name = "no", level = 12, image = noSprite });
        AddCharacter(new Character { Name = "o", level = 21, image = oSprite });

        PopulateGrid();
    }

    // ĳ���� �߰��ϱ�
    public void AddCharacter(Character character)
    {
        characterList.Add(character);
    }

    // �׸��忡 ���� ĳ���� ����
    public void PopulateGrid()
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // ĳ���� ����
        foreach(Character character in characterList)
        {
            GameObject slot = Instantiate(characterPrefab, content);
            CharacterSlotUI slotUI = slot.GetComponent<CharacterSlotUI>();

            // ���� ĳ���� ���� ����
            slotUI.SetCharacter(character);
        }
    }

    // �������� ���� ĳ���͵� ����?

    // �������� ���� ĳ���ʹ� ������ �������� �̵��ϰ�?

    // ���������� ĳ���͸� �����ϸ� ���� ��Ͽ� �߰�?
}