using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerContollerScript : MonoBehaviour
{
    [SerializeField]
    private GameStats gameStats;
    private bool takingDamage = false;
    private bool isDead = false;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip pickupSound;
    [SerializeField]
    private AudioClip loseFanfare;
    private Animator animator;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private Vector3 input;
    private UI_game_manager ui;

    public bool hasWhip = false;
    public bool hasBible = false;
    public bool hasHolyWater = false;
    public bool hasThrowingKnife = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = pickupSound;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        gameStats.player.PlayerHealth = gameStats.player.PlayerMaxHealth;
        animator = GetComponent<Animator>();
        ui = GameObject.FindGameObjectWithTag("UIToolkit").GetComponent<UI_game_manager>();
    }

    void Update()
    {
        audioSource.volume = PlayerPrefs.GetFloat("EffectsVolume");
        input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0.0f).normalized;

        if (!takingDamage)
        {
            sprite.color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (gameStats.player.PlayerHealth > gameStats.player.PlayerMaxHealth)
        {
            gameStats.player.PlayerHealth = gameStats.player.PlayerMaxHealth;
        }

        gameObject.transform.GetChild(0).gameObject.SetActive(hasWhip);
        gameObject.transform.GetChild(1).gameObject.SetActive(hasBible);
        gameObject.transform.GetChild(2).gameObject.SetActive(hasHolyWater);
        gameObject.transform.GetChild(3).gameObject.SetActive(hasThrowingKnife);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isDead)
        {
            rb.MovePosition(transform.position + input * Time.deltaTime * gameStats.player.PlayerSpeed);

            // Character animations and sprite flipping
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                animator.SetTrigger("player_walk");
                if (Input.GetAxisRaw("Horizontal") > 0)
                    transform.eulerAngles = new Vector3(0, 0, 0);
                else if (Input.GetAxisRaw("Horizontal") < 0)
                    transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else
            {
                animator.SetTrigger("player_idle");
            }
        }


    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && !isDead)
        {
            gameStats.player.PlayerHealth -= collision.GetComponent<EnemyControllerScript>().damage;

            if (gameStats.player.PlayerHealth <= 0)
            {
                StartCoroutine(Die());
                isDead = true;
            }
            else
            {
                takingDamage = true;
                sprite.color = new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
        if (collision.tag == "Projectile" && !isDead)
        {
            gameStats.player.PlayerHealth -= collision.GetComponent<ProjectileController>().damage;
            Destroy(collision.gameObject);

            if (gameStats.player.PlayerHealth <= 0)
            {
                StartCoroutine(Die());
                isDead = true;
            }
            else
            {
                takingDamage = true;
                sprite.color = new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            takingDamage = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "ExitDoor")
        {
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        }
        if (collision.tag == "EnterDoor")
        {
            PlayerPrefs.SetString("FoundEasterEgg","Found");
            PlayerPrefs.Save();
            SceneManager.LoadScene("HouseScene", LoadSceneMode.Single);
        }
    }

    IEnumerator Die()
    {
        // remove weapons
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
        gameObject.transform.GetChild(2).gameObject.SetActive(false);
        gameObject.transform.GetChild(3).gameObject.SetActive(false);
        audioSource.clip = loseFanfare;
        audioSource.Play();
        animator.SetTrigger("player_die");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 2);
        ui.EndGame(false);
    }
}
