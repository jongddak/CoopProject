using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleSceneManager : MonoBehaviour
{
    private static BattleSceneManager _instance;
    public static BattleSceneManager Instance { get { return _instance; } set { _instance = value; } }



    [SerializeField] private DraggableUI[] Draggables;
    

    [SerializeField] public GameObject[] inGridObject; // 아군 정보 배열 나중에 타입을 바꾸면 될듯

    [SerializeField] public string[] enemyGridObject;// 적 정보 배열

    [SerializeField] public int _timeLimit; // private 프로퍼티로 바꿀 예정

    [SerializeField] public Dictionary<int, int> curItemValues = new Dictionary<int, int>();// 클리어 보상
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        inGridObject = new GameObject[10]; // 캐릭터 목록이 0번 
        enemyGridObject = new string[9];
        
    }

    public void StageStart()
    {
        string datas = "";
        int count = 0;
        Debug.Log("스테이지 시작");
        // 씬만 넘어가게 해도 될듯 이미 매니저에 데이터가 있으니 
        for (int i = 0; i < inGridObject.Length; i++)
        {
            if (inGridObject[i] != null)
            {
                datas += i.ToString();
                count++;
            }
        }
        Debug.Log(datas);
        if (count > 5)
        {
            Debug.Log("5인 초과 출발 불가");  // 0 인 출발도 못하게 해야함
        }
    }
    public void BackStage()
    {
        // 스테이지패널 , 배틀씬 매니저정보 초기화 해야함
        for (int i = 0; i < inGridObject.Length; i++)
        {
            inGridObject[i] = null;

        }
        for (int i = 0; i < enemyGridObject.Length; i++)
        {
            enemyGridObject[i] = null;
        }
    }
    public void GetDraggables()
    {
       
        for (int i = 0; i < 13; i++)
        {
            if (Draggables[i] == null)
            {
                Debug.Log("Slot" + i.ToString());
                Draggables[i] = GameObject.Find("Slot" + i.ToString()).GetComponent<DraggableUI>();
                Draggables[i].gameObject.SetActive(false);

            }

        }
        Debug.Log(PlayerDataManager.Instance.PlayerData.UnitDatas.Count);
        for (int i = 0; i < PlayerDataManager.Instance.PlayerData.UnitDatas.Count; i++)
        {
            Draggables[i].gameObject.SetActive(true);

            int id = PlayerDataManager.Instance.PlayerData.UnitDatas[i].UnitId;
            string name = CsvDataManager.Instance.DataLists[5][id]["Name"];
            string level = PlayerDataManager.Instance.PlayerData.UnitDatas[i].UnitLevel.ToString();
            Sprite sprite = Resources.Load<Sprite>("Portrait/portrait_"+id.ToString());
            Draggables[i].GetComponent<CharSlot>().setCharSlotData(name, level,sprite); // 이미지는 리소스 파일기준으로 사용하자
                                                                                 // 리소스 파일 이름에 id 같은거 포함 시키면 될듯함 
            // 지금 형태는 csv에서 바로 가져오는 형태임 ,이걸 여기서 하는게 맞나 ?   
        }

    }

    private void BattleSceneStart()
    {
        // 적 위치, 종류(id?)
        // 아군 위치, 종류 최종 스탯 
        // 스테이지 시간 

        Spawner spawner = FindAnyObjectByType<Spawner>();

        spawner.SpawnUnits();

    }


}