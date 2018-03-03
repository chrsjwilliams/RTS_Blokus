﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOptionsSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;

    private const float SECONDS_TO_WAIT = 0.01f;
    private bool[] humanPlayers;

    private int levelSelected;
    [SerializeField]
    private GameObject levelButtonParent;
    private Button[] levelButtons;
    [SerializeField]
    private Image levelSelectionIndicator;
    [SerializeField]
    private Slider winWeightSlider;
    [SerializeField]
    private Slider structureWeightSlider;
    [SerializeField]
    private Slider blueprintWeightSlider;
    [SerializeField]
    private Slider attackWeightSlider;
    [SerializeField]
    private Button[] joinButtons;
    private Text[] joinButtonJoinTexts;
    private Text[] joinButtonPlayerTypeTexts;
    private Color[] baseColors;
    [SerializeField]
    private float defaultWinWeight;
    [SerializeField]
    private float defaultStructWeight;
    [SerializeField]
    private float defaultBlueprintWeight;
    [SerializeField]
    private float defaultAttackWeight;

    private float timeElapsed;
    private const float textPulsePeriod = 0.35f;
    private const float textPulseMaxScale = 1.075f;
    private bool pulsingUp = true;

    private TaskManager _tm = new TaskManager();

    private bool aiOptionsActive;
    [SerializeField]
    private GameObject aiOptionsMenu;

    internal override void OnEnter(TransitionData data)
    {
        if (PlayerPrefs.HasKey("winWeight"))
        {
            winWeightSlider.value = PlayerPrefs.GetFloat("winWeight");
        }
        else
        {
            winWeightSlider.value = defaultWinWeight;
        }
        if (PlayerPrefs.HasKey("structWeight"))
        {
            structureWeightSlider.value = PlayerPrefs.GetFloat("structWeight");
        }
        else
        {
            structureWeightSlider.value = defaultStructWeight;
        }
        if (PlayerPrefs.HasKey("blueprintWeight"))
        {
            blueprintWeightSlider.value = PlayerPrefs.GetFloat("blueprintWeight");
        }
        else
        {
            blueprintWeightSlider.value = defaultBlueprintWeight;
        }
        if (PlayerPrefs.HasKey("attackWeight"))
        {
            attackWeightSlider.value = PlayerPrefs.GetFloat("attackWeight");
        }
        else
        {
            attackWeightSlider.value = defaultAttackWeight;
        }
        levelButtons = levelButtonParent.GetComponentsInChildren<Button>();
        SelectLevel(0);
        aiOptionsMenu.SetActive(false);
        humanPlayers = new bool[2] { false, false };
        joinButtonPlayerTypeTexts = new Text[2] {
            joinButtons[0].GetComponentInChildren<Text>(),
            joinButtons[1].GetComponentInChildren<Text>()
        };
        joinButtonJoinTexts = new Text[2] {
            joinButtons[0].GetComponentsInChildren<Text>()[1],
            joinButtons[1].GetComponentsInChildren<Text>()[1]
        };
        baseColors = new Color[2] { Services.GameManager.Player1ColorScheme[0],
                        Services.GameManager.Player2ColorScheme[0] };

        for (int i = 0; i < 2; i++)
        {
            joinButtons[i].GetComponent<Image>().color = (baseColors[i] + Color.white) / 2;
        }

        if (Services.GameManager.tutorialMode)
        {
            levelButtonParent.SetActive(false);
            levelSelectionIndicator.gameObject.SetActive(false);
            levelSelected = 4;
        }
    }

    internal override void OnExit()
    {
        PlayerPrefs.Save();
    }

    public void SetWinWeight()
    {
        Services.GameManager.SetWinWeight(winWeightSlider.value);
    }

    public void SetStructureWeight()
    {
        Services.GameManager.SetStructureWeight(structureWeightSlider.value);
    }

    public void SetBlueprintWeight()
    {
        Services.GameManager.SetBlueprintWeight(blueprintWeightSlider.value);
    }

    public void SetAttackWeight()
    {
        Services.GameManager.SetAttackWeight(attackWeightSlider.value);
    }

    public void StartGame()
    {
        Services.GameManager.SetUserPreferences(levelSelected);
        _tm.Do
        (
                    new Wait(SECONDS_TO_WAIT))
              .Then(new ActionTask(ChangeScene)
        );
        
    }

    private void ChangeScene()
    {
        Services.GameManager.SetNumPlayers(humanPlayers);
        Services.Scenes.Swap<GameSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
        if (pulsingUp)
        {
            timeElapsed += Time.deltaTime;
        }
        else
        {
            timeElapsed -= Time.deltaTime;
        }
        if (timeElapsed >= textPulsePeriod)
        {
            pulsingUp = false;
        }
        if(timeElapsed <= 0)
        {
            pulsingUp = true;
        }
        for (int i = 0; i < joinButtonJoinTexts.Length; i++)
        {
            if (!humanPlayers[i])
            {
                joinButtonJoinTexts[i].transform.localScale =
                    Vector3.Lerp(Vector3.one, textPulseMaxScale * Vector3.one,
                    EasingEquations.Easing.QuadEaseOut(timeElapsed / textPulsePeriod));
                //joinButtonJoinTexts[i].color = Color.Lerp(new Color(1,1,1,0.8f), Color.white,
                //    EasingEquations.Easing.QuadEaseOut(timeElapsed / textPulsePeriod));

            }
            else
            {
                joinButtonJoinTexts[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void SelectLevel(int levelNum)
    {
        levelSelected = levelNum;
        MoveLevelSelector(levelNum);
    }

    void MoveLevelSelector(int levelNum)
    {
        levelSelectionIndicator.transform.position = 
            levelButtons[levelNum].transform.position;
    }

    public void ToggleAIOptionsMenu()
    {
        aiOptionsActive = !aiOptionsActive;
        aiOptionsMenu.SetActive(aiOptionsActive);
    }

    public void ToggleHumanPlayer(int playerNum)
    {
        int index = playerNum - 1;
        humanPlayers[index] = !humanPlayers[index];
        if (humanPlayers[index])
        {
            joinButtonPlayerTypeTexts[index].text = "Human \n";
            joinButtonJoinTexts[index].text = "\nTap to Withdraw";
            joinButtons[index].GetComponent<Image>().color = baseColors[index];
            joinButtonJoinTexts[index].color = Color.white;
            joinButtonPlayerTypeTexts[index].color = Color.white;
        }
        else
        {
            joinButtonPlayerTypeTexts[index].text = "CPU \n";
            joinButtonJoinTexts[index].text = "\nTap to Join";
            joinButtons[index].GetComponent<Image>().color = (baseColors[index] + Color.white) / 2;
            joinButtonJoinTexts[index].color = Color.black;
            joinButtonPlayerTypeTexts[index].color = Color.black;
        }
    }
}
