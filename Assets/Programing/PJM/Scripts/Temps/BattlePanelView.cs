using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class BattlePanelView : UIBInder
{
    [SerializeField] private GameObject hpBarPrefab;
    private void Awake()
    {
        Bind();
        
    }

    private void OnEnable()
    {
        Spawner.OnSpawnCompleted += InstantiateHPBars;
        Spawner.OnSpawnCompleted += InitializeButtons;
    }

    private void InitializeButtons()
    {
        AddEvent("PauseButton", EventType.Click, _ => ToggleTimeScale());
        AddEvent("AutoButton", EventType.Click, _ => ToggleAuto());
    }
    private void OnDisable()
    {
        Spawner.OnSpawnCompleted -= InstantiateHPBars;
        Spawner.OnSpawnCompleted -= InitializeButtons;
    }
    public void ToggleTimeScale()
    {
        if (BattleSceneManager.Instance.isGamePaused)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
        
        BattleSceneManager.Instance.isGamePaused = !BattleSceneManager.Instance.isGamePaused;
        GetUI<TMP_Text>("PauseText").text = BattleSceneManager.Instance.isGamePaused ? "Pause\nON" : "Pause\nOFF";
    }
    
    public void ToggleAuto()
    {
        BattleSceneManager.Instance.isAutoOn = !BattleSceneManager.Instance.isAutoOn;
        GetUI<TMP_Text>("AutoText").text = BattleSceneManager.Instance.isAutoOn ? "Auto\nON" : "Auto\nOFF";
    }

    private void InstantiateHPBars()
    {
        foreach (var playerUnit in BattleSceneManager.Instance.myUnits)
        {
            if(playerUnit == null || !playerUnit.gameObject.activeSelf)
                continue;
            
            GameObject barObject = Instantiate(hpBarPrefab, transform);
            UnitHealthBarController hpBar = barObject.GetComponent<UnitHealthBarController>();
            hpBar.Target = playerUnit.transform;
            if(hpBar.Target == null)
                Debug.Log("타겟 없음");
            hpBar.Unit = playerUnit.UnitModel;
            
            if(hpBar.Unit == null)
                Debug.Log("유닛없음");
        }

        foreach (var enemyUnit in BattleSceneManager.Instance.enemyUnits)
        {
            if(enemyUnit == null || !enemyUnit.gameObject.activeSelf)
                continue;
            
            GameObject barObject = Instantiate(hpBarPrefab, transform);
            UnitHealthBarController hpBar = barObject.GetComponent<UnitHealthBarController>();
            hpBar.Target = enemyUnit.transform;
            if(hpBar.Target == null)
                Debug.Log("타겟 없음");
            hpBar.Unit = enemyUnit.UnitModel;
            
            if(hpBar.Unit == null)
                Debug.Log("유닛없음");
            Image fillImage = hpBar.HealthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.red;
            }
        }
    }

}
