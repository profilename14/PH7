using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float health = 100;
    public float ph = 14;

    const float HEALTH_MAX = 100;
    const float PH_DEFAULT = 14;

    public float healthRegen = 0f;
    public float phRegen = 0.33f;

    public Slider healthBar;
    public Slider PHBar;

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


    // Start is called before the first frame update
    void Start()
    {
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      PHBar = GameObject.FindWithTag("PH Bar").GetComponent<Slider>();

      healthBar.maxValue= HEALTH_MAX;

      movementController = gameObject.GetComponent<MovementController>();

      audioSource = GameObject.FindGameObjectWithTag("Sound").GetComponent<AudioSource>();
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

      if (ph < PH_DEFAULT) {
        ph += phRegen * Time.deltaTime;
      } else if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      }

      if (health < HEALTH_MAX) {
        
      } else if (health > HEALTH_MAX) {
        health = HEALTH_MAX;
      }

      if (health < 0) {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

      healthBar.value= health;
      PHBar.value = 16 + 80 * (ph / PH_DEFAULT);
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
      float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
      health -= damage * multiplier;

      if (health < 0) {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            // No camera is displaying appears, but hey at least it stops gameplay
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
}
