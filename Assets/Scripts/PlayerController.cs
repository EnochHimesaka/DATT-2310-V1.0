using System;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed, direction0, faceDirection0;
    public Boolean rightCommand, leftCommand, sitting;
    private GameObject panel, black;
    private Rigidbody2D rigidBody;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Boolean gameover, gameoverTrigger;
    private int runningFactor;

    // --- 👣 脚步音效相关 ---
    public AudioSource stepSound;            // 拖入脚步声音效
    public float stepInterval = 0.4f;        // 两步之间的时间
    private float stepTimer = 0f;            // 内部计时器

    void Start()
    {
        if (GameObject.Find("MainDialoguePanel") != null)
        {
            panel = GameObject.Find("MainDialoguePanel");
        }
        if (GameObject.Find("Black") != null)
        {
            black = GameObject.Find("Black");
        }

        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        faceDirection0 = 1;
        direction0 = 1;
        gameover = false;
    }

    void Update()
    {
        Boolean running = Input.GetKey(KeyCode.LeftShift);
        runningFactor = running ? 2 : 1;
        spriteRenderer.color = sitting ? new Color(1, 1, 1, 0) : Color.white;
    }

    void FixedUpdate()
    {
        Vector2 velocity = rigidBody.linearVelocity;

        if (panel.GetComponent<TextPresentor>().IsHidden() && !IsGameOver())
        {
            float horizontal = sitting ? 0 : Input.GetAxis("Horizontal");

            if (leftCommand)
            {
                velocity.x = (Mathf.Max(horizontal, 0) - 1) * runningFactor * moveSpeed;
            }
            else if (rightCommand)
            {
                velocity.x = (Mathf.Min(horizontal, 0) + 1) * runningFactor * moveSpeed;
            }
            else
            {
                velocity.x = horizontal * runningFactor * moveSpeed;
            }

            rigidBody.linearVelocity = velocity;

            anim.SetBool("walking", Mathf.Abs(velocity.x) > 0);
        }
        else
        {
            velocity.x = 0;
            rigidBody.linearVelocity = velocity;
        }

        direction0 = velocity.x > 0 ? 1 : (velocity.x < 0 ? -1 : direction0);
        faceDirection0 += 0.2f * direction0;
        faceDirection0 = Math.Min(1, Math.Max(-1, faceDirection0));

        transform.localScale = new Vector3(faceDirection0, 1, 1);

        // 👣 播放脚步声（只在水平移动且Y方向接近0时）
        bool isWalking = Mathf.Abs(velocity.x) > 0.1f && Mathf.Abs(rigidBody.linearVelocity.y) < 0.1f;

        if (isWalking)
        {
            stepTimer += Time.fixedDeltaTime;
            if (stepTimer >= stepInterval)
            {
                if (stepSound != null) stepSound.Play();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval;
        }

        // 游戏失败
        if (gameover && !gameoverTrigger)
        {
            gameoverTrigger = true;
            anim.SetBool("hit", true);
            black.GetComponent<BlackCoverScript>().turnAlpha = false;
            Invoke("ReloadScene", 3f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Danger"))
        {
            gameover = true;
        }
        if (collision.gameObject.CompareTag("Teleporter"))
        {
            SceneManager.LoadScene(collision.gameObject.GetComponent<SceneTeleporter>().sceneName);
        }
        if (collision.gameObject.CompareTag("PlatformEdge"))
        {
            rigidBody.linearVelocity = new Vector3(-10, 0, 0);
        }
    }

    public Boolean IsGameOver()
    {
        return gameover;
    }

    public void ForceMove(string direction, float duration)
    {
        StopMovingCommand();

        if (direction == "right")
        {
            rightCommand = true;
        }
        else if (direction == "left")
        {
            leftCommand = true;
        }

        StartCoroutine(StopForcedMovementAfter(duration));
    }

    private IEnumerator StopForcedMovementAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StopMovingCommand();
    }

    public void StopMovingCommand()
    {
        rightCommand = false;
        leftCommand = false;
    }

    public Boolean HasMovingCommand()
    {
        return leftCommand || rightCommand;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
