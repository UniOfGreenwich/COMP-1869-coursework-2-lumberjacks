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
                    }
                    if(currentRoom == "RoomLoadingBay" && !visitedLoadingBay)
                    {
                        LoadingBayTutorial();
                    }
                    if(currentRoom == "RoomLumberYard" && !visitedLumberYard)
                    {
                        LumberYardTutorial();
                    }
                    if(currentRoom == "RoomStorage" && !visitedStorageRoom)
                    {
                        StorageRoomTutorial();
                    }
                    Stage1CompleteCheck();
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
                    Stage3();
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
            if(dialogueIndex == 1)
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
            }
            if(dialogueIndex == 2)
            {
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
        tutorialText.text = "Great! Now that we've talked to the customer, let's check the job board to see what materials we need to complete their order.";
        // jobBoard.SetActive(true); arrow pointing to job board
    }

    void StockMarketTutorial()
    {
        tutorialText.text = "Now that we know what materials we need, let's go to the computer to purchase some wood";
        //once computer is opened point to stock market app
        tutorialText.text = "Let's open the stock market app so we can gather enough wood to make the item";
        tutorialText.text = "The price of lumber changes every hour so be sure to check back often!";
        // once wood has been purchased
    }

    void ShopTutorial()
    {
        tutorialText.text = "Now that we have the materials we need to cut and assemble them! Let's buy a table saw, laser cutter and assembly machine from the shop app";
        // once machines have been purchased 
        tutorialText.text = "Great! You can also purchase new product designs and upgrades for the machines from the shop too!";
        currentStage = 3;
        // activate stage 3
    }

    void Stage2CompleteCheck()
    {
        
    }

    //using each machine
    void Stage3()
    {
        tutorialText.text = "Let's go to the storage room to collect the wood we just purchased from the stock market";
    }
}
