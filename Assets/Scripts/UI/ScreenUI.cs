using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(MatchStartData))]
public class ScreenUI : MonoBehaviour
{
    public TextAsset MatchesFile;
    public TextAsset LevelsFile;

    public Texture2D BackgroundTexture;
    public Texture2D LeftArrowTexture;
    public Texture2D RightArrowTexture;

    public Texture2D PlayerTexture;
    public Texture2D[] ControlsTextures = new Texture2D[7];
    public Texture2D XTexture;

    public GameObject MatchMusic = null;
    public GameObject MenuMusic = null;
    public FadeLoopNoise MusicLooper = null;

    public Texture2D[] Logos = new Texture2D[0];
    public int LogoToUse;

    private MenuScreen currentScreen;

    void Awake()
    {
        WorldConstants.ScreenUI = this;
    }

    void Start()
    {
        currentScreen = new MainMenuScreen(GetComponent<MatchStartData>(),
                                           GameObject.Find("Constants Owner").GetComponent<FrontEndConstants>(),
                                           this);
    }

    void OnGUI()
    {
        if (currentScreen.DrawBackground)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);
        }

        currentScreen = currentScreen.Update();
    }

    public void DestroyMatch(bool moveToMainMenu = true)
    {
        GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        for (int i = 0; i < all.Length; ++i)
        {
			string n = all[i].name;
			
            if (n != "Looped Sounds" && !n.Contains ("Menu Music") &&
				n != "AudioListener" && n != "Constants Owner" &&
				n != "Debugger" && n != "Match Wrapper" &&
				n != "Menu Screens")
            {				
                GameObject.Destroy(all[i]);
            }
        }

        if (moveToMainMenu)
        {
            currentScreen = new MainMenuScreen(GetComponent<MatchStartData>(), WorldConstants.ConstantsOwner.GetComponent<FrontEndConstants>(), this);
        }
    }
}