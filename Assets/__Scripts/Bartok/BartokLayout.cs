// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SlotDefBartok
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public float rot;
    public string type = "slot";
    public Vector2 stagger;
    public int player;
    public Vector3 pos;
}

public class BartokLayout : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "BartokLayout";
    #endregion

    #region Static

    #endregion

    #region Public
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;
    public List<SlotDefBartok> slotDefs;
    public SlotDefBartok drawPile;
    public SlotDefBartok discardPile;
    public SlotDefBartok target;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        SlotDefBartok tSD;
        PT_XMLHashList slotsX = xml["slot"];

        for(int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDefBartok();
            if (slotsX[i].HasAtt("type")) tSD.type = slotsX[i].att("type");
            else tSD.type = "slot";

            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch(tSD.type)
            {
                case "slot":
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "target":
                    target = tSD;
                    break;
                case "hand":
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = float.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
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