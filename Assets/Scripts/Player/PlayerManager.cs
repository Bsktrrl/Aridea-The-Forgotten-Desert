using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Stats")]
    public float InteractableDistance = 3.5f;
    public float movementSpeedMultiplier_SkillTree = 1f;

    public float jumpHeight = 2f;

    PlayerStats playerStats;

    [Header("Movement States")]
    public MovementStates oldMovementStates;
    public MovementStates movementStates;

    [Header("FOV values")]
    public float FOV_Addon;
    public float FOV_Standing;
    public float FOV_Walking;
    public float FOV_Running;
    public float FOV_Crouching;

    [Header("HealthParameterMovementVariables")]
    public int hungerTemp = 0;
    public int thirstTemp = 0;


    //--------------------


    private void Update()
    {
        SaveData();
    }


    //--------------------


    public void LoadData()
    {
        //Get loaded data
        playerStats = DataManager.Instance.playerStats_Store;

        //Set Player Position
        MainManager.Instance.player.transform.SetPositionAndRotation(playerStats.playerPos, playerStats.playerRot);

        InteractableDistance = playerStats.InteractableDistance;
        movementSpeedMultiplier_SkillTree = playerStats.movementSpeedMultiplier_SkillTree;

        jumpHeight = playerStats.jumpHeight;
    }
    public void SaveData()
    {
        playerStats.playerPos = MainManager.Instance.player.transform.position;
        playerStats.playerRot = MainManager.Instance.player.transform.rotation;

        playerStats.InteractableDistance = InteractableDistance;
        playerStats.movementSpeedMultiplier_SkillTree = movementSpeedMultiplier_SkillTree;

        playerStats.jumpHeight = jumpHeight;

        DataManager.Instance.playerStats_Store = playerStats;
    }
    public void SaveData(ref GameData gameData)
    {
        SaveData();

        print("Save_PlayerManager");
    }


    //--------------------


}

[Serializable]
public class PlayerStats
{
    public Vector3 playerPos;
    public Quaternion playerRot;

    public float jumpHeight;

    public float InteractableDistance;
    public float movementSpeedMultiplier_SkillTree;
}

public enum MovementStates
{
    Standing,

    Walking,
    Running,
    Crouching,
    Jumping
}