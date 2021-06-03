using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Text scoreText,bombText;
    Rigidbody2D rig;
    LevelController lc;
    Animator explosionAnim;

    public static PlayerController instance;
    
    private int playerScores = 0,playerBombs=0;


    float lastClick = 0;
    float dblClickCatchTime = 0.2f;

    GameObject explosionObj;
    public SpriteRenderer backgroundObj;
    
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        lc = LevelController.instance;

        explosionObj = transform.GetChild(0).gameObject;
        explosionAnim = explosionObj.GetComponent<Animator>();

    }

    public int CurrentScores
    {
        get { return playerScores; }
        set { playerScores = value; }
    }
    public void UpdateScores()
    {
        scoreText.text = playerScores.ToString();
        bombText.text = playerBombs.ToString();
    }


    void Update()
    {
        if(lc.GetGameState() == GameState.Running)
        {
            if (!rig.simulated)
                rig.simulated = true;
            
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastClick < dblClickCatchTime)
                {
                    if(playerBombs>0)
                    {
                        TriggerTheBomb();
                    }
                }
                lastClick = Time.time;

                rig.velocity = Vector3.zero;
                transform.rotation = Quaternion.Euler(0, 0, 40);
                rig.AddForce(Vector2.up * 280);
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, -100), 1 * Time.deltaTime);
        }
        else
        {
            if (rig.simulated)
            {
                rig.simulated = false;
            }
        }
    }

    void TriggerTheBomb()
    {
        lc.DeleteAllPipes();
        explosionObj.SetActive(true);
        explosionAnim.Play("Explode");
        playerBombs--;
        UpdateScores();
    }


    public void StartParams()
    {
        transform.position = new Vector3(-1.5f, 1, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        rig.velocity = Vector3.zero;
        backgroundObj.color = Color.white;
        playerBombs = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Killable")
        {
            lc.GameOver();
        }
        else if(tag == "Bonus")
        {
            playerScores++;
            if((playerScores%10)==0)
            {
                playerBombs++;
            }
            UpdateScores();
            ChangeBgColor();
        }
        else
        {
            //just in case for tests
            Debug.LogError("Unhandlaed type of collider");
        }
    }

    void ChangeBgColor()
    {
        float R = backgroundObj.color.r;
        float G = backgroundObj.color.g;
        float B = backgroundObj.color.b;
        if(R>0)
        {
            Color c = new Color(R-=0.02f, backgroundObj.color.g, backgroundObj.color.b, backgroundObj.color.a);
            backgroundObj.color = c;
        }
        else if(G>0)
        {
            Color c = new Color(backgroundObj.color.r, G -= 0.02f, backgroundObj.color.b, backgroundObj.color.a);
            backgroundObj.color = c;
        }
        else if(B>0)
        {
            Color c = new Color(backgroundObj.color.r, backgroundObj.color.g, B -= 0.02f, backgroundObj.color.a);
            backgroundObj.color = c;
        }
        
    }
}
