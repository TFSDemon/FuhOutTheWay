using BepInEx;
using GorillaGameModes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FuhOutTheWay
{
    [BepInPlugin("com.tfsdemon.fuhouttheway", "FuhOutTheWay", "1.2.1")]
   
    public class Plugin : BaseUnityPlugin
    {
        void Awake() => gameObject.AddComponent<TransparencyController>();
    }

    public class TransparencyController : MonoBehaviour
    {
        bool modEnabled = true;
        bool showGui = true;
        float alpha = 0.45f;
        float nextScan;
        Rect win = new Rect(12, 12, 260, 120);

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame)
                showGui = !showGui;

            if (!modEnabled) return;
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1f;

            if (!IsCasual()) return;
            ApplyOthers(alpha);
        }

        void OnGUI()
        {
            if (!showGui) return;
            win = GUI.Window(96001, win, id =>
            {
                modEnabled = GUILayout.Toggle(modEnabled, "Enabled");
                GUILayout.Label(IsCasual() ? "Casual — active" : "Not casual — off");
                GUILayout.Label(string.Format("Alpha {0:0.00}", alpha));
                alpha = GUILayout.HorizontalSlider(alpha, 0.1f, 1f);
                GUILayout.Label("F6 — hide GUI");
                GUI.DragWindow();
            }, "FuhOutTheWay");
        }

        static bool IsCasual()
        {
            try
            {
                return GameMode.CurrentGameModeType == GameModeType.Casual
                    || GameMode.CurrentGameModeType == GameModeType.SuperCasual;
            }
            catch { return false; }
        }

        static void ApplyOthers(float a)
        {
            VRRig local = null;
            try { local = GorillaTagger.Instance?.offlineVRRig; } catch { }

            VRRig[] rigs = Object.FindObjectsOfType<VRRig>();
            for (int i = 0; i < rigs.Length; i++)
            {
                VRRig rig = rigs[i];
                if (rig == null || rig == local) continue;
                try { if (rig.isOfflineVRRig) continue; } catch { }

                if (rig.mainSkin != null)
                    Fade(rig.mainSkin, a);

                Renderer[] rs = rig.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < rs.Length; j++)
                    Fade(rs[j], a);
            }
        }

        static void Fade(Renderer r, float a)
        {
            if (r == null) return;
            Material[] mats = r.materials;
            if (mats == null) return;

            for (int i = 0; i < mats.Length; i++)
            {
                Material m = mats[i];
                if (m == null) continue;

                if (m.HasProperty("_Color"))
                {
                    Color c = m.GetColor("_Color");
                    c.a = a;
                    m.SetColor("_Color", c);
                }
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = a;
                    m.SetColor("_BaseColor", c);
                }

                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
                if (m.HasProperty("_Mode")) m.SetFloat("_Mode", 3f);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.renderQueue = 3000;
                m.EnableKeyword("_ALPHABLEND_ON");
            }
        }
    }
}
