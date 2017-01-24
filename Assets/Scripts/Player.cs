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

    private Animator animator;
    private int food;

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

        horizontal = (int) Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

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
                break;

            case "Soda":
                food += pointsPerSoda;
                pickUpFoodTextUpdate(pointsPerSoda);
                collision.gameObject.SetActive(false);
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

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
            GameManager.instance.GameOver();
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
