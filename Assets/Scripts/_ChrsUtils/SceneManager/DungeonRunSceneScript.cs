﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

/*
 *      TODO: 
 *              Have a progress bar similar to tutorial
 *              Have an area for the reward
 *              Have tooltips for characters in dungeon run
 *              Endow ai with their own tech power-up?
 *              characters that taunt the play when the player loses/
 *                  whine when player wins
 *              
 *              
 *              Skins!?
 * 
 *          HYPER MODE!!!!!
 */ 

public class DungeonRunSceneScript : Scene<TransitionData>
{
    public bool[] humanPlayers { get; private set; }
    public static string progressFileName
    {
        get
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar +
              "progress.txt";
        }
    }
    private Level levelSelected;

    [SerializeField]
    private GameObject dungeonRunMenu;

    //  Dungeon Run Challenge Menu
    [SerializeField]
    private TextMeshProUGUI challengeLevelText;
    [SerializeField]
    private TextMeshProUGUI currentTechZone;
    private Button[][] currentTechMenu;
    [SerializeField]
    private Button startChallengeButton;

    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    private TaskManager _tm = new TaskManager();

    // Use this for initialization
    void Start()
    {

    }

    internal override void OnEnter(TransitionData data)
    {
        int dungeonRunProgress = 0;
        int completedDungeonRuns = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out dungeonRunProgress);
            int.TryParse(fileText, out completedDungeonRuns);
        }
        humanPlayers = new bool[2] { false, false };
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        SetDungeonRunProgress(DungeonRunManager.dungeonRunData.challengeNum);
        SetUpDungeonRunChallengeMenu();
        TaskTree dungeonRunChallengeSelect = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu)),
            new TaskTree(new AILevelSlideIn(currentTechZone, currentTechMenu[0], true, false)),
            new TaskTree(new LevelSelectTextEntrance(backButton,true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(dungeonRunChallengeSelect);

    }

    internal override void OnExit()
    {
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);
    }

    internal override void ExitTransition()
    {
        TaskTree dungeonRunExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu, false, true)),
            new TaskTree(new AILevelSlideIn(currentTechZone, currentTechMenu[0], true, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));
        _tm.Do(dungeonRunExit);
    }

    private void SetDungeonRunProgress(int progress)
    {
        if (DungeonRunManager.dungeonRunData.completedRun)
        {
            challengeLevelText.text = "Run Complete";
            startChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Try Again";
        }
        else
        {
            startChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Challenge";
            challengeLevelText.text = "Challenge " + progress + "/" + DungeonRunManager.MAX_DUNGEON_CHALLENGES;
        }
    }

    private void SetUpDungeonRunChallengeMenu()
    {

        currentTechMenu = new Button[1][];

        Button[] techButtons = currentTechZone.GetComponentsInChildren<Button>();
        currentTechMenu[0] = new Button[techButtons.Length];

        for (int j = 0; j < techButtons.Length; j++)
        {
            currentTechMenu[0][j] = techButtons[j];

            currentTechMenu[0][j].interactable = false;
            currentTechMenu[0][j].GetComponent<Image>().color = Services.GameManager.NeutralColor;
            currentTechMenu[0][j].GetComponentInChildren<TextMeshProUGUI>().text = "None";
            currentTechMenu[0][j].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
            currentTechMenu[0][j].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(BuildingType.NONE);
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = DungeonRunManager.GetBuildingFromType(selectedType);
            currentTechMenu[0][i].interactable = true;
            currentTechMenu[0][i].GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            currentTechMenu[0][i].GetComponentsInChildren<Image>()[1].color = Color.white;
            currentTechMenu[0][i].GetComponentInChildren<TextMeshProUGUI>().text = tech.GetName();
            currentTechMenu[0][i].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        }
    }

    public void StartGame()
    {
        if (DungeonRunManager.dungeonRunData.completedRun)
        {
            DungeonRunManager.ResetDungeonRun();
        }

        Services.GameManager.SetCurrentLevel(levelSelected);
        Task changeScene = new WaitUnscaled(0.01f);
        changeScene.Then(new ActionTask(ChangeScene));

        _tm.Do(changeScene);
    }

    private void ChangeScene()
    {
        Services.GameManager.SetNumPlayers(humanPlayers);
        Services.Scenes.Swap<GameSceneScript>();
    }

    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }

    // Update is called once per frame
    void Update()
    {
        _tm.Update();
    }
}