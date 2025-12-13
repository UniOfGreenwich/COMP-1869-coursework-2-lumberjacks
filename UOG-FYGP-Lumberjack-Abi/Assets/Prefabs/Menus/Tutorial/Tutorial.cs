using TMPro;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    //TODO: tutorial window should lock player movement and interaction until closed
    [SerializeField] private GameObject textPanel;
    private TMP_Text tutorialText;
    private GameObject taskPanel;
    public bool tutorialTextActive = false;
    private string currentRoom = "Workshop";

    [SerializeField] private bool tutorialCompleted = false;
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int dialogueIndex = 1;

    [Header("Door tutorial arrows")]
    [SerializeField] private GameObject doorLoadingBay;
    [SerializeField] private GameObject doorLumberYard;
    [SerializeField] private GameObject doorStorageRoom;

    [Header("Workshop re-entry Doors")]
    [SerializeField] private GameObject doorFromLoadingBay;
    [SerializeField] private GameObject doorFromLumberYard;
    [SerializeField] private GameObject doorFromStorageRoom;

    [Header("Customer tutorial arrows")]
    [SerializeField] private GameObject customer;
    [SerializeField] private GameObject jobBoard;

    //Stage 1 tasks
    private bool stage1IntroDialogueComplete = false;
    private bool visitedLoadingBay = false;
    private bool visitedLumberYard = false;
    private bool visitedStorageRoom = false;
    private bool stage1Complete = false;

    //stage 2 tasks
    private bool stage2IntroDialogueComplete = false;
    private bool visitedCustomer = false;
    private bool visitedJobBoard = false;
    private bool visitedStockMarket = false;
    private bool visitedShop = false;
    private bool stage2Complete = false;

    //stage 3 tasks
    private bool visitedStorageForWood = false;
    private bool visitedTableSaw = false;
    private bool visitedLaserCutter = false;
    private bool visitedAssemblyStation = false;
    private bool visitedLoadingBayForDelivery = false;
    private bool stage3Complete = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (tutorialCompleted)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
            tutorialText = textPanel.GetComponentInChildren<TMP_Text>();
            taskPanel = GetComponentInChildren<RectTransform>().Find("Panel_Tasks").gameObject;
            taskPanel.SetActive(false);
            doorLoadingBay.GetComponentInChildren<Button>(true).onClick.AddListener(LoadingBayEnter);
            doorLumberYard.GetComponentInChildren<Button>(true).onClick.AddListener(LumberYardEnter);
            doorStorageRoom.GetComponentInChildren<Button>(true).onClick.AddListener(StorageRoomEnter);
            doorFromLoadingBay.GetComponentInChildren<Button>(true).onClick.AddListener(() => { currentRoom = "Workshop";  if(!stage1Complete) Stage1CompleteCheck();});
            doorFromLumberYard.GetComponentInChildren<Button>(true).onClick.AddListener(() => { currentRoom = "Workshop"; if(!stage1Complete) Stage1CompleteCheck();});
            doorFromStorageRoom.GetComponentInChildren<Button>(true).onClick.AddListener(() => { currentRoom = "Workshop"; if(!stage1Complete) Stage1CompleteCheck();});
            Stage1Intro();
        }
    }

    void Update()
    {
        if(Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            if (tutorialTextActive)
            {
                dialogueIndex++;
                if (currentStage == 1)
                {
                    if(!stage1IntroDialogueComplete)
                    {
                        Stage1Intro();
                        return;
                    }
                    if(currentRoom == "RoomLoadingBay" && !visitedLoadingBay)
                    {
                        LoadingBayTutorial();
                        return;
                    }
                    if(currentRoom == "RoomLumberYard" && !visitedLumberYard)
                    {
                        LumberYardTutorial();
                        return;
                    }
                    if(currentRoom == "RoomStorage" && !visitedStorageRoom)
                    {
                        StorageRoomTutorial();
                        return;

                    }
                    if(currentRoom != "Workshop" && stage1Complete == false)
                    {
                        Stage1CompleteCheck();
                        return;
                    }
                }

                if (currentStage == 2)
                {
                    if(!stage2IntroDialogueComplete)
                    {
                        Stage2Intro();
                    }
                }

                if (currentStage == 3)
                {
                    if(!visitedStorageForWood)
                    {
                        StorageUITutorial();
                    }
                }
            }
        }
    }

    //introduction to rooms
    void Stage1Intro()
    {
        textPanel.SetActive(true);
        tutorialTextActive = true;

        if(stage1IntroDialogueComplete == false)
        {
            if(dialogueIndex == 1)
            {
                tutorialText.text = "Welcome to LumberJill's Carpenter Shop! Explore the workshop by clicking around, to go to another room walk over the yellow boxes on the floor.";
                return;
            }
            if (dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                stage1IntroDialogueComplete = true;
                dialogueIndex = 1;
            }
        }
    }

    void LoadingBayEnter()
    {
        if (currentStage == 1)
        {           
            currentRoom = "RoomLoadingBay";
            LoadingBayTutorial();
        }   
    }

    void LoadingBayTutorial()
    {
        if (currentStage == 1 && currentRoom == "RoomLoadingBay")
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the Loading Bay, where all the finished products are shipped out to customers.";     
            }
            if(dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedLoadingBay = true;
                doorLoadingBay.GetComponentInChildren<Button>().onClick.RemoveListener(LoadingBayEnter);
                dialogueIndex = 1;
                Stage1CompleteCheck();
            }
        }
    }

    void LumberYardEnter()
    {
        if (currentStage == 1)
        {
            currentRoom = "RoomLumberYard";
            LumberYardTutorial();
        }       
    }

    void LumberYardTutorial()
    {
        if (currentStage == 1 && currentRoom == "RoomLumberYard")
        {           
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the Lumber Yard, it used to be full of trees but Jack cut them all down in a rush trying to make too many products at once.";
            }
            if(dialogueIndex == 2)
            {
                tutorialText.text = "whilst we wait for our trees to grow back, I've brought my computer from my old investment banking job so we can buy wood from the stock market!";
            }
            if(dialogueIndex == 3)
            {
                tutorialText.text = "Once we start growing enough trees we can sell the excess wood on the stock market for a profit!";
            }
            if(dialogueIndex > 3)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedLumberYard = true;
                doorLumberYard.GetComponentInChildren<Button>().onClick.RemoveListener(LumberYardEnter);
                dialogueIndex = 1;
                Stage1CompleteCheck();
            }                    
        }        
    }

    void StorageRoomEnter()
    {
        if (currentStage == 1)
        {
            currentRoom = "RoomStorage";
            StorageRoomTutorial();
        }    
    }

    void StorageRoomTutorial()
    {
        if (currentStage == 1 && currentRoom == "RoomStorage")
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the Storage Room, where we keep all our inventory of raw materials and finished products.";     
            }
            if (dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedStorageRoom = true;
                doorStorageRoom.GetComponentInChildren<Button>().onClick.RemoveListener(StorageRoomEnter);
                dialogueIndex = 1;
                Stage1CompleteCheck();
            }
        }
    }

    void Stage1CompleteCheck()
    {
        if(stage1IntroDialogueComplete && visitedLoadingBay && visitedLumberYard && visitedStorageRoom)
        {
            if(dialogueIndex == 1 && currentRoom != "Workshop")
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Let's head back to the Workshop, now that we've seen all the rooms.";
            }
            if (dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                dialogueIndex = 1;
            }
            if (currentRoom == "Workshop")
            {
                stage1Complete = true;
                currentStage = 2;
                Stage2Intro();
            }
        }
    }

    //introduction to customers, shop and stock market
    void Stage2Intro()
    {
        if(currentStage == 2)
        {
            if(dialogueIndex == 1)
            {
                //customer walks in
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Oh look! A customer, let's talk to them and see what they want us to make for them.";
            }
            if (dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                stage2IntroDialogueComplete = true;
                dialogueIndex = 1;
            }
        }
    }
    void CustomerTutorial()
    {
        if(currentStage == 2)
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the customer's order, here you can see what they want, how much they are willing to pay and the deadline for when they need it by.";
                //TODO: make customer want a chair and will pay £50, deadline in 3 hours
            }
            if(dialogueIndex == 2)
            {
                //TODO: disable reject button for tutorial
                tutorialText.text = "Let's accept their order so we can get started!";
            }
            if (dialogueIndex > 2)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedCustomer = true;
                dialogueIndex = 1;
            }
        }
        
    }

    void JobBoardTutorial()
    {
        if(currentStage == 2)
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Great! Now that we've talked to the customer, let's check the job board to get the full details of the item we need to make for them.";
                // jobBoard.SetActive(true); arrow pointing to job board
            }
            if(dialogueIndex == 2)
            {
                tutorialText.text = "Here we can see the same information as the customer's order, but we can also see what star rating they will give us based on our current progress.";
            }
            if(dialogueIndex == 3)
            {
                tutorialText.text = "As we get more customers we can see all their orders on the job board and prioritise which ones to complete first based on their deadlines and payment amounts.";
            }
            if(dialogueIndex > 3)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedJobBoard = true;
                dialogueIndex = 1;
            }
        }
    }

    void ShopTutorial()
    {
        if(currentStage == 2)
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Now that we know what item we need to make, let's head to the shop on the computer to buy the blueprints and machines we need.";
            }
            if(dialogueIndex == 2)
            {
                tutorialText.text = "first, click on the shop icon to open the app";
            }
            if(dialogueIndex == 3)
            {
                tutorialText.text = "There's two tabs, let's go to the blueprints tab first to purchase the design for the chair";
            }
            if(dialogueIndex == 4)
            {
                tutorialText.text = "Great! Now let's go to the machines tab to purchase the machines we need to make the chair";
            }
            if(dialogueIndex == 5)
            {
                tutorialText.text = "We need a Table Saw to roughly cut the wood, a Laser Cutter to make more precise cuts and an assembly station to put it all together";
            }
            if(dialogueIndex == 6)
            {
                tutorialText.text = "As customers begin to ask for more complex items, we'll need to upgrade our machines so come back here often to check for new blueprints and machine upgrades!";
            }
            if(dialogueIndex > 6)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedShop = true;
                dialogueIndex = 1;
            }
        }
    }

    void StockMarketTutorial()
    {
        if(currentStage == 2)
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Now that we have the blueprints and machines we need, let's go to the stock market app to purchase some wood to make the chair.";
            }
            if(dialogueIndex == 2)
            {
                tutorialText.text = "Here we can see the price of lumber from the past 24 hours represented by a graph";
            }
            if(dialogueIndex == 3)
            {
                tutorialText.text = "each bar represents the price of lumber for that hour, green bars mean the price went up and red bars mean the price went down";
            }
            if(dialogueIndex == 4)
            {
                tutorialText.text = "To buy lumber, simply enter the amount you want to purchase and click the buy button.";
            }
            if(dialogueIndex == 5)
            {
                tutorialText.text = "we need [insert amount] of lumber to make the chair for our customer.";
            }
            if(dialogueIndex == 6)
            {
                tutorialText.text = "Once you've purchased the lumber, it will be delivered to our storage room so we can collect it from there.";
            }
            if (dialogueIndex == 7)
            {
                tutorialText.text = "The price changes every hour so be sure to check back often!";
            }
            if(dialogueIndex == 8)
            {
                tutorialText.text = "If you're smart you can buy lumber when the price is low and sell it back on the stock market when the price is high to make a profit!";
            }
            if(dialogueIndex > 8)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedStockMarket = true;
                dialogueIndex = 1;
            }
        }
    }

    void Stage2CompleteCheck()
    {
        if(stage2IntroDialogueComplete && visitedCustomer && visitedJobBoard && visitedShop && visitedStockMarket)
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Now that we have everything we need, let's go to the storage room to collect the wood we just purchased from the stock market.";
            }
            if (dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                dialogueIndex = 1;
            }
            if (currentRoom == "Workshop")
            {
                stage2Complete = true;
                currentStage = 3;
            }
        }
    }

    //using each machine
    void StorageUITutorial()
    {
        if (currentStage == 3)
        {
            if (dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "click on the storage crate in the right corner";
            }
            if (dialogueIndex == 2)
            {
                tutorialText.text = "Drag and drop the wood into your hot bar to collect it";
            }
            if (dialogueIndex == 3)
            {
                tutorialText.text = "Now that we have the wood, let's head to the table saw to start making the chair.";
            }
            if (dialogueIndex > 3)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedStorageForWood = true;
                dialogueIndex = 1;
            }
        }
    }

    void TableSawTutorial()
    {
        if (currentStage == 3)
        {
            if (dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the Table Saw, we use it to make rough cuts on the wood to get it closer to the shape we need.";
            }
            if (dialogueIndex == 2)
            {
                tutorialText.text = "To use the Table Saw, simply drag and drop the wood from your hot bar onto the machine.";
            }
            if (dialogueIndex == 3)
            {
                tutorialText.text = "Once the wood is on the machine, click the start button to begin cutting.";
            }
            if (dialogueIndex > 3)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedTableSaw = true;
                dialogueIndex = 1;
            }
        }
    }

    void LaserCutterTutorial()
    {
        if (currentStage == 3)
        {
            if (dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the Laser Cutter, we use it to make precise cuts on the wood to get it to the exact dimensions we need.";
            }
            if (dialogueIndex == 2)
            {
                tutorialText.text = "To use the Laser Cutter, simply drag and drop the wood from your hot bar onto the machine.";
            }
            if (dialogueIndex == 3)
            {
                tutorialText.text = "Once the wood is on the machine, click the start button to begin cutting.";
            }
            if (dialogueIndex > 3)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedLaserCutter = true;
                dialogueIndex = 1;
            }
        }
    }

    void AssemblyStationTutorial()
    {
        if (currentStage == 3)
        {
            if (dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "This is the Assembly Station, we use it to put all the pieces of the chair together.";
            }
            if (dialogueIndex == 2)
            {
                tutorialText.text = "To use the Assembly Station, simply drag and drop the cut pieces from your hot bar onto the machine.";
            }
            if (dialogueIndex == 3)
            {
                tutorialText.text = "Once all the pieces are on the machine, click the start button to begin assembling.";
            }
            if (dialogueIndex > 3)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedAssemblyStation = true;
                dialogueIndex = 1;
            }
        }
    }

    void LoadingBayDeliveryTutorial()
    {
        if (currentStage == 3)
        {
            if (dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Great! Now that we've finished the chair, let's head to the loading bay to deliver it to the customer.";
            }
            if (dialogueIndex == 2)
            {
                tutorialText.text = "Walk over to the delivery zone and click the deliver button to ship the chair to the customer.";
            }
            if (dialogueIndex > 2)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                visitedLoadingBayForDelivery = true;
                dialogueIndex = 1;
            }
        }
    }

    void Stage3CompleteCheck()
    {
        if(visitedStorageForWood && visitedTableSaw && visitedLaserCutter && visitedAssemblyStation && visitedLoadingBayForDelivery)
        {
            if(dialogueIndex == 1)
            {
                textPanel.SetActive(true);
                tutorialTextActive = true;
                tutorialText.text = "Congratulations! You've completed the tutorial and we are now ready to start taking on real customers and growing our carpenter shop!";
            }
            if (dialogueIndex > 1)
            {
                textPanel.SetActive(false);
                tutorialTextActive = false;
                dialogueIndex = 1;
                tutorialCompleted = true;
                gameObject.SetActive(false);
            }
        }
    }
}
