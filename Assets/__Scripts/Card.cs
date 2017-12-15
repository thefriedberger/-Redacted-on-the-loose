// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "Card";
    #endregion

    #region Static

    #endregion

    #region Public
    public string suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public List<GameObject> decosGOs = new List<GameObject>();
    public List<GameObject> pipGOs = new List<GameObject>();
    public GameObject back;
    public CardDefinition def;
    public SpriteRenderer[] spriteRenderers;
    #endregion

    #region Private

    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    public void PopulateSpriteRenderers() {
        if (spriteRenderers == null || spriteRenderers.Length == 0) spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetSortingLayerName(string tSLN) {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers) tSR.sortingLayerName = tSLN;
    }

    public void SetSortOrder(int sOrd) {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSR in spriteRenderers) {
            if (tSR.gameObject == this.gameObject) {
                tSR.sortingOrder = sOrd;
                continue;
            }

            switch (tSR.gameObject.name) {
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "face":
                default:
                    tSR.sortingOrder = sOrd+1;
                    break;
            }
        }
    }

    public virtual void OnMouseUpAsButton() {
        PrintDebugMsg(name + " was clicked!");
    }
    #endregion


    #region Debug
    private void PrintDebugMsg(string msg) {
        if (isDebug) Debug.Log(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintWarningDebugMsg(string msg) {
        Debug.LogWarning(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintErrorDebugMsg(string msg) {
        Debug.LogError(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    #endregion

    #region Getters_Setters
    public bool FaceUp {
        get {
            return !back.activeSelf;
        }
        set {
            back.SetActive(!value);
        }
    }
    #endregion
    #endregion



    #region Start_Update
    // Awake is called when the script instance is being loaded.
    void Awake() {
        PrintDebugMsg("Loaded.");
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start() {
        SetSortOrder(0);
    }
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate() {

    }
    // Update is called every frame, if the MonoBehaviour is enabled.
    void Update() {

    }
    // LateUpdate is called every frame after all other update functions, if the Behaviour is enabled.
    void LateUpdate() {

    }
    #endregion
}

[System.Serializable]
public class Decorator {
    public string type = "";
    public Vector3 loc = Vector3.zero;
    public bool flip = false;
    public float scale = 1f;
}

[System.Serializable]
public class CardDefinition {
    public string face = "";
    public int rank = 0;
    public List<Decorator> pips = new List<Decorator>();

    public string ToStringCD() {
        return "Face = " + face + " | " + "Rank = " + rank + " | " + "pips.Count = " + pips.Count;
    }
}