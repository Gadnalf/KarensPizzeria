using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaFactory : MonoBehaviour
{
    public GameObject pizzaPrefab;
    public GameObject receiptPrefab;

    public GameObject crust;
    public GameObject cheese;
    public List<GameObject> sauces;
    public List<GameObject> ingredients;

    private Dictionary<string, GameObject> sauceTable = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> ingredientsTable = new Dictionary<string, GameObject>();

    public float ingredientOffset = 3; //inches
    public float radialIngredientOffset = 3f; //offset around circle
    public float ingredientOffsetScaleFactor; //to game world scale
    public float ingredientSkew = 3;
    public float crustOffset = 1;

    public struct PizzaOrder
    {
        public PizzaOrder(int diameter, int slices, List<string> leftIngredients, List<string> rightIngredients, bool wellDone)
        {
            Diameter = diameter;
            Slices = slices;
            LeftIngredients = leftIngredients;
            RightIngredients = rightIngredients;
            WellDone = wellDone;
        }

        public int Diameter { get; set; }
        public int Slices { get; set; }
        public List<string> LeftIngredients { get; set; }
        public List<string> RightIngredients { get; set; }
        public bool WellDone { get; set; }
    }

    public struct GeneratedPizza {
        public GeneratedPizza(GameObject pizza, GameObject receipt, bool good) {
            Pizza = pizza;
            Receipt = receipt;
            CurrentPizzaGood = good;
        }
        public GameObject Pizza { get; private set; }
        public GameObject Receipt { get; private set; }
        public bool CurrentPizzaGood { get; private set; }
    }

    public void Start()
    {
        // for faster lookup (probably)
        foreach (GameObject sauce in sauces) {
            sauceTable.Add(sauce.name.ToLower(), sauce);
            Debug.Log("[PizzaFactory] Adding sauce: " + sauce.name);
        }
        foreach (GameObject ingredient in ingredients) {
            ingredientsTable.Add(ingredient.name.ToLower(), ingredient);
            Debug.Log("[PizzaFactory] Adding ingredient: " + ingredient.name);
        }
    }

    // ORDER THE PIZZA
    public PizzaOrder GenerateNewPizzaOrder() {
        string[] PossibleSauce = {"tomato sauce",  "pesto sauce"};
        string[] PossibleIngredients = {"cheese", "pepperoni", "mushrooms", "peppers", "pineapple", "anchovies", "olives"};

        Debug.Log("It's pizza time!");
        int diameter;
        int slices;
        List<string> leftIngredients = new List<string>();
        List<string> rightIngredients = new List<string>();
        bool wellDone;


        // set diameter
        // int personal = 10;
        int small = 12;
        int medium = 14;
        int large = 16; 
        int[] sizes = {small, medium, large};
        diameter = sizes[Random.Range(0,2)];
        Debug.Log("diameter" + diameter);

        // set number of slices
        slices = Random.Range(1,6) * 2;

        // set left ingredients
        int sauceChoice = Random.Range(0,2);
        if(sauceChoice < 2){
            leftIngredients.Add(PossibleSauce[sauceChoice]);
        } 
        

        int numLeftIngredients = Random.Range(0,PossibleIngredients.Length-1);
        

        for(int i = 0; i < numLeftIngredients; i ++){
            string topping = PossibleIngredients[Random.Range(0,PossibleIngredients.Length-1)];
            if(!leftIngredients.Contains(topping)){
                leftIngredients.Add(topping);
            }
        }
        Debug.Log("leftIngredients" + string.Join(", ", leftIngredients.ToArray()));

        // set right ingredients
        sauceChoice = Random.Range(0,2);
        if(sauceChoice < 2){
            rightIngredients.Add(PossibleSauce[sauceChoice]);
        } // else no sauce

        int numRightIngredients = Random.Range(0,PossibleIngredients.Length-1);
        

        for(int i = 0; i < numRightIngredients; i ++){
            string topping = PossibleIngredients[Random.Range(0,PossibleIngredients.Length-1)];
            if(!rightIngredients.Contains(topping)){
                rightIngredients.Add(topping);
            }
        }
        Debug.Log("rightIngredients" + string.Join(", ", rightIngredients.ToArray()));

        // is well done
        if (Random.Range(0,1) == 0){
            wellDone = false;
        } else {
            wellDone = true;
        }
        Debug.Log("wellDone" + wellDone);
        return new PizzaOrder(diameter, slices, leftIngredients, rightIngredients, wellDone);
    }

    // SUMMON THE PIZZA
    public GeneratedPizza CreatePizza(PizzaOrder pizzaOrder)
    {
        Vector3 centerPosition = new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z);
        Vector3 centerCameraPosition = Camera.main.ViewportToWorldPoint(centerPosition);
        GameObject instantiatedPizza = InstantiatePizza(pizzaOrder);

        instantiatedPizza.GetComponents<PizzaTrashBehaviour>()[0].eventSystem = gameObject;

        Vector3 transform = new Vector3(-5f, 0f, 0f);
        Vector3 offsetCameraPositon = centerCameraPosition + transform;
        GameObject instantiatedReceipt = Instantiate(receiptPrefab, offsetCameraPositon, Quaternion.identity);
        Color pizzaColor = new Color(
            Random.Range(0f, 1f), 
            Random.Range(0f, 1f), 
            Random.Range(0f, 1f)
        );
        instantiatedPizza.GetComponent<Renderer>().material.SetColor("_Color", pizzaColor);

        bool currentPizzaGood = Random.Range(0, 2) != 0;
        if (currentPizzaGood) {
            instantiatedReceipt.GetComponent<Renderer>().material.SetColor("_Color", pizzaColor);
        } else {
            Color receiptColor = new Color(
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f)
            );
            instantiatedReceipt.GetComponent<Renderer>().material.SetColor("_Color", receiptColor);
        }

        return new GeneratedPizza(instantiatedPizza, instantiatedReceipt, currentPizzaGood);
    }

    public GameObject InstantiatePizza(PizzaOrder order)
    {
        GameObject pizza = Instantiate(crust);

        // TODO: don't forget to handle cheese and sauce n shit

        float angleOffset = 0.1f;
        foreach (string ingredient in order.LeftIngredients)
        {
            //Debug.Log("Adding left ingredient: " + ingredient);
            AddIngredientsRadially(pizza, ingredient, true, angleOffset);
            angleOffset += ingredientSkew;
            angleOffset *= -1;
        }

        angleOffset = 0;
        foreach (string ingredient in order.RightIngredients)
        {
            //Debug.Log("Adding right ingredient: " + ingredient);
            AddIngredientsRadially(pizza, ingredient, false, angleOffset);
            angleOffset += ingredientSkew;
            angleOffset *= -1;
        }
        return pizza;

        void AddIngredientsRadially(GameObject parent, string ingredientName, bool left, float angleOffset)
        {
            Vector2 origin = parent.transform.position;
            float angle = angleOffset;
            for (float radius = ingredientOffset; radius < (order.Diameter - crustOffset); radius += ingredientOffset)
            {
                //Debug.Log("Current circumference: " + 2 * 3.14f * radius);
                float ingredientAngle = 360/Mathf.RoundToInt(2 * 3.14f * radius / radialIngredientOffset);
                // get angle based on circumference
                for (int i = 0; i < (360 / ingredientAngle); i++)
                {
                    angle += ingredientAngle;
                    //Debug.Log("Current angle: " + angle);
                    if (left && (angle % 360 < 180) || !left && (angle % 360 >= 180))
                    {
                        // rotate a vector?
                        Vector2 rotationVector = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.down;
                        Vector2 offsetVector = rotationVector * (ingredientOffsetScaleFactor * (radius + Random.Range(-0.2f, 0.2f)));
                        Vector2 ingredientPos = origin + offsetVector;
                        GameObject newIngredient = Instantiate(ingredientsTable[ingredientName]);
                        //Debug.Log("Creating new ingredient: " + newIngredient + "at pos: " + ingredientPos);
                        newIngredient.transform.position = (Vector3)ingredientPos + new Vector3(0, 0, -1);
                        newIngredient.transform.parent = parent.transform;
                    }
                }
                Debug.Log(radius);
            }
        }
    }
}
