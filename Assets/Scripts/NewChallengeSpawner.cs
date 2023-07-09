using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class NewChallengeSpawner : MonoBehaviour
{
    public GameObject instantiatedPizza;
    public GameObject instantiatedReceipt;
    public GameObject alert;
    public GameObject scoreText;
    public GameObject streakText;

    private PizzaFactory pizzaFactory;
    private CitationFactory citationFactory;
    private int warningPoints;
    public int score;
    private int streak;
    public bool currentPizzaGood;
    public string messedUpReason;

    public bool reviewed;
    public bool pizzaTrashed;
    public bool receiptTrashed;

    void Start() {
        pizzaFactory = GetComponent<PizzaFactory>();
        citationFactory = GetComponent<CitationFactory>();

        OrderNewPizza();
        if (alert) {
            alert.SetActive(false);
        }
    }

    public void Review(bool thrownAway){
        reviewed = true;
        if (thrownAway && currentPizzaGood)
        {
            // Do nothing;
        }
        else if (thrownAway)
        {
            streak = 0;
            score -= 300;
            Debug.Log("Messed up reason: " + messedUpReason);
            ErroredJudgement(false);
            streakText.GetComponent<TextMeshProUGUI>().faceColor = Color.red;
            streakText.GetComponent<TextMeshProUGUI>().text = "- 500";
            citationFactory.RegisterCitation(false);
        }
        else if (!currentPizzaGood)
        {
            streak = Math.Min(streak + 1, 3);
            score += 100 * streak;
            streakText.GetComponent<TextMeshProUGUI>().faceColor = Color.green;
              streakText.GetComponent<TextMeshProUGUI>().text = "+ " + (streak * 100).ToString();
        }
        else
        {
            streak = 0;
            score -= 300;
            ErroredJudgement(true);
            streakText.GetComponent<TextMeshProUGUI>().faceColor = Color.red;
            streakText.GetComponent<TextMeshProUGUI>().text = "- 500";
            citationFactory.RegisterCitation(true);
        }
        scoreText.GetComponent<TextMeshProUGUI>().text = "Score: $" + score.ToString();
        StartCoroutine(ShowStreak());
    }

    public void onTrashPizza() {
        if (!reviewed)
        {
            Review(true);
        }
        pizzaTrashed = true;
        if (receiptTrashed)
        {
            SpawnNewChallenge();
        }
    }

    public void onTrashReceipt()
    {
        receiptTrashed = true;
        if (pizzaTrashed)
        {
            SpawnNewChallenge();
        }
    }

    public void onBadReview() {
        Review(false);
    }

    public void SpawnNewChallenge() {
        reviewed = false;
        pizzaTrashed = false;
        receiptTrashed = false;
        Destroy(instantiatedPizza);
        Destroy(instantiatedReceipt);
        OrderNewPizza();
    }

    public int GetCurrentScore() {
        return score;
    }

    IEnumerator ShowStreak(){
        streakText.GetComponent<TextMeshProUGUI>().enabled = true;
        yield return new WaitForSeconds(2f);
        streakText.GetComponent<TextMeshProUGUI>().enabled = false;
    }

    void ErroredJudgement(bool isZelp) {
        warningPoints++;
        Debug.Log("Uh oh, you have "+ warningPoints + " warnings!");
    }

    public void OrderNewPizza() {
        PizzaFactory.PizzaOrder order = pizzaFactory.GenerateNewPizzaOrder();
        PizzaFactory.GeneratedPizza pizza = pizzaFactory.CreatePizza(order);

        instantiatedPizza = pizza.Pizza;
        instantiatedReceipt = pizza.Receipt;
        currentPizzaGood = pizza.CurrentPizzaGood;
        messedUpReason = pizza.MessedUpReason;
    }
}
