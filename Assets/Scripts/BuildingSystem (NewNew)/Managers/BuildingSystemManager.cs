using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BuildingSystemManager : Singleton<BuildingSystemManager>
{
    #region Variables
    [Header("_SO")]
    public BuildingBlocks_SO buildingBlocks_SO;
    public Furniture_SO furniture_SO;
    public Machines_SO machines_SO;

    [Header("Active BuildingObject")] //From the BuildingObject Menu - Tablet
    public ActiveBuildingObject activeBuildingObject_Info; //Active BuildingObject right now
    public List<WorldBuildingObject> worldBuildingObjectInfoList = new List<WorldBuildingObject>(); //Info about all Objects in the world

    [Header("World Objects")]
    public GameObject worldObject_Parent;
    public List<GameObject> worldBuildingObjectListSpawned = new List<GameObject>(); //All Physical Objects in the world

    public GameObject WorldObjectGhost_Parent; //Ghost Parent
    public GameObject ghostObject_Holding; //GhostBlock Holding

    [Header("Object Ghost")]
    public Material canPlace_Material;
    public Material cannotPlace_Material;
    public Material invisible_Material;

    public GameObject buildingBlockHit;

    public bool isSnapping;

    public float rotationValue = 0;
    [SerializeField] float rotationSpeed = 75;

    public BuildingBlockColliderDirection directionHit;

    Ray ray;
    RaycastHit hit;
    RaycastHit rayHit;

    public LayerMask layerMask_Ground;
    public LayerMask layerMask_IgnoreRaycast;
    public LayerMask layerMask_BuildingBlock;
    public LayerMask layerMask_BuildingBlockModel;
    public LayerMask layerMask_Furniture;
    public LayerMask layerMask_Machine;

    Transform hitTransform;

    [SerializeField] float rayHitAngle_Normal;
    [SerializeField] float rayHitAngle_Left;
    [SerializeField] float rayHitAngle_Right;
    [SerializeField] float rayHitAngle_Up;
    [SerializeField] float rayHitAngle_Down;

    Quaternion ghostRotation = Quaternion.identity;

    [Header("BuildingBlocks Detected")]
    [SerializeField] GameObject BB_Normal;
    [SerializeField] GameObject BB_Up;
    [SerializeField] GameObject BB_Down;
    [SerializeField] GameObject BB_Left;
    [SerializeField] GameObject BB_Right;

    [Header("Having correct conditions to Build?")]
    public bool canPlaceBuildingObject = false;
    [Space(5)]
    public bool enoughItemsToBuild = false;
    public bool isColliding = false;
    public bool isCollidingWithBuildingBlock = false;
    public bool isTouchingABuildingBlock = false;
    #endregion


    //--------------------


    private void Start()
    {
        PlayerButtonManager.isPressed_MoveableRotation_Right += SetObjectRotation_Right;
        PlayerButtonManager.isPressed_MoveableRotation_Left += SetObjectRotation_Left;
    }
    private void Update()
    {
        Snapping_RaycastSettings();
        MoveWorldBuildingObject();

        Snapping_CheckRaycastHit();
    }


    //--------------------


    #region Save/Load
    public void LoadData()
    {
        //Set activeBuildingObject_Info
        activeBuildingObject_Info = DataManager.Instance.activeBuildingObject_Store;

        //Set worldBuildingObjectInfoList
        worldBuildingObjectInfoList = DataManager.Instance.worldBuildingObjectInfoList_Store;

        for (int i = 0; i < worldBuildingObjectInfoList.Count; i++)
        {
            if (worldBuildingObjectInfoList[i].buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
            {
                if (GetBuildingObjectInfo(worldBuildingObjectInfoList[i].buildingBlockObjectName_Active, worldBuildingObjectInfoList[i].buildingMaterial_Active).objectInfo.worldObject != null)
                {
                    worldBuildingObjectListSpawned.Add(Instantiate(GetBuildingObjectInfo(worldBuildingObjectInfoList[i].buildingBlockObjectName_Active, worldBuildingObjectInfoList[i].buildingMaterial_Active).objectInfo.worldObject) as GameObject);

                    worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.parent = worldObject_Parent.transform;
                    worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.SetPositionAndRotation(worldBuildingObjectInfoList[i].objectPos, worldBuildingObjectInfoList[i].objectRot);
                }
            }
            else if (worldBuildingObjectInfoList[i].buildingObjectType_Active == BuildingObjectTypes.Furniture)
            {
                if (GetBuildingObjectInfo(worldBuildingObjectInfoList[i].furnitureObjectName_Active).objectInfo.worldObject != null)
                {
                    worldBuildingObjectListSpawned.Add(Instantiate(GetBuildingObjectInfo(worldBuildingObjectInfoList[i].furnitureObjectName_Active).objectInfo.worldObject) as GameObject);

                    worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.parent = worldObject_Parent.transform;
                    worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.SetPositionAndRotation(worldBuildingObjectInfoList[i].objectPos, worldBuildingObjectInfoList[i].objectRot);
                }
            }
            else if (worldBuildingObjectInfoList[i].buildingObjectType_Active == BuildingObjectTypes.Machine)
            {
                if (GetBuildingObjectInfo(worldBuildingObjectInfoList[i].machineObjectName_Active).objectInfo.worldObject != null)
                {
                    worldBuildingObjectListSpawned.Add(Instantiate(GetBuildingObjectInfo(worldBuildingObjectInfoList[i].machineObjectName_Active).objectInfo.worldObject) as GameObject);

                    worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.parent = worldObject_Parent.transform;
                    worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.SetPositionAndRotation(worldBuildingObjectInfoList[i].objectPos, worldBuildingObjectInfoList[i].objectRot);
                }
            }
        }

        SpawnNewSelectedBuildingObject();

        SetupHammerDisplayScreen();

        SaveData();
    }
    public void SaveData()
    {
        DataManager.Instance.activeBuildingObject_Store = activeBuildingObject_Info;
        DataManager.Instance.worldBuildingObjectInfoList_Store = worldBuildingObjectInfoList;
    }

    #endregion


    //--------------------


    #region Object Spawning
    public void SpawnNewSelectedBuildingObject()
    {
        //Remove previous child
        RemoveSelectedBuildingObjectChild();

        //Get a new Child of the selected BuildingObject
        GameObject newObject = new GameObject();
        if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
        {
            newObject = Instantiate(GetBuildingObjectInfo(activeBuildingObject_Info.buildingBlockObjectName_Active, activeBuildingObject_Info.buildingMaterial_Active).objectInfo.worldObject);
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Furniture)
        {
            newObject = Instantiate(GetBuildingObjectInfo(activeBuildingObject_Info.furnitureObjectName_Active).objectInfo.worldObject);
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Machine)
        {
            newObject = Instantiate(GetBuildingObjectInfo(activeBuildingObject_Info.machineObjectName_Active).objectInfo.worldObject);
        }
        else
        {
            return;
        }

        //Set new Parent and Pos/Rot
        if (newObject)
        {
            newObject.transform.parent = WorldObjectGhost_Parent.transform;

            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;
        }

        //Set layer to be GhostLayer
        newObject.layer = layerMask_IgnoreRaycast;
        for (int i = 0; i < newObject.transform.childCount; i++)
        {
            newObject.transform.GetChild(i).gameObject.layer = layerMask_IgnoreRaycast;

            for (int j = 0; j < newObject.transform.GetChild(i).transform.childCount; j++)
            {
                newObject.transform.GetChild(i).transform.GetChild(j).gameObject.layer = layerMask_IgnoreRaycast;

                for (int k = 0; k < newObject.transform.GetChild(i).transform.GetChild(j).transform.childCount; k++)
                {
                    newObject.transform.GetChild(i).transform.GetChild(j).transform.GetChild(k).gameObject.layer = layerMask_IgnoreRaycast;
                }
            }
        }

        //Hide Colliders
        Destroy(newObject.GetComponent<MoveableObject>().collidersOnObject);

        //Make sure not getting errors when changing to Furniture or Machine Object
        isColliding = false;
    }
    void RemoveSelectedBuildingObjectChild()
    {
        for (int i = WorldObjectGhost_Parent.transform.childCount - 1; i >= 0; i--)
        {
            if (WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<InteractableObject>())
            {
                WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<InteractableObject>().DestroyThisinteractableObject();
            }
            else if (WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<MoveableObject>())
            {
                WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<MoveableObject>().DestroyThisMovableObject();
            }
            else if (WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<Model>())
            {
                if (WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<Model>().gameObject.transform.parent.gameObject.GetComponent<MoveableObject>())
                {
                    WorldObjectGhost_Parent.transform.GetChild(0).gameObject.GetComponent<Model>().gameObject.transform.parent.gameObject.GetComponent<MoveableObject>().DestroyThisMovableObject();
                }
            }
        }
    }
    void SetupHammerDisplayScreen()
    {
        if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
        {
            BuildingDisplayManager.Instance.UpdateSelectedDisplay(GetBuildingObjectInfo(activeBuildingObject_Info.buildingBlockObjectName_Active, activeBuildingObject_Info.buildingMaterial_Active));
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Furniture)
        {
            BuildingDisplayManager.Instance.UpdateSelectedDisplay(GetBuildingObjectInfo(activeBuildingObject_Info.furnitureObjectName_Active));
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Machine)
        {
            BuildingDisplayManager.Instance.UpdateSelectedDisplay(GetBuildingObjectInfo(activeBuildingObject_Info.machineObjectName_Active));
        }
        else
        {
            BuildingDisplayManager.Instance.ResetDisplay();
        }

        BuildingDisplayManager.Instance.UpdateScreenBuildingRequirementDisplayInfo();
        SpawnNewSelectedBuildingObject();

        SaveData();
    }
    #endregion


    //--------------------


    #region ObjectMovement
    public void MoveWorldBuildingObject()
    {
        if (WorldObjectGhost_Parent.transform.childCount > 0
            && MainManager.Instance.menuStates == MenuStates.None
            && (HotbarManager.Instance.selectedItem == Items.WoodBuildingHammer || HotbarManager.Instance.selectedItem == Items.StoneBuildingHammer || HotbarManager.Instance.selectedItem == Items.CryoniteBuildingHammer)
            && ghostObject_Holding)
        {
            //Change Position and Rotation
            SnappingSetup(); //If holding a BuildingBlock
            NotSnappingSetup(); //If holding a Furniture or Machine

            //Set New Material on all Materials of the Object
            SetObjectMaterial();
        }
        else
        {
            WorldObjectGhost_Parent.SetActive(false);
            ghostObject_Holding = GetBuildingObjectGhost();
        }
    }
    void SnappingSetup() //- Fixed
    {
        if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.BuildingBlock)
        {
            if (directionHit != BuildingBlockColliderDirection.None)
            {
                isSnapping = true;

                //Set new Position and Rotation of the Ghost
                ghostObject_Holding.transform.SetPositionAndRotation(Snapping_SetPosition(), Snapping_SetRotation(buildingBlockHit));

                WorldObjectGhost_Parent.SetActive(true);
                GetBuildingObjectGhost().SetActive(true);
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, PlayerManager.Instance.InteractableDistance + 1, layerMask_BuildingBlockModel))
                {
                    isTouchingABuildingBlock = true;
                    SetPosToGroundAndBuldingBlock();
                }
                else if (Physics.Raycast(ray, out hit, PlayerManager.Instance.InteractableDistance + 1, layerMask_Ground))
                {
                    isTouchingABuildingBlock = false;
                    SetPosToGroundAndBuldingBlock();
                }
                else
                {
                    isTouchingABuildingBlock = false;
                    ghostObject_Holding = GetBuildingObjectGhost();

                    WorldObjectGhost_Parent.SetActive(false);
                    GetBuildingObjectGhost().SetActive(false);
                }

                isSnapping = false;
            }
        }
    }
    void NotSnappingSetup() //- Fixed
    {
        if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.Furniture
            || ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.Machine)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, PlayerManager.Instance.InteractableDistance + 1, layerMask_BuildingBlockModel))
            {
                isTouchingABuildingBlock = true;
                SetPosToGroundAndBuldingBlock();
            }
            else if (Physics.Raycast(ray, out hit, PlayerManager.Instance.InteractableDistance + 1, layerMask_Ground))
            {
                isTouchingABuildingBlock = false;
                SetPosToGroundAndBuldingBlock();
            }
            else
            {
                isTouchingABuildingBlock = false;

                ghostObject_Holding = GetBuildingObjectGhost();

                WorldObjectGhost_Parent.SetActive(false);
                GetBuildingObjectGhost().SetActive(false);
            }

            isSnapping = false;
        }
    }

    void SetPosToGroundAndBuldingBlock() //- Fixed
    {
        WorldObjectGhost_Parent.SetActive(true);
        GetBuildingObjectGhost().SetActive(true);

        //Set the object's position to the ground height
        ghostObject_Holding = WorldObjectGhost_Parent.transform.GetChild(0).gameObject;

        if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
        {
            ghostObject_Holding.transform.SetPositionAndRotation(new Vector3(hit.point.x, hit.point.y + 1f, hit.point.z), MainManager.Instance.playerBody.transform.rotation * Quaternion.Euler(0, rotationValue + 0, 0));
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Furniture)
        {
            ghostObject_Holding.transform.SetPositionAndRotation(new Vector3(hit.point.x, hit.point.y, hit.point.z), MainManager.Instance.playerBody.transform.rotation * Quaternion.Euler(0, rotationValue + 0, 0));
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Machine)
        {
            ghostObject_Holding.transform.SetPositionAndRotation(new Vector3(hit.point.x, hit.point.y, hit.point.z), MainManager.Instance.playerBody.transform.rotation * Quaternion.Euler(0, rotationValue + 0, 0));
        }
    }
    
    void SetObjectMaterial() //- Fixed
    {
        for (int i = 0; i < ghostObject_Holding.GetComponent<MoveableObject>().modelList.Count; i++)
        {
            //MeshRenderer
            if (ghostObject_Holding.GetComponent<MoveableObject>().modelList[i].GetComponent<MeshRenderer>())
            {
                if (CanPlaceBuildingObject_Check())
                {
                    Materials_MeshRenderer_Can(i);
                }
                else
                {
                    Materials_MeshRenderer_Cannot(i);
                }
            }

            //SkinnedMeshRenderer
            else if (ghostObject_Holding.GetComponent<MoveableObject>().modelList[i].GetComponent<SkinnedMeshRenderer>())
            {
                if (CanPlaceBuildingObject_Check())
                {
                    Materials_SkinnedMeshRenderer_Can(i);
                }
                else
                {
                    Materials_SkinnedMeshRenderer_Cannot(i);
                }
            }
        }
        #endregion
    }
    void Materials_MeshRenderer_Can(int index)
    {
        Material[] materials = ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<MeshRenderer>().materials;

        for (int j = 0; j < materials.Length; j++)
        {
            materials[j] = canPlace_Material;
        }

        ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<MeshRenderer>().materials = materials;
    }
    void Materials_MeshRenderer_Cannot(int index)
    {
        Material[] materials = ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<MeshRenderer>().materials;

        for (int j = 0; j < materials.Length; j++)
        {
            materials[j] = cannotPlace_Material;
        }

        ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<MeshRenderer>().materials = materials;
    }
    void Materials_SkinnedMeshRenderer_Can(int index)
    {
        Material[] materials = ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<SkinnedMeshRenderer>().materials;

        for (int j = 0; j < materials.Length; j++)
        {
            materials[j] = canPlace_Material;
        }

        ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<SkinnedMeshRenderer>().materials = materials;
    }
    void Materials_SkinnedMeshRenderer_Cannot(int index)
    {
        Material[] materials = ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<SkinnedMeshRenderer>().materials;

        for (int j = 0; j < materials.Length; j++)
        {
            materials[j] = cannotPlace_Material;
        }

        ghostObject_Holding.GetComponent<MoveableObject>().modelList[index].GetComponent<SkinnedMeshRenderer>().materials = materials;
    }

    public bool CanPlaceBuildingObject_Check() //- Fixed
    {
        //BuildingBlock
        if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.BuildingBlock)
        {
            if (enoughItemsToBuild && !isColliding && !isCollidingWithBuildingBlock && !isTouchingABuildingBlock)
            {
                canPlaceBuildingObject = true;
                return true;
            }
            else
            {
                canPlaceBuildingObject = false;
                return false;
            }
        }

        //Furniture
        else if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.Furniture)
        {
            if (enoughItemsToBuild && !isColliding && isTouchingABuildingBlock)
            {
                canPlaceBuildingObject = true;
                return true;
            }
            else
            {
                canPlaceBuildingObject = false;
                return false;
            }
        }

        //Machine
        else if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.Machine)
        {
            if (enoughItemsToBuild && !isColliding && isTouchingABuildingBlock)
            {
                canPlaceBuildingObject = true;
                return true;
            }
            else
            {
                canPlaceBuildingObject = false;
                return false;
            }
        }

        return false;
    }


    //--------------------


    #region Snapping
    void Snapping_RaycastSettings()
    {
        //Only run RayCast when "BuildingHammer" is in the hand
        if ((HotbarManager.Instance.selectedItem == Items.WoodBuildingHammer
            || HotbarManager.Instance.selectedItem == Items.StoneBuildingHammer
            || HotbarManager.Instance.selectedItem == Items.CryoniteBuildingHammer)
            && MainManager.Instance.menuStates == MenuStates.None
            && ghostObject_Holding)
        {
            if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.BuildingBlock)
            {
                Snapping_Raycast();
            }
            else
            {
                isSnapping = false;
            }
        }
        else
        {
            isSnapping = false;
        }
    }
    public void Snapping_Raycast()
    {
        // Cast a Ray from the mouse position to the world space
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Make 5 Raycasts (Forward, Up-Forward, Down-Forward, Right-Forward, Left-Forward)
        Snapping_RaycastDirection(RaycastDirections.Right, Color.blue);
        Snapping_RaycastDirection(RaycastDirections.Left, Color.blue);
        Snapping_RaycastDirection(RaycastDirections.Down, Color.cyan);
        Snapping_RaycastDirection(RaycastDirections.Up, Color.cyan);
        Snapping_RaycastDirection(RaycastDirections.Normal, Color.green);

        #region Update which BuildingBlock to look at
        if (hitTransform != null)
        {
            if (hitTransform.gameObject.GetComponent<BuildingBlockDirection>())
            {
                //If looking directly at a BuildingBlock
                if ((BB_Normal != null))
                {
                    //Get the direction of the object looking at
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Normal();
                }

                else if (BB_Normal == null && BB_Up != null)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Up();
                }
                else if (BB_Normal == null && BB_Down != null)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Down();
                }

                //If Left/Right looks at the BuildingBlock with a smaller Angle
                else if (BB_Normal == null && BB_Left != null && rayHitAngle_Left <= 125)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Normal();
                }
                else if (BB_Normal == null && BB_Right != null && rayHitAngle_Right <= 125)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Normal();
                }

                //If Left/Right + Top looks at the BuildingBlock
                else if (BB_Normal == null && BB_Left != null && BB_Up != null && BB_Left != BB_Up)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Normal();
                }
                else if (BB_Normal == null && BB_Right != null && BB_Up != null && BB_Right != BB_Up)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Normal();
                }

                //If only 1 Ray looks at the BuildingBlock
                else if (BB_Normal == null && BB_Right != null)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Right();
                }
                else if (BB_Normal == null && BB_Left != null)
                {
                    hitTransform.gameObject.GetComponent<BuildingBlockDirection>().EnterBlockDirection_BB_Left();
                }

                //If not looking at a BuildingBlock at all
                else
                {
                    directionHit = BuildingBlockColliderDirection.None;
                    buildingBlockHit = null;
                }
            }
        }

        //If not looking at a BuildingBlock at all
        else
        {
            directionHit = BuildingBlockColliderDirection.None;
            buildingBlockHit = null;
        }
        #endregion
    }
    void Snapping_RaycastDirection(RaycastDirections _raycastDirection, Color color)
    {
        Vector3 direction = Vector3.zero;

        //Get the correct Direction
        switch (_raycastDirection)
        {
            case RaycastDirections.None:
                direction = Vector3.zero;
                break;

            case RaycastDirections.Normal:
                direction = Vector3.zero;
                break;
            case RaycastDirections.Up:
                direction = MainManager.Instance.mainMainCamera.transform.up;
                break;
            case RaycastDirections.Down:
                direction = -MainManager.Instance.mainMainCamera.transform.up;
                break;
            case RaycastDirections.Left:
                direction = -MainManager.Instance.mainMainCamera.transform.right;
                break;
            case RaycastDirections.Right:
                direction = MainManager.Instance.mainMainCamera.transform.right;
                break;

            default:
                direction = Vector3.zero;
                break;
        }

        Vector3 startPoint = ray.origin + direction /** 0.5f*/;

        Debug.DrawRay(startPoint, MainManager.Instance.mainMainCamera.transform.forward * (PlayerManager.Instance.InteractableDistance + 1), color);

        if (Physics.Raycast(startPoint, MainManager.Instance.mainMainCamera.transform.forward * (PlayerManager.Instance.InteractableDistance + 1), out rayHit, (PlayerManager.Instance.InteractableDistance + 1), layerMask_BuildingBlock))
        {
            //Get the Transform of GameObject hit
            hitTransform = rayHit.transform;

            //Get the Object looking at
            if (hitTransform.gameObject.GetComponent<BuildingBlockDirection>())
            {
                switch (_raycastDirection)
                {
                    case global::RaycastDirections.None:
                        BB_Normal = hitTransform.gameObject;
                        rayHitAngle_Normal = Vector3.Angle(ray.direction, rayHit.normal);
                        break;

                    case global::RaycastDirections.Normal:
                        BB_Normal = hitTransform.gameObject;
                        rayHitAngle_Normal = Vector3.Angle(ray.direction, rayHit.normal);
                        break;
                    case global::RaycastDirections.Up:
                        BB_Up = hitTransform.gameObject;
                        rayHitAngle_Up = Vector3.Angle(ray.direction, rayHit.normal);
                        break;
                    case global::RaycastDirections.Down:
                        BB_Down = hitTransform.gameObject;
                        rayHitAngle_Down = Vector3.Angle(ray.direction, rayHit.normal);
                        break;
                    case global::RaycastDirections.Left:
                        BB_Right = hitTransform.gameObject;
                        rayHitAngle_Right = Vector3.Angle(ray.direction, rayHit.normal);
                        break;
                    case global::RaycastDirections.Right:
                        BB_Left = hitTransform.gameObject;
                        rayHitAngle_Left = Vector3.Angle(ray.direction, rayHit.normal);
                        break;

                    default:
                        BB_Normal = hitTransform.gameObject;
                        rayHitAngle_Normal = Vector3.Angle(ray.direction, rayHit.normal);
                        break;
                }
            }
        }

        //Looking at something not a BuildingBlock
        else if (Physics.Raycast(startPoint, Camera.main.transform.forward * (PlayerManager.Instance.InteractableDistance + 1), out rayHit, (PlayerManager.Instance.InteractableDistance + 1)))
        {
            switch (_raycastDirection)
            {
                case global::RaycastDirections.None:
                    BB_Normal = null;
                    rayHitAngle_Normal = 0;
                    break;

                case global::RaycastDirections.Normal:
                    rayHitAngle_Normal = 0;
                    BB_Normal = null;
                    break;
                case global::RaycastDirections.Up:
                    rayHitAngle_Up = 0;
                    BB_Up = null;
                    break;
                case global::RaycastDirections.Down:
                    rayHitAngle_Down = 0;
                    BB_Down = null;
                    break;
                case global::RaycastDirections.Left:
                    rayHitAngle_Right = 0;
                    BB_Right = null;
                    break;
                case global::RaycastDirections.Right:
                    BB_Left = null;
                    rayHitAngle_Left = 0;
                    break;

                default:
                    BB_Normal = null;
                    rayHitAngle_Normal = 0;
                    break;
            }
        }

        //Looking at Nothing (out in the thin air)
        else
        {
            switch (_raycastDirection)
            {
                case global::RaycastDirections.None:
                    BB_Normal = null;
                    rayHitAngle_Normal = 0;
                    break;

                case global::RaycastDirections.Normal:
                    BB_Normal = null;
                    rayHitAngle_Normal = 0;
                    break;
                case global::RaycastDirections.Up:
                    BB_Up = null;
                    rayHitAngle_Up = 0;
                    break;
                case global::RaycastDirections.Down:
                    BB_Down = null;
                    rayHitAngle_Down = 0;
                    break;
                case global::RaycastDirections.Left:
                    BB_Right = null;
                    rayHitAngle_Right = 0;
                    break;
                case global::RaycastDirections.Right:
                    BB_Left = null;
                    rayHitAngle_Left = 0;
                    break;

                default:
                    BB_Normal = null;
                    rayHitAngle_Normal = 0;
                    break;
            }
        }
    }

    Vector3 Snapping_SetPosition()
    {
        switch (directionHit)
        {
            case BuildingBlockColliderDirection.None:
                break;

            case BuildingBlockColliderDirection.Front:
                return buildingBlockHit.transform.position + buildingBlockHit.transform.forward * 2;

            case BuildingBlockColliderDirection.Back:
                return buildingBlockHit.transform.position - buildingBlockHit.transform.forward * 2;

            case BuildingBlockColliderDirection.Up:
                return buildingBlockHit.transform.position + buildingBlockHit.transform.up * 2;

            case BuildingBlockColliderDirection.Down:
                return buildingBlockHit.transform.position - buildingBlockHit.transform.up * 2;

            case BuildingBlockColliderDirection.Right:
                return buildingBlockHit.transform.position + buildingBlockHit.transform.right * 2;

            case BuildingBlockColliderDirection.Left:
                return buildingBlockHit.transform.position - buildingBlockHit.transform.right * 2;

            default:
                break;
        }

        return Vector3.zero;
    }
    Quaternion Snapping_SetRotation(GameObject _object)
    {
        //If a BuildingBlock
        if (ghostObject_Holding.GetComponent<MoveableObject>().buildingObjectType == BuildingObjectTypes.BuildingBlock)
        {
            switch (ghostObject_Holding.GetComponent<MoveableObject>().buildingBlockObjectName)
            {
                case BuildingBlockObjectNames.None:
                    return _object.transform.rotation;

                //Floor
                case BuildingBlockObjectNames.Floor_Square:
                    return _object.transform.rotation;
                case BuildingBlockObjectNames.Floor_Triangle:
                    return _object.transform.rotation;

                //Wall
                case BuildingBlockObjectNames.Wall:
                    return SnapRotation_Wall(_object);
                case BuildingBlockObjectNames.Wall_Triangle:
                    return SnapRotation_Wall(_object);
                case BuildingBlockObjectNames.Wall_Diagonal:
                    break;
                case BuildingBlockObjectNames.Wall_Window:
                    return SnapRotation_Wall(_object);
                case BuildingBlockObjectNames.Wall_Door:
                    return SnapRotation_Wall(_object);
                case BuildingBlockObjectNames.Fence:
                    return SnapRotation_Wall(_object);
                case BuildingBlockObjectNames.Fence_Diagonal:
                    break;

                //Ramp
                case BuildingBlockObjectNames.Ramp_Stair:
                    break;
                case BuildingBlockObjectNames.Ramp_Ramp:
                    break;
                case BuildingBlockObjectNames.Ramp_Triangle:
                    break;
                case BuildingBlockObjectNames.Ramp_Corner:
                    break;

                default:
                    break;
            }
        }
        

        return _object.transform.rotation;
    }
    Quaternion SnapRotation_Wall(GameObject _object)
    {
        switch (directionHit)
        {
            case BuildingBlockColliderDirection.None:
                break;

            case BuildingBlockColliderDirection.Front:
                return _object.transform.rotation * Quaternion.Euler(0, 0, 0);
            case BuildingBlockColliderDirection.Back:
                return _object.transform.rotation * Quaternion.Euler(0, 180, 0);
            case BuildingBlockColliderDirection.Up:
                break;
            case BuildingBlockColliderDirection.Down:
                break;
            case BuildingBlockColliderDirection.Left:
                return _object.transform.rotation * Quaternion.Euler(0, 270, 0);
            case BuildingBlockColliderDirection.Right:
                return _object.transform.rotation * Quaternion.Euler(0, 90, 0);

            default:
                break;
        }

        return _object.transform.rotation;
    }

    GameObject GetBuildingObjectGhost()
    {
        if (WorldObjectGhost_Parent.transform.childCount > 0)
        {
            return WorldObjectGhost_Parent.transform.GetChild(0).gameObject;
        }

        return null;
    }

    void Snapping_CheckRaycastHit()
    {
        if (rayHitAngle_Normal == 0 && rayHitAngle_Left == 0 && rayHitAngle_Right == 0 && rayHitAngle_Up == 0 && rayHitAngle_Down == 0)
        {
            directionHit = BuildingBlockColliderDirection.None;
        }
    }
    #endregion


    //--------------------

    //Object Rotation
    #region - Fixed
    void SetObjectRotation_Right()
    {
        rotationValue += rotationSpeed * Time.deltaTime;
    }
    void SetObjectRotation_Left()
    {
        rotationValue -= rotationSpeed * Time.deltaTime;
    }
    #endregion


    //--------------------


    //Add/Remove BuildingBlocks
    #region - Fixed
    public void PlaceWorldBuildingObject()
    {
        if (WorldObjectGhost_Parent.activeInHierarchy && WorldObjectGhost_Parent.transform.childCount > 0
            && MainManager.Instance.menuStates == MenuStates.None
            && (HotbarManager.Instance.selectedItem == Items.WoodBuildingHammer || HotbarManager.Instance.selectedItem == Items.StoneBuildingHammer || HotbarManager.Instance.selectedItem == Items.CryoniteBuildingHammer))
        {
            //Check if requirements are met (resources) - Use the variable "enoughItemsToBuild" instead of "true"
            if (canPlaceBuildingObject)
            {
                //Spawn BuildingBlock
                if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
                {
                    print("Place BuildingBlock Object");
                    #region Play Sound
                    if (activeBuildingObject_Info.buildingMaterial_Active == BuildingMaterial.Wood)
                        SoundManager.Instance.Play_Building_Place_Wood_Clip();
                    else if (activeBuildingObject_Info.buildingMaterial_Active == BuildingMaterial.Stone)
                        SoundManager.Instance.Play_Building_Place_Stone_Clip();
                    else if (activeBuildingObject_Info.buildingMaterial_Active == BuildingMaterial.Cryonite)
                        SoundManager.Instance.Play_Building_Place_Cryonite_Clip();
                    #endregion

                    worldBuildingObjectListSpawned.Add(Instantiate(GetBuildingObjectInfo(activeBuildingObject_Info.buildingBlockObjectName_Active, activeBuildingObject_Info.buildingMaterial_Active).objectInfo.worldObject) as GameObject);
                }

                //Spawn Furniture
                else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Furniture)
                {
                    print("Place Furniture Object");
                    #region Play Sound
                    SoundManager.Instance.Play_Building_Place_MoveableObject_Clip();
                    #endregion

                    worldBuildingObjectListSpawned.Add(Instantiate(GetBuildingObjectInfo(activeBuildingObject_Info.furnitureObjectName_Active).objectInfo.worldObject) as GameObject);
                }

                //Spawn Machine
                else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Machine)
                {
                    print("Place Machine Object");
                    #region Play Sound
                    SoundManager.Instance.Play_Building_Place_MoveableObject_Clip();
                    #endregion

                    worldBuildingObjectListSpawned.Add(Instantiate(GetBuildingObjectInfo(activeBuildingObject_Info.machineObjectName_Active).objectInfo.worldObject) as GameObject);
                }

                //Set new Parent
                worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.parent = worldObject_Parent.transform;

                //Set position and Rotation to be the same as the Ghost
                worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.SetPositionAndRotation(ghostObject_Holding.transform.position, ghostObject_Holding.transform.rotation);

                //Remove Building Items from inventory
                RemoveItemsFromInventoryAfterPlacingObject();

                //Add the ObjectInfo to the List
                AddBuildingObjectInfoToList();
            }
            else
            {
                SoundManager.Instance.Play_Building_CannotPlaceBlock_Clip();
            }
        }
        else
        {
            SoundManager.Instance.Play_Building_CannotPlaceBlock_Clip();
        }
    }
    void AddBuildingObjectInfoToList()
    {
        //Add Info to saveList
        if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
        {
            BuildingBlockInfo buildingBlockInfo = GetBuildingObjectInfo(activeBuildingObject_Info.buildingBlockObjectName_Active, activeBuildingObject_Info.buildingMaterial_Active);
            WorldBuildingObject worldBuildingObject = new WorldBuildingObject();

            worldBuildingObject.objectPos = worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.position;
            worldBuildingObject.objectRot = worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.rotation;

            worldBuildingObject.buildingObjectType_Active = buildingBlockInfo.buildingObjectType;
            worldBuildingObject.buildingMaterial_Active = buildingBlockInfo.buildingMaterial;

            worldBuildingObject.buildingBlockObjectName_Active = buildingBlockInfo.blockName;

            worldBuildingObjectInfoList.Add(worldBuildingObject);
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Furniture)
        {
            FurnitureInfo buildingBlockInfo = GetBuildingObjectInfo(activeBuildingObject_Info.furnitureObjectName_Active);
            WorldBuildingObject worldFurnitureObject = new WorldBuildingObject();

            worldFurnitureObject.objectPos = worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.position;
            worldFurnitureObject.objectRot = worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.rotation;

            worldFurnitureObject.buildingObjectType_Active = buildingBlockInfo.buildingObjectType;

            worldFurnitureObject.furnitureObjectName_Active = buildingBlockInfo.furnitureName;

            worldBuildingObjectInfoList.Add(worldFurnitureObject);
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Machine)
        {
            MachineInfo buildingBlockInfo = GetBuildingObjectInfo(activeBuildingObject_Info.machineObjectName_Active);
            WorldBuildingObject worldMachineObject = new WorldBuildingObject();

            worldMachineObject.objectPos = worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.position;
            worldMachineObject.objectRot = worldBuildingObjectListSpawned[worldBuildingObjectListSpawned.Count - 1].transform.rotation;

            worldMachineObject.buildingObjectType_Active = buildingBlockInfo.buildingObjectType;

            worldMachineObject.machineObjectName_Active = buildingBlockInfo.machinesName;

            worldBuildingObjectInfoList.Add(worldMachineObject);
        }

        SaveData();
    }
    void RemoveItemsFromInventoryAfterPlacingObject()
    {
        List<CraftingRequirements> resources = new List<CraftingRequirements>();

        if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.BuildingBlock)
        {
            resources = GetBuildingObjectInfo(activeBuildingObject_Info.buildingBlockObjectName_Active, activeBuildingObject_Info.buildingMaterial_Active).objectInfo.buildingRequirements;
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Furniture)
        {
            resources = GetBuildingObjectInfo(activeBuildingObject_Info.furnitureObjectName_Active).objectInfo.buildingRequirements;
        }
        else if (activeBuildingObject_Info.buildingObjectType_Active == BuildingObjectTypes.Machine)
        {
            resources = GetBuildingObjectInfo(activeBuildingObject_Info.machineObjectName_Active).objectInfo.buildingRequirements;
        }

        for (int i = 0; i < resources.Count; i++)
        {
            for (int j = 0; j < resources[i].amount; j++)
            {
                InventoryManager.Instance.RemoveItemFromInventory(0, resources[i].itemName, -1, false);
            }
        }
    }
    
    public void RemoveWorldBuildingObject(MoveableObject buildingObject)
    {
        for (int i = 0; i < worldBuildingObjectListSpawned.Count; i++)
        {
            if (buildingObject.gameObject == worldBuildingObjectListSpawned[i])
            {
                worldBuildingObjectInfoList.RemoveAt(i);

                if (worldBuildingObjectListSpawned[i].GetComponent<InteractableObject>())
                {
                    worldBuildingObjectListSpawned[i].GetComponent<InteractableObject>().DestroyThisinteractableObject();
                }
                else
                {
                    Destroy(worldBuildingObjectListSpawned[i]);
                }

                worldBuildingObjectListSpawned.RemoveAt(i);

                break;
            }
        }

        SaveData();
    }
    #endregion


    //--------------------


    //Get BuildingObjectInfo
    #region - Fixed
    public BuildingBlockInfo GetBuildingObjectInfo(BuildingBlockObjectNames objectName, BuildingMaterial buildingMaterial)
    {
        for (int i = 0; i < buildingBlocks_SO.buildingBlockObjectsList.Count; i++)
        {
            if (buildingBlocks_SO.buildingBlockObjectsList[i].blockName == objectName
                && buildingBlocks_SO.buildingBlockObjectsList[i].buildingMaterial == buildingMaterial)
            {
                return buildingBlocks_SO.buildingBlockObjectsList[i];
            }
        }

        return null;
    }
    public FurnitureInfo GetBuildingObjectInfo(FurnitureObjectNames objectName)
    {
        for (int i = 0; i < furniture_SO.furnitureObjectsList.Count; i++)
        {
            if (furniture_SO.furnitureObjectsList[i].furnitureName == objectName)
            {
                return furniture_SO.furnitureObjectsList[i];
            }
        }

        return null;
    }
    public MachineInfo GetBuildingObjectInfo(MachineObjectNames objectName)
    {
        for (int i = 0; i < machines_SO.machineObjectsList.Count; i++)
        {
            if (machines_SO.machineObjectsList[i].machinesName == objectName)
            {
                return machines_SO.machineObjectsList[i];
            }
        }

        return null;
    }
    #endregion

}

#region Classes
[Serializable]
public class ActiveBuildingObject
{
    public BuildingObjectTypes buildingObjectType_Active;
    public BuildingMaterial buildingMaterial_Active;
    [Space(5)]
    public BuildingBlockObjectNames buildingBlockObjectName_Active;
    public FurnitureObjectNames furnitureObjectName_Active;
    public MachineObjectNames machineObjectName_Active;
}

[Serializable]
public class WorldBuildingObject
{
    public Vector3 objectPos;
    public Quaternion objectRot;

    public BuildingObjectTypes buildingObjectType_Active;
    public BuildingMaterial buildingMaterial_Active;
    [Space(5)]
    public BuildingBlockObjectNames buildingBlockObjectName_Active;
    public FurnitureObjectNames furnitureObjectName_Active;
    public MachineObjectNames machineObjectName_Active;
}
#endregion

#region Enums
public enum BuildingObjectTypes
{
    None,

    BuildingBlock,
    Furniture,
    Machine
}

public enum BuildingMaterial
{
    None,

    Wood,
    Stone,
    Cryonite
}

public enum BuildingBlockObjectNames
{
    None,

    Floor_Square,
    Floor_Triangle,

    Wall,
    Wall_Triangle,
    Wall_Diagonal,
    Wall_Window,
    Wall_Door,

    Fence,
    Fence_Diagonal,

    Ramp_Stair,
    Ramp_Ramp,
    Ramp_Triangle,
    Ramp_Corner
}
public enum FurnitureObjectNames
{
    [Description("None")][InspectorName("None")] None,

    [Description("Crafting Table")][InspectorName("Crafting Table")] CraftingTable,
    [Description("Research Table")][InspectorName("Research Table")] ResearchTable,
    [Description("Skill Table")][InspectorName("Skill Table")] SkillTreeTable,

    [Description("Chest Small")][InspectorName("Chest Small")] Chest_Small,
    [Description("Chest Medium")][InspectorName("Chest Medium")] Chest_Medium,
    [Description("Chest Big")][InspectorName("Chest Big")] Chest_Big,

    [Description("Lamp Area")][InspectorName("Lamp Area")] Lamp_Area,
    [Description("Lamp Spot")][InspectorName("Lamp Spot")] Lamp_Spot,
    [Description("Lamp Ar�dia Area")][InspectorName("Lamp Ar�dia Area")] Lamp_Ar�dia_Area,
    [Description("Lamp Ar�dia Spot")][InspectorName("Lamp Ar�dia Spot")] Lamp_Ar�dia_Spot,

    [Description("Other1")][InspectorName("Other1")] FU_Other1,
    [Description("Other2")][InspectorName("Other2")] FU_Other2,
    [Description("Other3")][InspectorName("Other3")] FU_Other3,
    [Description("Other4")][InspectorName("Other4")] FU_Other4,
    [Description("Other5")][InspectorName("Other5")] FU_Other5
}
public enum MachineObjectNames
{
    [Description("None")][InspectorName("None")] None,

    [Description("Ghost Tank")][InspectorName("Ghost Tank")] GhostTank,
    [Description("Energy Storage Tank")][InspectorName("Energy Storage Tank")] EnergyStorageTank,
    [Description("Ghost Repeller")][InspectorName("Ghost Repeller")] GhostRepeller,

    [Description("Extractor")][InspectorName("Extractor")] Extractor,
    [Description("Heat Regulator")][InspectorName("Heat Regulator")] HeatRegulator,
    [Description("Resource Converter")][InspectorName("Resource Converter")] ResourceConverter,
    [Description("Blender")][InspectorName("Blender")] Blender,

    [Description("Crop Plot Small")][InspectorName("Crop Plot Small")] CropPlot_Small,
    [Description("Crop Plot Medium")][InspectorName("Crop Plot Medium")] CropPlot_Medium,
    [Description("Crop Plot Big")][InspectorName("Crop Plot Big")] CropPlot_Big,

    [Description("Grill Small")][InspectorName("Grill Small")] Grill_Small,
    [Description("Grill Medium")][InspectorName("Grill Medium")] Grill_Medium,
    [Description("Grill Big")][InspectorName("Grill Big")] Grill_Big,

    [Description("Battery Small")][InspectorName("Battery Small")] Battery_Small,
    [Description("Battery Medium")][InspectorName("Battery Medium")] Battery_Medium,
    [Description("Battery Big")][InspectorName("Battery Big")] Battery_Big,

    [Description("Other1")][InspectorName("Other1")] MA_Other1,
    [Description("Other2")][InspectorName("Other2")] MA_Other2,
    [Description("Other3")][InspectorName("Other3")] MA_Other3,
    [Description("Other4")][InspectorName("Other4")] MA_Other4,
    [Description("Other5")][InspectorName("Other5")] MA_Other5
}
public enum RaycastDirections
{
    None,

    Normal,
    Up,
    Down,
    Left,
    Right
}

public enum BuildingBlockColliderDirection
{
    None,

    Front,
    Back,
    Up,
    Down,
    Left,
    Right
}
#endregion