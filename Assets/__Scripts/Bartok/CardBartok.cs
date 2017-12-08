// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CBState
{
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card
{
    #region GlobalVareables
    #region Static
    public static float MOVE_DURATION = .5f;
    public static string MOVE_EASING = Easing.InOut;
    public static float CARD_HEIGHT = 3.5f;
    public static float CARD_WIDTH = 2;
    #endregion

    #region Public
    public CBState state = CBState.drawpile;
    public List<Vector3> bezierPts;
    public List<Quaternion> bezierRots;
    public float timeStart, timeDuration;
    public GameObject reportToFinish = null;
    public int eventualSortOrder;
    public string eventualSortLayer;
    public Player callbackPlayer = null;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition);
        bezierPts.Add(ePos);
        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);
        bezierRots.Add(eRot);

        if (timeStart == 0) timeStart = Time.time;
        timeDuration = MOVE_DURATION;

        state = CBState.to;
    }

    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }

    public override void OnMouseUpAsButton()
    {
        Bartok.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
    #endregion

    #region Private

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
        callbackPlayer = null;
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
        switch(state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);

                if(u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                }
                else if(u >= 1)
                {
                    uC = 1;
                    if (state == CBState.toHand) state = CBState.hand;
                    if (state == CBState.toTarget) state = CBState.toTarget;
                    if (state == CBState.to) state = CBState.idle;
                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierPts.Count - 1];
                    timeStart = 0;

                    if (reportToFinish != null)
                    {
                        reportToFinish.SendMessage("CBCallback", this);
                        reportToFinish = null;
                    }
                    else if(callbackPlayer != null)
                    {
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;
                    }
                }
                else
                {
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;

                    if (u > .5f && spriteRenderers[0].sortingOrder != eventualSortOrder) SetSortOrder(eventualSortOrder);
                    if (u > .75f && spriteRenderers[0].sortingLayerName != eventualSortLayer) SetSortingLayerName(eventualSortLayer);
                }
                break;
        }
    }
    // LateUpdate is called every frame after all other update functions, if the Behaviour is enabled.
    void LateUpdate()
    {

    }
    #endregion
}