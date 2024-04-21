using System.ComponentModel;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    #region Variables
    [Header("Is the player in range?")]
    [HideInInspector] public bool playerInRange;

    [Header("Stats")]
    public Items itemName;
    public int amount;
    public int durability_Current;
    public InteracteableType interactableType;

    [Header("If Object is an Inventory")]
    public int inventoryIndex;

    [Header("If Object is a Plant")]
    public GameObject plantParent;

    [Header("If Object is a Journal Page")]
    public JournalMenuState journalType;
    public int journalPageIndex;

    [Header("If Object is a Blueprint")]
    public BuildingBlockObjectNames blueprint_BuildingBlock_Name;
    public BuildingMaterial buildingMaterial;
    [Space(5)]
    public FurnitureObjectNames blueprint_Furniture_Name;
    public MachineObjectNames blueprint_Machine_Name;

    //bool isHittingGround;
    #endregion


    //--------------------


    private void Start()
    {
        PlayerButtonManager.objectInterraction_isPressedDown += ObjectInteraction;

        //Add SphereCollider for the item
        Vector3 scale = gameObject.transform.lossyScale;
    }


    //--------------------


    void ObjectInteraction()
    {
        //if (MainManager.Instance.gameStates == GameStates.GameOver) { return; }

        if (SelectionManager.Instance.selectedObject)
        {
            if (SelectionManager.Instance.onTarget && SelectionManager.Instance.selectedObject == gameObject
            && MainManager.Instance.menuStates == MenuStates.None)
            {
                //If Object is an item
                #region
                if (interactableType == InteracteableType.Item)
                {
                    //print("Interact with a Pickup");

                    //Check If item can be added
                    for (int i = 0; i < amount; i++)
                    {
                        if (InventoryManager.Instance.AddItemToInventory(0, gameObject, false))
                        {
                            SoundManager.Instance.Play_Inventory_PickupItem_Clip();

                            //Remove Object from the worldObjectList
                            WorldObjectManager.Instance.WorldObject_SaveState_RemoveObjectFromWorld(gameObject);

                            //Destroy gameObject
                            DestroyThisObject();
                        }
                    }
                }
                #endregion

                //If Object is a PlantItem
                #region
                else if (interactableType == InteracteableType.Plant)
                {
                    print("Interract with a PlantItem");

                    //Pick the Plant
                    if (plantParent)
                    {
                        if (plantParent.GetComponent<Plant>() && !plantParent.GetComponent<Plant>().isPicked)
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                SoundManager.Instance.Play_Inventory_PickupItem_Clip();

                                //Check If item can be added
                                InventoryManager.Instance.AddItemToInventory(0, itemName);

                                plantParent.GetComponent<Plant>().PickPlant();
                            }
                        }
                    }
                }
                #endregion

                //If Object is an Inventory
                #region
                else if (interactableType == InteracteableType.Inventory)
                {
                    //print("Interract with an Inventory");

                    TabletManager.Instance.objectInteractingWith_Object = gameObject;

                    //Set Open Chest Animation
                    if (gameObject.GetComponent<Animations_Objects>())
                    {
                        gameObject.GetComponent<Animations_Objects>().StartAnimation();
                    }

                    //Open the chest Inventory
                    InventoryManager.Instance.chestInventoryOpen = inventoryIndex;
                    InventoryManager.Instance.PrepareInventoryUI(inventoryIndex, false); //Prepare Chest Inventory
                    TabletManager.Instance.chestInventory_Parent.GetComponent<RectTransform>().sizeDelta = InventoryManager.Instance.inventories[inventoryIndex].inventorySize * InventoryManager.Instance.cellsize;
                    TabletManager.Instance.chestInventory_Parent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(InventoryManager.Instance.cellsize, InventoryManager.Instance.cellsize);
                    TabletManager.Instance.chestInventory_Parent.SetActive(true);

                    InventoryManager.Instance.chestInventory_Fake_Parent.GetComponent<RectTransform>().sizeDelta = InventoryManager.Instance.inventories[inventoryIndex].inventorySize * InventoryManager.Instance.cellsize;
                    InventoryManager.Instance.chestInventory_Fake_Parent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(InventoryManager.Instance.cellsize, InventoryManager.Instance.cellsize);
                    InventoryManager.Instance.chestInventory_Fake_Parent.SetActive(true);

                    MainManager.Instance.menuStates = MenuStates.ChestMenu;
                    TabletManager.Instance.objectInteractingWith = ObjectInteractingWith.Chest;

                    InventoryManager.Instance.ClosePlayerInventory();
                    InventoryManager.Instance.OpenPlayerInventory();
                    TabletManager.Instance.OpenTablet(TabletMenuState.ChestInventory);
                }
                #endregion

                //If Object is a Crafting Table
                #region
                else if (interactableType == InteracteableType.CraftingTable)
                {
                    //print("Interract with a CraftingTable");

                    SoundManager.Instance.Play_InteractableObjects_OpenCraftingTable_Clip();

                    TabletManager.Instance.objectInteractingWith_Object = gameObject;

                    //Set Crafting Table Animation
                    if (gameObject.GetComponent<Animations_Objects>())
                    {
                        gameObject.GetComponent<Animations_Objects>().StartAnimation();
                    }

                    //Open the crafting menu
                    TabletManager.Instance.OpenTablet(TabletMenuState.CraftingTable);

                    TabletManager.Instance.objectInteractingWith = ObjectInteractingWith.CraftingTable;
                }
                #endregion

                //If Object is a Research Table
                #region
                else if (interactableType == InteracteableType.ResearchTable)
                {
                    //print("Interact with a Research Table");

                    SoundManager.Instance.Play_InteractableObjects_OpenResearchTable_Clip();

                    TabletManager.Instance.objectInteractingWith_Object = gameObject;

                    //Set Research Table Animation
                    if (gameObject.GetComponent<Animations_Objects>())
                    {
                        gameObject.GetComponent<Animations_Objects>().StartAnimation();
                    }

                    //Open the Research menu
                    TabletManager.Instance.OpenTablet(TabletMenuState.ResearchTable);

                    TabletManager.Instance.objectInteractingWith = ObjectInteractingWith.ResearchTable;
                }
                #endregion

                //If Object is a SkillTree
                #region
                else if (interactableType == InteracteableType.SkillTreeTable)
                {
                    //print("Interract with a SkillTree");

                    SoundManager.Instance.Play_InteractableObjects_OpenSkillTreeTable_Clip();

                    TabletManager.Instance.objectInteractingWith_Object = gameObject;

                    //Set SkillTree Animation
                    if (gameObject.GetComponent<Animations_Objects>())
                    {
                        gameObject.GetComponent<Animations_Objects>().StartAnimation();
                    }

                    //Open the crafting menu
                    TabletManager.Instance.OpenTablet(TabletMenuState.SkillTree);

                    TabletManager.Instance.objectInteractingWith = ObjectInteractingWith.SkillTree;
                }
                #endregion

                //If Object is a GhostTank
                #region
                else if (interactableType == InteracteableType.GhostTank)
                {
                    print("Interact with a GhostTank");

                    if (gameObject.GetComponent<GhostTank>())
                    {
                        gameObject.GetComponent<GhostTank>().InteractWithGhostTank();
                    }
                }
                #endregion

                //If Object is a JournalPage
                #region
                else if (interactableType == InteracteableType.JournalPage)
                {
                    print("Interact with a Journal Page");

                    JournalManager.Instance.AddJournalPageToList(journalType, journalPageIndex);

                    //Destroy gameObject
                    DestroyThisObject();
                }
                #endregion

                //If Object is a Blueprint
                #region
                else if (interactableType == InteracteableType.Blueprint)
                {
                    print("Interact with a Blueprint");

                    if (blueprint_BuildingBlock_Name != BuildingBlockObjectNames.None)
                    {
                        BlueprintManager.Instance.AddBlueprint(blueprint_BuildingBlock_Name, buildingMaterial);
                    }
                    else if (blueprint_Furniture_Name != FurnitureObjectNames.None)
                    {
                        BlueprintManager.Instance.AddBlueprint(blueprint_Furniture_Name);
                    }
                    else if (blueprint_Machine_Name != MachineObjectNames.None)
                    {
                        BlueprintManager.Instance.AddBlueprint(blueprint_Machine_Name);
                    }

                    //Destroy gameObject
                    DestroyThisObject();
                }
                #endregion
            }
        }
    }


    //--------------------


    private void OnCollisionEnter(Collision collision)
    {
        //Spawn on the gorund, if an item
        if (collision.gameObject.tag == "Ground" && interactableType == InteracteableType.Item)
        {
            //isHittingGround = true;
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        //If a player is entering the area
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        //If a player is exiting the area
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }


    //--------------------


    public void DestroyThisObject()
    {
        //Unsubscribe from Event
        PlayerButtonManager.objectInterraction_isPressedDown -= ObjectInteraction;

        Destroy(gameObject);
    }
}

public enum InteracteableType
{
    [Description("None")][InspectorName("None")] None,

    [Description("Item")][InspectorName("Item/Item")] Item,
    [Description("Inventory")][InspectorName("Inventory/Inventory")] Inventory,

    [Description("Crafting Table")][InspectorName("Furniture/Crafting Table")] CraftingTable,
    [Description("SkillTree Table")][InspectorName("Furniture/SkillTree Table")] SkillTreeTable,

    [Description("GhostTank")][InspectorName("Machine/GhostTank")] GhostTank,
    [Description("Extractor")][InspectorName("Machine/Extractor")] Extractor,
    [Description("Ghost Repeller")][InspectorName("Machine/Ghost Repeller")] GhostRepeller,
    [Description("Heat Regulator")][InspectorName("Machine/Heat Regulator")] HeatRegulator,
    [Description("Resource Converter")][InspectorName("Machine/Resource Converter")] ResourceConverter,

    [Description("CropPlot x1")][InspectorName("Machine/CropPlot x1")] CropPlot_x1,
    [Description("CropPlot x2")][InspectorName("Machine/CropPlot x2")] CropPlot_x2,
    [Description("CropPlot x4")][InspectorName("Machine/CropPlot x4")] CropPlot_x4,

    [Description("Grill x1")][InspectorName("Machine/Grill x1")] Grill_x1,
    [Description("Grill x2")][InspectorName("Machine/Grill x2")] Grill_x2,
    [Description("Grill x4")][InspectorName("Machine/Grill x4")] Grill_x4,

    [Description("Battery x1")][InspectorName("Buff/Battery x1")] Battery_x1,
    [Description("Battery x2")][InspectorName("Buff/Battery x2")] Battery_x2,
    [Description("Battery x3")][InspectorName("Buff/Battery x3")] Battery_x3,

    [Description("Plant")][InspectorName("Plant/Plant")] Plant,

    [Description("Blender")][InspectorName("Machine/Blender")] Blender,
    [Description("Energy Storage Tank")][InspectorName("Machine/Energy Storage Tank")] EnergyStorageTank,

    //Ores - Mining
    [Description("Tungsten Ore")][InspectorName("Ore/Tungsten Ore")] Tungsten_Ore,
    [Description("Gold Ore")][InspectorName("Ore/Gold Ore")] Gold_Ore,
    [Description("Stone Ore")][InspectorName("Ore/Stone Ore")] Stone_Ore,
    [Description("Cryonite Ore")][InspectorName("Ore/Cryonite Ore")] Cryonite_Ore,
    [Description("Magnetite Ore")][InspectorName("Ore/Magnetite Ore")] Magnetite_Ore,
    [Description("Viridian Ore")][InspectorName("Ore/Viridian Ore")] Viridian_Ore,
    [Description("Ar�dite Crystal Ore")][InspectorName("Ore/Ar�dite Crystal Ore")] Ar�diteCrystal_Ore,

    //Journal Pages
    [Description("Journal Page")][InspectorName("Journal Page/Journal Page")] JournalPage,


    [Description("Research Table")][InspectorName("Furniture/Research Table")] ResearchTable,

    //Tree Types
    [Description("Palm Tree")][InspectorName("Trees/Palm Tree")] Palm_Tree,
    [Description("Blood Tree")][InspectorName("Trees/Blood Tree")] BloodTree,
    [Description("Blood Tree Bush")][InspectorName("Trees/Blood Tree Bush")] BloodTreeBush,
    [Description("Tree 4")][InspectorName("Trees/Tree 4")] Tree_4,
    [Description("Tree 5")][InspectorName("Trees/Tree 5")] Tree_5,
    [Description("Tree 6")][InspectorName("Trees/Tree 6")] Tree_6,
    [Description("Tree 7")][InspectorName("Trees/Tree 7")] Tree_7,
    [Description("Tree 8")][InspectorName("Trees/Tree 8")] Tree_8,
    [Description("Tree 9")][InspectorName("Trees/Tree 9")] Tree_9,
    [Description("Cactus")][InspectorName("Trees/Cactus")] Cactus,

    //Ghost
    [Description("Ghost")][InspectorName("Ghost/Ghost")] Ghost,

    //Blueprint
    [Description("Blueprint")][InspectorName("Blueprint/Blueprint")] Blueprint,
}