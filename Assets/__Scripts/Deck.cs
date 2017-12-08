// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Deck";
    #endregion

    #region Static

    #endregion

    #region Public
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabSprite;
    public GameObject prefabCard;

    [Header("For debug view only.")]

    public PT_XMLReader xmlr = null;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static
    public static void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();

        int ndx;
        tCards = new List<Card>();
        while(oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }
    #endregion

    #region Public
    public void InitDeck(string deckXMLText)
    {
        if (GameObject.Find("Deck") == null)
        {
            GameObject anchorGO = new GameObject("Deck");
            deckAnchor = anchorGO.transform;
        }

        dictSuits = new Dictionary<string, Sprite>()
        {
            { "C", suitClub},
            { "D", suitDiamond},
            { "H", suitHeart},
            { "S", suitSpade}
        };

        ReadDeck(deckXMLText);
        MakeCards();
    }

    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);
        PrintDebugMsg(deckXMLText);

        string s = "xml[0] decorator[0] ";
        s += " type = " + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x = " + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y = " + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale = " + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        PrintDebugMsg(s);

        decorators = new List<Decorator>();
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for(int i = 0; i < xDecos.Count; i++)
        {
            deco = new Decorator();
            deco.type = xDecos[i].att("type");
            deco.flip = xDecos[i].att("flip") == "1";
            deco.scale = float.Parse(xDecos[i].att("scale"));
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            decorators.Add(deco);
        }

        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for(int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if(xPips != null)
            {
                for(int ii = 0; ii < xPips.Count; ii++)
                {
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = xPips[ii].att("flip") == "1";
                    deco.loc.x = float.Parse(xPips[ii].att("x"));
                    deco.loc.y = float.Parse(xPips[ii].att("y"));
                    deco.loc.z = float.Parse(xPips[ii].att("z"));
                    if (xPips[ii].HasAtt("scale")) deco.scale = float.Parse(xPips[ii].att("scale"));
                    cDef.pips.Add(deco);
                }
            }
            if (xCardDefs[i].HasAtt("face")) cDef.face = xCardDefs[i].att("face");
            cardDefs.Add(cDef);
        }
    }

    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach(CardDefinition cd in cardDefs)
        {
            if (cd.rank == rnk) return cd;
        }

        return null;
    }

    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach(string s in letters)
        {
            for (int i = 0; i < 13; i++) cardNames.Add(s + (i + 1));
        }

        cards = new List<Card>();
        Sprite ts = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;
        
        for (int i = 0; i < cardNames.Count; i++)
        {
            GameObject cgo = Instantiate(prefabCard) as GameObject;
            cgo.transform.parent = deckAnchor;
            Card card = cgo.GetComponent<Card>();

            cgo.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);

            card.name = cardNames[i];
            card.suit = card.name[0].ToString();
            card.rank = int.Parse(card.name.Substring(1));
            if(card.suit == "D" || card.suit == "H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }
            else
            {
                card.colS = "Black";
                card.color = Color.black;
            }
            card.def = GetCardDefinitionByRank(card.rank);

            foreach(Decorator deco in decorators)
            {
                if(deco.type == "suit")
                {
                    tGO = Instantiate(prefabSprite) as GameObject;
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    tSR.sprite = dictSuits[card.suit];
                }
                else
                {
                    tGO = Instantiate(prefabSprite) as GameObject;
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    ts = rankSprites[card.rank];
                    tSR.sprite = ts;
                    tSR.color = card.color;
                }

                tSR.sortingOrder = 1;
                tGO.transform.parent = cgo.transform;
                tGO.transform.localPosition = deco.loc;
                if (deco.flip) tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                if (deco.scale != 1) tGO.transform.localScale = Vector3.one * deco.scale;
                tGO.name = deco.type;
                card.decosGOs.Add(tGO);
            }

            PrintDebugMsg("Card.def = " + card.def.ToStringCD());
            foreach(Decorator pip in card.def.pips)
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                tGO.transform.parent = cgo.transform;
                tGO.transform.localPosition = pip.loc;
                if (pip.flip) tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                if (pip.scale != 1) tGO.transform.localScale = Vector3.one * pip.scale;
                tGO.name = "pip";
                tSR = tGO.GetComponent<SpriteRenderer>();
                tSR.sprite = dictSuits[card.suit];
                tSR.sortingOrder = 1;
                card.pipGOs.Add(tGO);
            }

            if(card.def.face != " ")
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();
                ts = GetFace(card.def.face + card.suit);
                tSR.sprite = ts;
                tSR.sortingOrder = 1;
                tGO.transform.parent = card.transform;
                tGO.transform.localPosition = Vector3.zero;
                tGO.name = "face";
            }

            tGO = Instantiate(prefabSprite) as GameObject;
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;
            tGO.transform.parent = card.transform;
            tGO.transform.localPosition = Vector3.zero;
            tSR.sortingOrder = 2;
            tGO.name = "back";
            card.back = tGO;

            card.FaceUp = true;
            
            cards.Add(card);
        }
    }

    public Sprite GetFace(string faceS)
    {
        foreach(Sprite tS in faceSprites)
        {
            if (tS.name == faceS) return tS;
        }

        return null;
    }
    #endregion

    #region Private

    #endregion

    #region Debug
    private void PrintDebugMsg(string msg)
    {
        if (isDebug) Debug.Log(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintWarningDebugMsg(string msg)
    {
        Debug.LogWarning(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintErrorDebugMsg(string msg)
    {
        Debug.LogError(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    #endregion

    #region Getters_Setters

    #endregion
    #endregion

    #region UnityFunctions

    #endregion

    #region Start_Update
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        PrintDebugMsg("Loaded.");
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {

    }
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {

    }
    // Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {

    }
    // LateUpdate is called every frame after all other update functions, if the Behaviour is enabled.
    void LateUpdate()
    {

    }
    #endregion
}