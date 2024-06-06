using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float health = 6;
    public float ph = 0;
    public float acid = 0;

    public float maxHealth = 6;
    const float PH_DEFAULT = 14;

    public float healthRegen = 0f;
    public float phRegen = 0.33f;

    public bool inAcid = false;
    public bool inAlkaline = false;

    public Slider healthBar;
    public Slider PHBar;
    public Slider AcidBar;
    public Image AlkalineIndicator;
    public Image AcidIndicator;

    public bool isInvincible;
    public float iFrameSeconds;
    private float iFrameTimer = 0;

    public GameObject cam;
    
    private float slowdownRate = 0.14f;
    private float slowdownLength = 0.02f;
    public float slowdownRateMultiplier = 2f;
    public float slowdownLengthMultiplier = 1f;
    private AudioSource audioSource;
    [SerializeField] private Material playerHitMaterial;
    [SerializeField] private AudioClip playerHitSound;

    private MovementController movementController;

    public MusicClass music;

    public Vector3 spawnpoint;

    public bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      PHBar = GameObject.FindWithTag("PH Bar").GetComponent<Slider>();
      AcidBar = GameObject.FindWithTag("Acid Bar").GetComponent<Slider>();

      healthBar.maxValue= maxHealth;

      movementController = gameObject.GetComponent<MovementController>();

      audioSource = GameObject.FindGameObjectWithTag("Sound").GetComponent<AudioSource>();
        gameStarted = true;
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (gameStarted)
        {
            //this.gameObject.transform.position = spawnpoint;
        }
    }

    // Update is called once per frame
    void Update()
    {

    if (isInvincible)
    {
        iFrameTimer += Time.deltaTime;
        if (iFrameTimer > iFrameSeconds)
        {
            isInvincible = false;
            iFrameTimer = 0;
        }
    }

      if (ph < 0) {
        ph = 0;
      } else if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      } else {
        ph -= Time.deltaTime;
      }

      if (acid < 0) {
        acid = 0;
      } else if (acid > 14) {
        acid = 14;
      } else {
        acid -= Time.deltaTime;
      }

      if (health < maxHealth) {
        
      } else if (health > maxHealth) {
        health = maxHealth;
      }

      if (health < 0) {
            music.StopMusic();
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

      healthBar.value= health;
      PHBar.value = 16 + 80 * (ph / PH_DEFAULT);
      AcidBar.value = 16 + 80 * (acid / PH_DEFAULT);
      
      if (inAlkaline) {
        AlkalineIndicator.enabled = true;
      } else {
        AlkalineIndicator.enabled = false;
      }

      if (inAcid) {
        AcidIndicator.enabled = true;
      } else {
        AcidIndicator.enabled = false;
      }
    }

    public void playerDamage(float damage, float phChange, Vector3 position, float knockback) {
      bool isPlayerDashing;
      isPlayerDashing = movementController.isDashing;


      if (isPlayerDashing || isInvincible) {
        return;
      }

      ph -= phChange;

      if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      } else if (ph < 0) {
        ph = 0;
      }

      float pHDifference = Mathf.Abs(PH_DEFAULT - ph);
      //float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
      health -= damage;

      if (health < 0) {
            music.StopMusic();
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

      if (knockback > 0) {
        isInvincible = true;
        iFrameTimer = 0;
      }

      movementController.applyKnockback(position, knockback);

        cam.GetComponent<screenShake>().ScreenShake(.1f);
      GameManager.slowdownTime(slowdownRate / slowdownRateMultiplier, slowdownLength * slowdownLengthMultiplier);
        Color flashColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        playerHitMaterial.SetColor("_Color", flashColor);
        
      audioSource.PlayOneShot(playerHitSound, 0.45F);


    }

    public void changePH(float amount) {
      ph += amount;

      if (ph < 0) {
        ph = 0;
      }
      else if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      }
      else {
        ph -= Time.deltaTime;
      }

  }

  public void changeAcidity(float amount) {
      acid += amount;

      if (acid < 0) {
        acid = 0;
      } else if (acid > 14) {
        acid = 14;
      } else {
        acid -= Time.deltaTime;
      }
    }
    
}
