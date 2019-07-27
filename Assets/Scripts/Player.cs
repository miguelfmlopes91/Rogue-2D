using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Player : MovingObject{
    public int wallDamage = 1;
    public int pointsForFood = 10;
    public int pointsForSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1; 
    public AudioClip eatSound2; 
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;


    private Animator animator;
    private int food;



    // Start is called before the first frame update
    protected override void Start(){
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        foodText.text = "Food: " + food;
        base.Start();
    }

    private void OnDisable(){
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update(){
        if (!GameManager.instance.playerTurn){
            return;
        }

        int horizontal;
        int vertical;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal!=0){
            vertical = 0;
        }

        if (horizontal!=0 || vertical!=0){
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    protected override void AttemptMove<T>(int xDir, int yDir){
        --food;
        foodText.text = "Food: " + food;

        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;
        if (Move(xDir,yDir, out hit)){
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        checkIfGameOver();

        GameManager.instance.playerTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.tag == "Exit"){
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if(other.tag == "Food"){
            food += pointsForFood;
            foodText.text = "+" + pointsForFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsForSoda;
            foodText.text = "+" + pointsForSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }

    protected override void OnCantMove<T>(T component){
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("playerChop");
    }

    private void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void loseFood(int loss) {
        animator.SetTrigger("playerhit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;

        checkIfGameOver();
    }

    private void checkIfGameOver() {
        if (food <= 0){
            SoundManager.instance.RandomizeSfx(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }
}
