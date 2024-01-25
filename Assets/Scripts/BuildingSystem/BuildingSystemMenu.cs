using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingSystemMenu : Singleton<BuildingSystemMenu>
{
    public GameObject buildingSystemMenu;
    public Image selectedBuildingBlockImage;

    public List<GameObject> buildingBlockUIList = new List<GameObject>();

    public bool buildingSystemMenu_isOpen;

    [Header("BuildingRequirement Slot")]
    public GameObject buildingRequirement_Parent;
    public List<GameObject> buildingRequirement_List = new List<GameObject>();


    //--------------------


    private void Start()
    {
        PlayerButtonManager.isPressed_BuildingSystemMenu_Enter += BuildingBlockSelecter_Enter;
        PlayerButtonManager.isPressed_BuildingSystemMenu_Exit += BuildingBlockSelecter_Exit;

        buildingSystemMenu.SetActive(false);
    }


    //--------------------


    public void SetSelectedImage(Sprite sprite)
    {
        if (MoveableObjectManager.Instance.moveableObjectType == MoveableObjectType.None)
        {
            selectedBuildingBlockImage.gameObject.SetActive(false);
        }
        else
        {
            selectedBuildingBlockImage.gameObject.SetActive(true);
            selectedBuildingBlockImage.sprite = sprite;
        }
    }
    


    //--------------------


    void BuildingBlockSelecter_Enter()
    {
        buildingSystemMenu_isOpen = true;

        Cursor.lockState = CursorLockMode.None;
        MainManager.Instance.menuStates = MenuStates.MoveableObjectMenu;
        BuildingManager.Instance.SetAllGhostState_Off();

        //Deactivate old directionObjectList
        if (BuildingManager.Instance.old_lastBuildingBlock_LookedAt != null)
        {
            for (int i = 0; i < BuildingManager.Instance.old_lastBuildingBlock_LookedAt.GetComponent<BuildingBlock_Parent>().directionObjectList.Count; i++)
            {
                if (BuildingManager.Instance.old_lastBuildingBlock_LookedAt.GetComponent<BuildingBlock_Parent>().directionObjectList[i].activeInHierarchy)
                {
                    BuildingManager.Instance.old_lastBuildingBlock_LookedAt.GetComponent<BuildingBlock_Parent>().directionObjectList[i].SetActive(false);
                }
            }

            BuildingManager.Instance.old_lastBuildingBlock_LookedAt = null;
        }

        buildingSystemMenu.SetActive(true);
        TabletManager.Instance.tabletMenuState = TabletMenuState.MoveableObjects;
        TabletManager.Instance.OpenTablet();
    }
    void BuildingBlockSelecter_Exit()
    {
        TabletManager.Instance.CloseTablet();
        TabletManager.Instance.tabletMenuState = TabletMenuState.Inventory;
        buildingSystemMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        MainManager.Instance.menuStates = MenuStates.None;

        buildingSystemMenu_isOpen = false;
    }
}