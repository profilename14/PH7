using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string sceneToLoadOnRespawn;

    public Vector3 respawnPosition;

    public Transform defaultSceneRespawn;

    public List<Dictionary<int, bool>> collectablesObtained;
    public List<Dictionary<int, bool>> collectablesDamageObtained;
    public List<Dictionary<int, bool>> clearedCombatRooms;
    public int soapstones = 0;
    public int lapis = 0;
    public int damageUpgrade = 0; // Each point addes 33% damage rounded up
    public bool dashUnlocked = false;
    public bool bubbleUnlocked = false;


    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);

        DontDestroyOnLoad(this.gameObject);
        sceneToLoadOnRespawn = SceneManager.GetActiveScene().name;
        if(defaultSceneRespawn != null) respawnPosition = defaultSceneRespawn.position;

        initializeSaveDataLists();
        Player.instance.uiManager.UpdateSoapstones(GameManager.instance.soapstones);
        Player.instance.uiManager.UpdateLapis(GameManager.instance.lapis);
    }

    public void LoadNewScene(string scene, string destinationDoorId)
    {
        StartCoroutine(SetupScene(scene, destinationDoorId));
    }

    public IEnumerator SetupScene(string scene, string destinationDoorId)
    {
        var asyncLoadLevel = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

        while (!asyncLoadLevel.isDone)
        {
            yield return null;
        }

        yield return null;

        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        foreach (GameObject g in doors)
        {
            Door d = g.GetComponent<Door>();
            if (d != null && d.thisDoorId == destinationDoorId)
            {
                Player.instance.WarpPlayer(d.typhisEntranceTransform.position);
            }
        }

        Player.instance.uiManager.UpdateLapis(GameManager.instance.lapis);
        Player.instance.uiManager.UpdateSoapstones(GameManager.instance.soapstones);

    }

    public void PlayerRespawn()
    {
        StartCoroutine(LoadRespawnScene());
    }

    public IEnumerator LoadRespawnScene()
    {
        var asyncLoadLevel = SceneManager.LoadSceneAsync(sceneToLoadOnRespawn, LoadSceneMode.Single);

        while (!asyncLoadLevel.isDone)
        {
            yield return null;
        }

        yield return null;

        Player.instance.WarpPlayer(respawnPosition);
        Player.instance.uiManager.UpdateSoapstones(GameManager.instance.soapstones);
        Player.instance.uiManager.UpdateLapis(GameManager.instance.lapis);
    }

    void initializeSaveDataLists()
    {
        collectablesObtained = new List<Dictionary<int, bool>>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            collectablesObtained.Add(new Dictionary<int, bool>()); // Add a row for every scene. Dependent on build order (breaks saves)
        }

        collectablesDamageObtained = new List<Dictionary<int, bool>>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            collectablesDamageObtained.Add(new Dictionary<int, bool>()); // Add a row for every scene. Dependent on build order (breaks saves)
        }

        clearedCombatRooms = new List<Dictionary<int, bool>>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            clearedCombatRooms.Add(new Dictionary<int, bool>()); // Add a row for every scene. Dependent on build order (breaks saves)
        }
    }
}
