using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class TempBattleSceneUIView : UIBInder
{
    [SerializeField] private GameObject hpBarPrefab;
    private void Awake()
    {
        Bind();
        
    }

    private void OnEnable()
    {
        
    }

    private void Start()
    {
        InstantiateHPBars();
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        AddEvent("PauseButton", EventType.Click, _ => ToggleTimeScale());
        AddEvent("AutoButton", EventType.Click, _ => ToggleAuto());
    }
    private void Update()
    {
        
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
        GetUI<TMP_Text>("PauseText").text = BattleSceneManager.Instance.isGamePaused ? " Pause : ON" : "Pause : OFF";
    }
    
    public void ToggleAuto()
    {
        BattleSceneManager.Instance.isAutoOn = !BattleSceneManager.Instance.isAutoOn;
        GetUI<TMP_Text>("AutoText").text = BattleSceneManager.Instance.isAutoOn ? " Auto : ON" : "Auto : OFF";
        //Debug.Log($"Auto : {TempBattleContext.Instance.isAutoOn}");
    }

    private void InstantiateHPBars()
    {
        foreach (var playerUnit in BattleSceneManager.Instance.myUnits)
        {
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
