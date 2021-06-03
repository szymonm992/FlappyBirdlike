using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;
    PlayerController pc;

    private const float CAMERA_SIZE = 5;
    public static float pipeSpawnTimer = 1f;

    [Header("--Pipes--")]
    public List<Pipe> all_possible_pipes;

    [Header("--Prefab--")]
    public GameObject _pipe;

    [Header("--Page--")]
    public GameObject initialPage, startPage, ingamePage, overPage;

    private List<Transform> all_pipes = new List<Transform>();

    private bool pauseGame = true;

    private GameState gameState;

    private List<int> highscores = new List<int>();

    private void Awake()
    {
        instance = this;
        //PlayerPrefs.DeleteAll();
    }

    private void Start()
    {
        pc = PlayerController.instance;
        UpdateGameState(GameState.None);
    }

    void Update()
    {
        SpawnPipes();
        PipesHandle();
    }

    #region GameStates
    public GameState GetGameState()
    {
        return gameState;
    }
    public void PlayGame()
    {
        UpdateGameState(GameState.Start);
    }
   
    public void StartingGame()
    {
        UpdateGameState(GameState.Start);
    }
    public void GameOver()
    {
        UpdateGameState(GameState.GameOver);
    } 

    public void QuitGame()
    {
        Application.Quit();
    }
    public void RunGame()
    {
        UpdateGameState(GameState.Running);
    }

    void UpdateGameState(GameState state)
    {
        gameState = state;
        switch (gameState)
        {
            case GameState.None:
                {
                    pauseGame = true;
                    initialPage.SetActive(true);
                    ingamePage.SetActive(false);
                    startPage.SetActive(false);
                    overPage.SetActive(false);
                }
                break;
            case GameState.Start:
                {
                    initialPage.SetActive(false);
                    ingamePage.SetActive(false);
                    startPage.SetActive(true);
                    overPage.SetActive(false);
                    pc.UpdateScores();

                    pauseGame = true;
                    DeleteAllPipes();
                }
                break;
            case GameState.Running:
                {
                    initialPage.SetActive(false);
                    ingamePage.SetActive(true);
                    startPage.SetActive(false);
                    overPage.SetActive(false);

                    pauseGame = false;
                }
                break;
            case GameState.GameOver:
                {
                    pauseGame = true;
                    SaveScores();
                    pc.CurrentScores = 0;
                    pc.UpdateScores();

                    initialPage.SetActive(false);
                    ingamePage.SetActive(false);
                    startPage.SetActive(false);
                    overPage.SetActive(true);
                }
                break;
                
        }
    }


    #endregion

   


    #region Highscores
    void SaveScores()
    {
        overPage.transform.GetChild(2).gameObject.GetComponent<Text>().text = "Scores: "+pc.CurrentScores;
        if(PlayerPrefs.HasKey("highscoreTbl"))
        {
            string hsString = PlayerPrefs.GetString("highscoreTbl");
            Highscores hs = JsonUtility.FromJson<Highscores>(hsString);
            if (hs != null)
            {
                highscores = hs.highscore_list;
            }
           
        }
        else
        {
            highscores = new List<int>();
        }
       



        if (highscores == null || highscores.Count == 0)
        {
            Debug.Log("The actual highscore does not exists yet, probably first attempt to play the game");
            highscores = new List<int>();
            highscores.Add(pc.CurrentScores);
            overPage.transform.GetChild(4).gameObject.SetActive(false);
        }
        else
        {
            bool beatRecord = CheckRecordPlace();

            if (beatRecord)
            {
                ReorganizeHighscore();
            }
            else
            {
                overPage.transform.GetChild(4).gameObject.SetActive(false);
            }
        }

        Highscores objectToSave = new Highscores { highscore_list = highscores };
        string json = JsonUtility.ToJson(objectToSave);
        PlayerPrefs.SetString("highscoreTbl", json);
        PlayerPrefs.Save();
        Debug.Log(PlayerPrefs.GetString("highscoreTbl"));
    }

    void BeatRecordCommunicate()
    {
        int msc = highscores.IndexOf(pc.CurrentScores);
        overPage.transform.GetChild(4).gameObject.SetActive(true);
        overPage.transform.GetChild(4).GetComponent<Text>().text = "New top " + (msc+1) + " record!";
       
        //Debug.Log("pobito rekord, nadpisano miejsce: "+(msc+1));
    }

    void ReorganizeHighscore()
    {
        //firstly we add a score to the list
        highscores.Add(pc.CurrentScores);
       

        //we sort it descending
        for(int i=0;i<highscores.Count;i++)
        {
            for (int j = 0; j < highscores.Count; j++)
            {
                if(highscores[j] < highscores[i])
                {
                    int tmp = highscores[i];
                    highscores[i] = highscores[j];
                    highscores[j] = tmp;
                }
            }
        }
        //we remove last one after sorting the list
        if(highscores.Count>5)
        {
            highscores.RemoveAt(highscores.Count - 1);
            BeatRecordCommunicate();
        }
        
        
    }

    bool CheckRecordPlace()
    {
        if (highscores == null || highscores.Count < 5 ) return true;
        foreach(int val in highscores)
        {
            if(pc.CurrentScores > val)
            {
                return true;
            }
        }
        return false;
       
    }

    #endregion

    public void DeleteAllPipes()
    {
        for (int i = 0; i < all_pipes.Count; i++)
        {
            Destroy(all_pipes[i].gameObject);
        }
        all_pipes = new List<Transform>();
    }
    
    void CreatePipe(float xPos, float h)
    {
        List<Pipe> allowed = ReturnAllowedPipes();
        int rand = 0;
        if (allowed.Count > 1)
        {
            rand = Random.Range(0, allowed.Count);
        }
       //creating an object
        GameObject pipe = Instantiate(_pipe, new Vector3(xPos, -CAMERA_SIZE + h * 0.5f, 0), Quaternion.identity);
        pipe.name = "insta_Pipe";
        //setting up the pipe attributes here
        PipeManager script = pipe.GetComponent<PipeManager>();
        script.SetupThePipe(allowed[rand].color, allowed[rand].sprite_assigned);
        //adding to the scene settings
        all_pipes.Add(pipe.transform);
        pipeSpawnTimer = 1;
    }

  
    List<Pipe> ReturnAllowedPipes()
    {
        List<Pipe> retVal = new List<Pipe>();
        foreach(Pipe pipe in all_possible_pipes)
        {
            if (pc.CurrentScores >= pipe.appearingLevel)
                retVal.Add(pipe);
        }
        return retVal;
    }
    void SpawnPipes()
    {
        if (pauseGame) return;

        pipeSpawnTimer -= Time.deltaTime;
        if(pipeSpawnTimer <= 0)
        {
            float rand = Random.Range(6.5f, 14.5f);
            CreatePipe(4f, rand);
        }
    }
    
    void PipesHandle()
    {
        if (pauseGame) return;

        for (int i=0;i<all_pipes.Count;i++)
        {
            if(all_pipes[i].position.x <= -12)
            {
                GameObject go = all_pipes[i].gameObject;
                all_pipes.RemoveAt(i);
                Destroy(go);
            }
            else
            {
                
                all_pipes[i].position += new Vector3(-1, 0, 0) * 2.7f * Time.deltaTime;
            }
        }
    }
}

//other 
public class Highscores
{
    public List<int> highscore_list = new List<int>();
}
public enum GameState
{
    None,
    Start,
    Running,
    GameOver,
}
