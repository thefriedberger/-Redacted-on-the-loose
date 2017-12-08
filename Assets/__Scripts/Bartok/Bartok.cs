// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameover
}

public class Bartok : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Bartok";
    #endregion

    #region Static
    public static Bartok S;
    public static Player CURRENT_PLAYER;
    #endregion

    #region Public
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 10f;
    public int numStartingCards = 7;
    public float drawTimeStagger = .1f;
    public GameObject gtGameOver;
    public GameObject gtRoundResult;
    public GUIText gtOlympusCount;
    public int olympusCount = 0;

    [Header("For debug use only")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;

    public BartokLayout layout;
    public Transform layoutAnchor;

    public List<Player> players;
    public CardBartok targetCard;

    public TurnPhase phase = TurnPhase.idle;
    public GameObject turnLight;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void ArrangeDrawPile()
    {
        CardBartok tCB;

        for(int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.parent = layoutAnchor;
            tCB.transform.localPosition = layout.drawPile.pos;
            tCB.FaceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-1 * 4);
            tCB.state = CBState.drawpile;
        }
    }

    public CardBartok Draw()
    {
        CardBartok cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    public void DrawFirstTarget()
    {
        CardBartok tCB = MoveToTarget(Draw());
        tCB.reportToFinish = this.gameObject;
    }

    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.FaceUp = true;
        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null) MoveToDiscard(targetCard);
        targetCard = tCB;
        olympusCount = olympusCount + targetCard.rank;
        gtOlympusCount.text = olympusCount.ToString();


        return tCB;
    }

    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

        return tCB;
    }

    public void CBCallback(CardBartok cb)
    {
        Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CBCallback()", cb.name);
        StartGame();
    }

    public void StartGame()
    {
        PassTurn(1);
    }

    public void PassTurn(int num = -1)
    {
        if(num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;
            if (CheckGameOver()) return;
        }
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;

        CURRENT_PLAYER.TakeTurn();

        Vector3 lPos = CURRENT_PLAYER.handSLotDef.pos + Vector3.back * 5;
        turnLight.transform.position = lPos;

        Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
    }

    public bool ValidPlay(CardBartok cb)
    {
        if (cb.rank == targetCard.rank) return true;

        if (cb.suit == targetCard.suit) return true;

        return false;
    }

    public void CardClicked(CardBartok tCB)
    {
        if (CURRENT_PLAYER.type != PlayerType.human) return;
        if (phase == TurnPhase.waiting) return;

        switch(tCB.state)
        {
            case CBState.drawpile:
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;
            case CBState.hand:
                if (ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CardClicked()", "Play", tCB.name, targetCard.name + " is target");
                    phase = TurnPhase.waiting;
                    olympusCount = olympusCount + targetCard.rank;
                    gtOlympusCount.text = olympusCount.ToString();
                    
}
                else Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
                break;
        }
    }

    public bool CheckGameOver()
    {
        if(drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardBartok cb in discardPile) cards.Add(cb);
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardsList(cards);
            ArrangeDrawPile();
        }

        if(CURRENT_PLAYER.hand.Count == 0)
        {
            if(CURRENT_PLAYER.type == PlayerType.human)
            {
                gtGameOver.GetComponent<GUIText>().text = "You Won!";
                gtRoundResult.GetComponent<GUIText>().text = "";
            }
            else
            {
                gtGameOver.GetComponent<GUIText>().text = "Game Over";
                gtRoundResult.GetComponent<GUIText>().text = "Player " + CURRENT_PLAYER.playerNum + " won";
            }
            gtGameOver.SetActive(true);
            gtRoundResult.SetActive(true);
            phase = TurnPhase.gameover;
            Invoke("RestartGame", 1);
            return true;
        }

        return false;
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("bartok");
    }
    #endregion

    #region Private
    private List<CardBartok> UpgradeCardsList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (Card tCD in lCD) lCB.Add(tCD as CardBartok);
        return lCB;
    }

    private void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        ArrangeDrawPile();

        Player p1;
        players = new List<Player>();
        foreach (SlotDefBartok tSD in layout.slotDefs)
        {
            p1 = new Player();
            p1.handSLotDef = tSD;
            players.Add(p1);
            p1.playerNum = players.Count;
        }
        players[0].type = PlayerType.human;

        CardBartok tCB;
        for(int i = 0; i < numStartingCards; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                tCB = Draw();
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
                players[(j + 1) % 4].AddCard(tCB);
            }
        }

        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
    }
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

        S = this;

        turnLight = GameObject.Find("TurnLight");
        gtGameOver = GameObject.Find("GTGameOver");
        gtRoundResult = GameObject.Find("GTRoundResult");
        gtGameOver.SetActive(false);
        gtRoundResult.SetActive(false);
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<BartokLayout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = UpgradeCardsList(deck.cards);
        LayoutGame();


    }


    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {

    }
    // Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {/*
        if (Input.GetKeyDown(KeyCode.Alpha1)) players[0].AddCard(Draw());
        if (Input.GetKeyDown(KeyCode.Alpha2)) players[1].AddCard(Draw());
        if (Input.GetKeyDown(KeyCode.Alpha3)) players[2].AddCard(Draw());
        if (Input.GetKeyDown(KeyCode.Alpha4)) players[3].AddCard(Draw());*/
    }
    // LateUpdate is called every frame after all other update functions, if the Behaviour is enabled.
    void LateUpdate()
    {

    }
    #endregion
}