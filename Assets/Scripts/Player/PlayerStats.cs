using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [HideInInspector] public Hazard acidLink = null;
    [HideInInspector] public Hazard alkalineLink = null;

    [SerializeField] private GameObject spinslashAcidPuddle;
    [SerializeField] private GameObject spinslashAlkalinePuddle;


    public Slider healthBar;
    public Slider PHBar;
    public Slider PHBar2;
    public Slider AcidBar;
    public Slider AcidBar2;
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
    private RotationController rotation;

    public MusicClass music;

    public Vector3 spawnpoint;

    public bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      GameObject PHUI = GameObject.FindWithTag("PH Bar");
      PHBar = PHUI.transform.GetChild(0).GetComponent<Slider>();
      PHBar2 = PHUI.transform.GetChild(1).GetComponent<Slider>();
      AcidBar = PHUI.transform.GetChild(2).GetComponent<Slider>();
      AcidBar2 = PHUI.transform.GetChild(3).GetComponent<Slider>();

      healthBar.maxValue= maxHealth;

      movementController = gameObject.GetComponent<MovementController>();
      rotation = gameObject.transform.GetChild(0).GetComponent<RotationController>();

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
        //ph -= 0.5f * Time.deltaTime;
      }

      if (acid < 0) {
        acid = 0;
      } else if (acid > 14) {
        acid = 14;
      } else {
        //acid -= 0.5f * Time.deltaTime;
      }

      if (health < maxHealth) {
        
      } else if (health > maxHealth) {
        health = maxHealth;
      }

      if (health <= 0) {
            if (music != null) {
              music.StopMusic();
            }
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

      healthBar.value= health;
      PHBar.value = 4 + 80 * (ph / PH_DEFAULT);
      AcidBar.value = 16 + 80 * (acid / PH_DEFAULT);

      
      if (ph > 7) {
        AlkalineIndicator.enabled = true;
        PHBar.value = 7;
        PHBar2.value = (ph-7) / 7;
      } else {
        AlkalineIndicator.enabled = false;
        PHBar.value = ((ph) / 7);
        PHBar2.value = 0;

      }

      if (acid > 7) {
        AcidIndicator.enabled = true;
        AcidBar.value = 7;
        AcidBar2.value = (acid-7) / 7;
      } else {
        AcidIndicator.enabled = false;
        AcidBar.value = ((acid) / 7);
        AcidBar2.value = 0;

      }


      
      if (ph > 7) {
      } else {
      }

      if (acid > 7) {
      } else {
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
            if (music != null) {
              music.StopMusic();
            }
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

      if (knockback > 0) {
        isInvincible = true;
        iFrameTimer = 0;
      }

      movementController.applyKnockback(position, knockback);

      if (GameManager.isScreenshakeEnabled) {
        cam.GetComponent<screenShake>().ScreenShake(.1f);
        GameManager.slowdownTime(slowdownRate / slowdownRateMultiplier, slowdownLength * slowdownLengthMultiplier);
        Color flashColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        playerHitMaterial.SetColor("_Color", flashColor);
      }

        
        
      audioSource.PlayOneShot(playerHitSound, 0.45F);


    }

    public void changePH(float amount) {

      if (alkalineLink != null) {
        alkalineLink.spendPuddle();
      }

      ph += amount / 3;

      if (ph < 0) {
        ph = 0;
      }
      else if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      }
      else {
        ph -= Time.deltaTime;
      }

      if (ph > 14 - acid) {
        acid = 14 - ph;
      }

  }

  public void changeAcidity(float amount) {
      if (acidLink != null) {
        acidLink.spendPuddle();
      }

      acid += amount / 3;

      if (acid < 0) {
        acid = 0;
      }
      else if (acid > 14) {
        acid = 14;
      }
      else {
        acid -= Time.deltaTime;
      }

      if (acid > 14 - ph) {
        ph = 14 - acid;
      }
    }

  public TypesPH spinslashStarted() {
    if (ph > 7) {
      ph -= 7;
      if (ph == 7) {
        ph= 7.01f;
      }

      Vector3 puddlePos = new Vector3(transform.position.x, -0.85f, transform.position.z);
      puddlePos = puddlePos + (-rotation.GetRotationDirection() * 3f);
      Instantiate(spinslashAlkalinePuddle, puddlePos, rotation.transform.rotation);

      return TypesPH.Alkaline;
    }
    else if (acid > 7) {
      acid -= 7;
      if (acid == 7) {
        acid = 7.01f;
      }

      Vector3 puddlePos = new Vector3(transform.position.x, -0.85f, transform.position.z);
      puddlePos = puddlePos + (-rotation.GetRotationDirection() * 3);
      Instantiate(spinslashAcidPuddle, puddlePos, rotation.transform.rotation);

      return TypesPH.Acidic;
    }
    else {
      return TypesPH.Neutral;
    }
  }

  public void makeScreenshake() {
    if (GameManager.isScreenshakeEnabled) {
      cam.GetComponent<screenShake>().ScreenShake(0.05f);
      GameManager.slowdownTime(slowdownRate / slowdownRateMultiplier, slowdownLength * slowdownLengthMultiplier / 4f);
    }
        
  }
    
}
