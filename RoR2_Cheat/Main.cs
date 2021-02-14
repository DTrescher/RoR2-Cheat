using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.SceneManagement;

namespace RoR2_Cheat
{
    public class Main : MonoBehaviour
    {
        public static List<PurchaseInteraction> purchaseInteractions = new List<PurchaseInteraction>();
        public static List<BarrelInteraction> barrelInteractions = new List<BarrelInteraction>();
        public static List<PressurePlateController> secretButtons = new List<PressurePlateController>();
        public static List<ScrapperController> scrappers = new List<ScrapperController>();

        public static bool onRenderIntEnable = true, renderMobs, renderInteractables, renderMods = false;

        private static GUIStyle labelStyle, titleStyle, activeModsStyle, renderTeleporterStyle, renderMobsStyle, renderInteractablesStyle, renderSecretsStyle, watermarkStyle, statsStyle, selectedChestStyle;

        private static void BuildStyles()
        {
            labelStyle = CreateGUIStyle(null, null, Color.grey, 18, FontStyle.Normal, TextAnchor.UpperCenter);
            statsStyle = CreateGUIStyle(null, null, Color.grey, 18, FontStyle.Normal, TextAnchor.MiddleLeft);
            titleStyle = CreateGUIStyle(null, null, Color.HSVToRGB(0.5256f, 0.9286f, 0.9333f), 18, FontStyle.Normal, TextAnchor.UpperCenter);
            activeModsStyle = CreateGUIStyle(null, null, Color.HSVToRGB(0.5256f, 0.9286f, 0.9333f), 18, FontStyle.Normal, TextAnchor.MiddleLeft, true);
            renderInteractablesStyle = CreateGUIStyle(null, null, Color.green, 14, FontStyle.Normal, TextAnchor.MiddleLeft);
            renderSecretsStyle = CreateGUIStyle(null, null, Color.HSVToRGB(0.5065f, 1.0000f, 1.0000f), 14, FontStyle.Normal, TextAnchor.MiddleLeft);
            renderTeleporterStyle = CreateGUIStyle(null, null, Color.white, 14, FontStyle.Normal, TextAnchor.MiddleLeft);
            renderMobsStyle = CreateGUIStyle(null, null, Color.red, 14, FontStyle.Normal, TextAnchor.MiddleLeft);
            selectedChestStyle = CreateGUIStyle(null, null, Color.blue, 14, FontStyle.Normal, TextAnchor.MiddleRight);
            watermarkStyle = CreateGUIStyle(null, null, Color.HSVToRGB(0.5256f, 0.9286f, 0.9333f), 14, FontStyle.Normal, TextAnchor.MiddleLeft);
        }

        public static GUIStyle CreateGUIStyle(Texture2D normalBackground, Texture2D activeBackground, Color color, int fontSize, FontStyle font, TextAnchor textAlignmnet, bool wrapWords = false)
        {
            GUIStyle GUIStyle = new GUIStyle();
            GUIStyle.normal.background = normalBackground;
            GUIStyle.onNormal.background = normalBackground;
            GUIStyle.active.background = activeBackground;
            GUIStyle.onActive.background = activeBackground;
            GUIStyle.normal.textColor = color;
            GUIStyle.onNormal.textColor = color;
            GUIStyle.active.textColor = color;
            GUIStyle.onActive.textColor = color;
            GUIStyle.fontSize = fontSize;
            GUIStyle.fontStyle = font;
            GUIStyle.alignment = textAlignmnet;
            GUIStyle.wordWrap = wrapWords;
            return GUIStyle;
        }

        public void Start()
        {
            BuildStyles();

            SceneManager.activeSceneChanged += OnSceneLoaded;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleRenderInteractables();
            }
        }

        public static void DumpInteractables(SceneDirector obj)
        {
            barrelInteractions = MonoBehaviour.FindObjectsOfType<BarrelInteraction>().ToList();
            purchaseInteractions = MonoBehaviour.FindObjectsOfType<PurchaseInteraction>().ToList();
            secretButtons = MonoBehaviour.FindObjectsOfType<PressurePlateController>().ToList();
            scrappers = MonoBehaviour.FindObjectsOfType<ScrapperController>().ToList();
        }

        private void ToggleRenderInteractables()
        {
            if (renderInteractables)
            {
                DisableInteractables();
            }
            else
            {
                EnableInteractables();
            }
            renderInteractables = !renderInteractables;
        }

        public static void EnableInteractables()
        {
            if (onRenderIntEnable)
            {
                DumpInteractables(null);
                SceneDirector.onPostPopulateSceneServer += DumpInteractables;
                onRenderIntEnable = false;
            }
        }

        public static void DisableInteractables()
        {
            if (!onRenderIntEnable)
            {
                SceneDirector.onPostPopulateSceneServer -= DumpInteractables;
                onRenderIntEnable = true;
            }
        }

        public void OnSceneLoaded(Scene s1, Scene s2)
        {
            if (s2 != null)
            {
                bool inGame = s2.name != "title" && s2.name != "lobby" && s2.name != "" && s2.name != " ";
                if (inGame)
                {
                    if (renderInteractables)
                    {
                        DumpInteractables(null);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (renderInteractables)
            {
                ESPRoutine();
            }
        }

        private void ESPRoutine()
        {
            if (TeleporterInteraction.instance)
            {
                var teleporterInteraction = TeleporterInteraction.instance;
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, teleporterInteraction.transform.position);
                Vector3 Position = Camera.main.WorldToScreenPoint(teleporterInteraction.transform.position);
                var BoundingVector = new Vector3(Position.x, Position.y, Position.z);
                if (BoundingVector.z > 0.01)
                {
                    renderTeleporterStyle.normal.textColor =
                        teleporterInteraction.isIdle ? Color.magenta :
                        teleporterInteraction.isIdleToCharging || teleporterInteraction.isCharging ? Color.yellow :
                        teleporterInteraction.isCharged ? Color.green : Color.yellow;
                    int distance = (int)distanceToObject;
                    String friendlyName = "Teleporter";
                    string status = "" + (
                        teleporterInteraction.isIdle ? "Idle" :
                        teleporterInteraction.isCharging ? "Charging" :
                        teleporterInteraction.isCharged ? "Charged" :
                        teleporterInteraction.isActiveAndEnabled ? "Idle" :
                        teleporterInteraction.isIdleToCharging ? "Idle-Charging" :
                        teleporterInteraction.isInFinalSequence ? "Final-Sequence" :
                        "???");
                    string boxText = $"{friendlyName}\n{status}\n{distance}m";
                    GUI.Label(new Rect(BoundingVector.x - 50f, (float)Screen.height - BoundingVector.y, 100f, 50f), boxText, renderTeleporterStyle);
                }
            }

            foreach (var barrel in barrelInteractions)
            {
                if (!barrel.Networkopened)
                {
                    string friendlyName = "Barrel";
                    Vector3 Position = Camera.main.WorldToScreenPoint(barrel.transform.position);
                    var BoundingVector = new Vector3(Position.x, Position.y, Position.z);
                    if (BoundingVector.z > 0.01)
                    {
                        float distance = (int)Vector3.Distance(Camera.main.transform.position, barrel.transform.position);
                        string boxText = $"{friendlyName}\n{distance}m";
                        GUI.Label(new Rect(BoundingVector.x - 50f, (float)Screen.height - BoundingVector.y, 100f, 50f), boxText, renderInteractablesStyle);
                    }
                }
            }

            foreach (var secretButton in secretButtons)
            {
                if (secretButton)
                {
                    string friendlyName = "Secret Button";
                    Vector3 Position = Camera.main.WorldToScreenPoint(secretButton.transform.position);
                    var BoundingVector = new Vector3(Position.x, Position.y, Position.z);
                    if (BoundingVector.z > 0.01)
                    {
                        float distance = (int)Vector3.Distance(Camera.main.transform.position, secretButton.transform.position);
                        string boxText = $"{friendlyName}\n{distance}m";
                        GUI.Label(new Rect(BoundingVector.x - 50f, (float)Screen.height - BoundingVector.y, 100f, 50f), boxText, renderSecretsStyle);
                    }
                }
            }

            foreach (var scrapper in scrappers)
            {
                if (scrapper)
                {
                    string friendlyName = "Scrapper";
                    Vector3 Position = Camera.main.WorldToScreenPoint(scrapper.transform.position);
                    var BoundingVector = new Vector3(Position.x, Position.y, Position.z);
                    if (BoundingVector.z > 0.01)
                    {
                        float distance = (int)Vector3.Distance(Camera.main.transform.position, scrapper.transform.position);
                        string boxText = $"{friendlyName}\n{distance}m";
                        GUI.Label(new Rect(BoundingVector.x - 50f, (float)Screen.height - BoundingVector.y, 100f, 50f), boxText, renderInteractablesStyle);
                    }
                }
            }

            foreach (var purchaseInteraction in purchaseInteractions)
            {
                if (purchaseInteraction.available)
                {
                    string dropName = null;
                    /*var chest = purchaseInteraction?.gameObject.GetComponent<ChestBehavior>();
                    if (chest)
                    {
                        dropName = Util.GenerateColoredString(Language.GetString(chest.GetField<PickupIndex>("dropPickup").GetPickupNameToken()), chest.GetField<PickupIndex>("dropPickup").GetPickupColor());
                    }*/
                    float distanceToObject = Vector3.Distance(Camera.main.transform.position, purchaseInteraction.transform.position);
                    Vector3 Position = Camera.main.WorldToScreenPoint(purchaseInteraction.transform.position);
                    var BoundingVector = new Vector3(Position.x, Position.y, Position.z);
                    if (BoundingVector.z > 0.01)
                    {
                        int distance = (int)distanceToObject;
                        String friendlyName = purchaseInteraction.GetDisplayName();
                        int cost = purchaseInteraction.cost;
                        string boxText = dropName != null ? $"{friendlyName}\n${cost}\n{distance}m\n{dropName}" : $"{friendlyName}\n${cost}\n{distance}m";
                        GUI.Label(new Rect(BoundingVector.x - 50f, (float)Screen.height - BoundingVector.y, 100f, 50f), boxText, renderInteractablesStyle);
                    }
                }
            }
        }
    }
}
