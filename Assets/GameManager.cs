using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Static instance for the singleton
    public static GameManager Instance { get; private set; }

    public int GameID = 0;

    public GameObject GameOverScreen, GameWinScreen, InfoScreen;
    public GameObject Information;
    public bool GameState = false;
    public BasePlayer Player;

    private ScoreObj Score;
    public GameObject[] face;
    public Transform[] spawnPoints;
    public float spawnDelay = 1f;

    private float lastTapTime = 0f; // Time since the last tap
    private float tapDecayDelay = 0f; // Time before value starts decreasing
    public float decayRate = 0.3f;




    private Vector2 tapPosition;
    public ParticleSystem poff;
    private List<GameObject> spawnedFaces = new List<GameObject>();

    public GameObject OldGame;
    public GameObject NewGame;

    public Text ScoreText;
    private int currentScore;

    public AudioSource GameOverReal;
    public AudioSource GameOverCartoon;
    public AudioSource Coin;
    public AudioSource GAmoverSfx;
    public AudioSource[] Tap;
    public AudioSource UISound;
    private float highestProgressValue = 0f;
    public Slider progressBar;

    // public Animator Playeranimators;
    public ParticleSystem poof;

    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private void Start()
    {
        InfoScreen.SetActive(true);
        StartCoroutine(SpawnFacesCoroutine());
        StartCoroutine(TimerRoutine());

    }

    void Update()
    {
        if (!GameState)
            return;


        // Playeranimators.SetFloat("Val", progressBar.value / progressBar.maxValue);
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 tapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(tapPosition, Vector2.zero);

            if (hit.collider.CompareTag("Enemy"))
            {

                IncreaseSliderValue(0.4f);
                int randomIndex = Random.Range(0, Tap.Length);
                Tap[randomIndex].Play();
                GameObject foodObject = hit.collider.gameObject;
                Vector3 collisionPosition = foodObject.gameObject.transform.position;
                foodObject.SetActive(false);
                poof.transform.position = collisionPosition;
                poof.Play();

                StartCoroutine(ReactivateAfterDelay(foodObject, 0.5f));
                // Adjust the decrement value as needed
            }

            if (hit.collider.CompareTag("dog"))
            {

                GameOVer();
            }
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) // Detects only the initial tap, ignoring holds
            {

                Vector2 tapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(tapPosition, Vector2.zero);

                if (hit.collider.CompareTag("Enemy"))
                {

                    IncreaseSliderValue(0.4f);
                    int randomIndex = Random.Range(0, Tap.Length);
                    Tap[randomIndex].Play();
                    GameObject foodObject = hit.collider.gameObject;
                    Vector3 collisionPosition = foodObject.gameObject.transform.position;
                    foodObject.SetActive(false);
                    poof.transform.position = collisionPosition;
                    poof.Play();

                    StartCoroutine(ReactivateAfterDelay(foodObject, 0.5f));
                    // Adjust the decrement value as needed
                }
                if (hit.collider.CompareTag("dog"))
                {

                    GameOVer();
                }


            }
        }



        if (progressBar.value == 0)
        {
            GameOVer();
        }




        if (Time.time - lastTapTime > tapDecayDelay)
        {
            DecreaseSliderValue();

        }
    }

    private IEnumerator ReactivateAfterDelay(GameObject oldFood, float delay)
    {
        yield return new WaitForSeconds(delay);


        Destroy(oldFood);
    }
    public GameObject[] doc;
    IEnumerator TimerRoutine()
    {
        while (true) // Runs forever unless stopped
        {
            yield return new WaitForSeconds(5f); // Waits 10 seconds
            StartCoroutine(DocActive()); // Calls your function
        }
    }
    public IEnumerator DocActive()
    {
        int randomIndex = Random.Range(0, doc.Length);
        doc[randomIndex].SetActive(true);
        yield return new WaitForSeconds(1f);
        doc[randomIndex].SetActive(false);
    }

    private void DecreaseSliderValue()
    {
        float previousValue = progressBar.value;
        if (progressBar != null && progressBar.value > 0)
        {
            progressBar.value -= decayRate * Time.deltaTime; // Decrease over time

            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);
        }
    }

    private void IncreaseSliderValue(float increment)
    {
        if (progressBar != null)
        {
            float previousValue = progressBar.value;
            progressBar.value += increment; // Increase slider value
            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);

            if (progressBar.value > highestProgressValue)
            {
                highestProgressValue = progressBar.value;
                AddScore();
            }




            if (progressBar.value >= progressBar.maxValue)
            {

                GameWin();

            }

        }
        lastTapTime = Time.time;
    }


    public void PlayGame()
    {
        GameState = true;
        Time.timeScale = 1;
        Information.SetActive(false);
    }
    public void PauseGame()
    {
        GameState = false;
        Information.SetActive(true);
        StartCoroutine(Pause());
    }
    public void uiSound()
    {
        UISound.Play();
    }

    IEnumerator Pause()
    {


        // Wait for a specified duration (adjust the delay as needed)
        yield return new WaitForSeconds(0.2f);
        Time.timeScale = 0;


    }


    IEnumerator SpawnFacesCoroutine()
    {
        while (true)
        {
            if (GameState)
            {
                // Generate a random number of faces to spawn: 2 or 3.
                int facesToSpawn = Random.Range(2, 4); // Upper bound is exclusive.

                for (int i = 0; i < facesToSpawn; i++)
                {
                    int randomIndex = Random.Range(0, face.Length);
                    GameObject randomFace = face[randomIndex];

                    // Check if there is at least one spawn point available.
                    if (spawnPoints.Length > 0)
                    {
                        int spawnIndex = Random.Range(0, spawnPoints.Length);
                        GameObject spawnedFace = Instantiate(randomFace, spawnPoints[spawnIndex].position, Quaternion.identity);
                        spawnedFaces.Add(spawnedFace);
                    }
                    else
                    {
                        Debug.LogWarning("No spawn points assigned!");
                    }
                }

                yield return new WaitForSeconds(spawnDelay);
            }
            else
            {
                yield return null;
            }
        }
    }


    private void DestroyAllSpawnedFaces()
    {
        foreach (GameObject faceObj in spawnedFaces)
        {
            if (faceObj != null)
            {
                Destroy(faceObj);
            }
        }
        spawnedFaces.Clear();
    }

    public void GameWin()
    {
        GameState = false;
        GameWinScreen.SetActive(true);
        Debug.Log(currentScore);
        SendScore(currentScore, 100);
    }

    public void GameOVer()
    {
        DestroyAllSpawnedFaces();
        GameState = false;
        GameOverScreen.SetActive(true);
        Debug.Log(currentScore);
        GAmoverSfx.Play();
        SendScore(currentScore, 114);
    }

    public void GameResetScreen()
    {
        progressBar.value = 2;
        highestProgressValue = 0f;
        ScoreText.text = "0";
        Score.score = 0;
        currentScore = 0;
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        GameState = true;
        Player.Reset();
    }

    public void AddScore()
    {


        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore += 10;
            ScoreText.text = currentScore.ToString();
        }
        else
        {

            ScoreText.text = "0";
        }
    }






    //HELPER FUNTION TO GET SPAWN POINT
    public Vector2 GetRandomPointInsideSprite(SpriteRenderer SpawnBounds)
    {
        if (SpawnBounds == null || SpawnBounds.sprite == null)
        {
            Debug.LogWarning("Invalid sprite renderer or sprite.");
            return Vector2.zero;
        }

        Bounds bounds = SpawnBounds.sprite.bounds;
        Vector2 randomPoint = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        // Transform local point to world space
        return SpawnBounds.transform.TransformPoint(randomPoint);
    }


    public struct ScoreObj
    {
        public float score;
    }
}
