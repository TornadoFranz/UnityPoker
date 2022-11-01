using UnityEngine;
using Poker;
using PokerGUI;
using System.Collections;
using System;
using HoldemHand;

//https://www.codeproject.com/articles/12279/fast-texas-holdem-hand-evaluation-and-analysis
//https://www.codeproject.com/Articles/19091/More-Texas-Holdem-Analysis-in-C-Part-1
//https://www.codeproject.com/articles/19092/more-texas-holdem-analysis-in-c-part-2

public class Main : MonoBehaviour
{

    public static Main current { get; private set; }
    public Table table { get; private set; }


    [SerializeField] private TableSettings tableSettings;
    [SerializeField] private GameObject pause;
    [SerializeField] private Cam cam;
    [SerializeField] private GameObject pot;
    [SerializeField] private GameObject board;
    [SerializeField] private GameObject hand;
   

    public bool PlayAnimations => table.PlayAnimations;

    private bool PlayerIsThinking;
    private IEnumerator coroutine;


    private void Awake()
    {
        current = this;
        AnimTime.SkipMode = false;
        table = new Table(tableSettings.playerPresets, tableSettings.SBAmount, tableSettings.BBAmount, tableSettings.Ante);

        pot.SetActive(false);
        board.SetActive(false);
        hand.SetActive(false);
    }

    private void Start()
    {
        table.pokerEvents.SetTable(table);

        if(!PlayAnimations)
        {
            table.StartNewGame();
            return;
        }

        cam.StartGameCamera(c =>
        {
            pot.SetActive(true);
            board.SetActive(true);
            hand.SetActive(true);
            table.StartNewGame();
        });
    }

    private void Update()
    {
        //QUIT GAME
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //ARTIFICIAL AI THINKING TIME
        if (table.GameOn)
        {
            if (!LeanTween.isTweening())
            {
                if (table.CurrentPlayer.identity == Identity.AI)
                {
                    if (!PlayerIsThinking)
                    {
                        if (table.Players.ActiveList.Count > 1)
                        {
                            coroutine = WaitForAction();
                            StartCoroutine(coroutine);
                        }
                    }
                }
                else if (table.CurrentPlayer.identity == Identity.Human)
                {
                    if (!PlayerIsThinking)
                    {
                        table.CurrentPlayer.HumanAction();
                    }
                }
            }
        }

        //TOGGLE SKIP MODE
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            AnimTime.SkipMode = true;
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
                StartCoroutine(coroutine);
            }            
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            AnimTime.SkipMode = false;
        }

        //PAUSE
        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (Time.timeScale == 0)
            {
                pause.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                pause.SetActive(true);
                Time.timeScale = 0;
            }
        }


        //PERFORMANCE TESTS... currently empty
        if (Input.GetKeyDown(KeyCode.J))
        {

            System.Diagnostics.Stopwatch m_stopwatch = new System.Diagnostics.Stopwatch();
            m_stopwatch.Start();



            m_stopwatch.Stop();
            UnityEngine.Debug.Log($"YEP: { m_stopwatch.ElapsedMilliseconds} ms");
        }
    }


    IEnumerator WaitForAction()
    {
        table.pokerEvents.NewTurn(table);
        PlayerIsThinking = true;
        float timer = 0;

        if(PlayAnimations)
        {
            while (timer < AnimTime.waitforAI)
            {
                while (Time.timeScale == 0)
                {
                    yield return null;
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }        

        //yield return new WaitForSeconds(AnimTime.waitforAI);
        table.CurrentPlayer.AIAction();
        PlayerIsThinking = false;
    }
}
