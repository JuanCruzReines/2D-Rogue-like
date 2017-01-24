using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;

    [Header("Sonidos")]
    [SerializeField]
    public AudioClip moveSound1;
    [SerializeField]
    public AudioClip moveSound2;
    [SerializeField]
    public AudioClip eatSound1;
    [SerializeField]
    public AudioClip eatSound2;
    [SerializeField]
    public AudioClip drinkSound1;
    [SerializeField]
    public AudioClip drinkSound2;
    [SerializeField]
    public AudioClip gameOverSound;

    private Animator animator;
    private int food;
    private Vector2 touchOrigin = -Vector2.one;

	protected override void Start () {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoint;

        updateFoodText();

        base.Start();
	}

    void OnDisable()
    {
        GameManager.instance.playerFoodPoint = food;
    }
	
	void Update () {
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

#if UNITY_STANDALONE || UNITY_WEBPLAYER

        horizontal = (int) Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

#else
        if(Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
                touchOrigin = myTouch.position;
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;

                if (Mathf.Abs(x) > Mathf.Abs(y))
                    horizontal = x > 0 ? 1 : -1;
                else
                    vertical = y > 0 ? 1 : -1; 
            }
                
        }

#endif
        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
            AttempMove<Wall>(horizontal, vertical);
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Exit":
                Invoke("Restart", restartLevelDelay);
                enabled = false;
                break;

            case "Food":
                food += pointsPerFood;
                pickUpFoodTextUpdate(pointsPerFood);
                collision.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
                break;

            case "Soda":
                food += pointsPerSoda;
                pickUpFoodTextUpdate(pointsPerSoda);
                collision.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
                break;

            default:
                break;
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("playerChop");
    }


    void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;

        CheckIfGameOver();
    }

    protected override void AttempMove<T>(int xDir, int yDir)
    {
        food--;
        updateFoodText();

        base.AttempMove<T>(xDir, yDir);

        RaycastHit2D hit;

        if (Move(xDir, yDir, out hit))
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }

    private void updateFoodText()
    {
        foodText.text = "Food: " + food;
    }

    private void pickUpFoodTextUpdate(int points)
    {
        foodText.text = "+" + points + " Food: " + food;
    }
}
