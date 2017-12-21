// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PlayerType
{
    human,
    ai
}

[System.Serializable]
public class Player
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Player";
    #endregion

    #region Static

    #endregion

    #region Public
    public PlayerType type = PlayerType.ai;
    public int playerNum;

    public List<CardBartok> hand;

    public SlotDefBartok handSLotDef;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();
        hand.Add(eCB);

        if(type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();
            cards = cards.OrderBy(cd => cd.rank).ToArray();
            hand = new List<CardBartok>(cards);
        }

        eCB.SetSortingLayerName("10");
        eCB.eventualSortLayer = handSLotDef.layerName;

        FanHand();
        return eCB;
    }

    public CardBartok RemoveCard(CardBartok cb)
    {
        hand.Remove(cb);
        FanHand();
        return cb;
    }

    public void FanHand()
    {
        float startRot = handSLotDef.rot;
        if (hand.Count > 1) startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;

        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for(int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);

            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

            pos = rotQ * pos;

            pos += handSLotDef.pos;
            pos.z = -.5f * i;

            if (Bartok.S.phase != TurnPhase.idle) hand[i].timeStart = 0;

            hand[i].MoveTo(pos, rotQ);
            hand[i].state = CBState.toHand;
            /*
            hand[i].transform.localPosition = pos;
            hand[i].transform.rotation = rotQ;
            hand[i].state = CBState.hand;*/

            hand[i].FaceUp = (type == PlayerType.human);

            hand[i].eventualSortOrder = i * 4;
            //hand[i].SetSortOrder(i * 4);
        }
    }

    public void TakeTurn()
    {
        Utils.tr(Utils.RoundToPlaces(Time.time), "Player.TakeTurn()");

        if (type == PlayerType.human) return;

        Bartok.S.phase = TurnPhase.waiting;

        CardBartok cb;

        List<CardBartok> validCards = new List<CardBartok>();
        foreach(CardBartok tCB in hand)
        {
            validCards.Add(tCB);
        }
        if(validCards.Count == 0)
        {
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }

        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallback(CardBartok tCB)
    {
        CardBartok cb;
        Utils.tr(Utils.RoundToPlaces(Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
        cb = this.AddCard(Bartok.S.Draw());
        cb.callbackPlayer = this;
    }

    public void CBCallback2(CardBartok tCB)
    {
        Utils.tr(Utils.RoundToPlaces(Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
        Bartok.S.PassTurn();
    }
    #endregion

    #region Private

    #endregion

    #region Debug
    private void PrintDebugMsg(string msg)
    {
        if (isDebug) Debug.Log(debugScriptName + "): " + msg);
    }
    private void PrintWarningDebugMsg(string msg)
    {
        Debug.LogWarning(debugScriptName + "): " + msg);
    }
    private void PrintErrorDebugMsg(string msg)
    {
        Debug.LogError(debugScriptName + "): " + msg);
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