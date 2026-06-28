using UnityEngine;
using System.Collections;
using iBoxDB.LocalServer;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkManager : Photon.MonoBehaviour
{

    public Texture mmarow;
    public static NetworkManager Instance;
    [SerializeField]
    bool useOffline;
    GameObject __player;
    private float timer = 0f;
    void Start()
    {
        // DontDestroyOnLoad(gameObject);
        Instance = this;
        Debug.Log("NetworkManager initialized");
        
        // Auto-start connection if in offline mode and character exists
        SceneManager.sceneLoaded += (s, mode) =>
        {
            if (s.name == Menu.defaultLevel)
            {
                Debug.LogWarning($"Hogwarts scene loaded. Checking if should auto-start...");
                if (useOffline)
                {
                    Debug.LogWarning("Auto-starting offline connection...");
                    PhotonNetwork.JoinRandomRoom();
                    // startConnection();
                }
            }
        };
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 2f)
        {
            timer = 0f;

            if (__player == null)
            {
                // Only warn if we're actually in the game scene, not on main menu
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == Menu.defaultLevel)
                {
                    Debug.LogWarning("[NetworkManager] __player is null! It was destroyed or cleared.");
                }
                return;
            }

            var mainCamera = __player.transform.Find("Main Camera");
            if (mainCamera != null)
            {
                Debug.Log($"[NetworkManager] Main Camera found at position: {mainCamera.position}");
            }
            else
            {
                Debug.LogError("[NetworkManager] Main Camera not found as child of player!");
            }
        }
    }

    public static void validateGameVersion()
    {
        // http://answers.unity3d.com/questions/792342/how-to-validate-ssl-certificates-when-using-httpwe.html
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        string latestVersion = (new System.Net.WebClient()).DownloadString("https://raw.githubusercontent.com/OpenHogwarts/hogwarts/master/latest_build.txt").Trim();

        if (Menu.GAME_VERSION != latestVersion)
        {
            Application.Quit();
            throw new System.Exception("Please download the latest build " + Menu.GAME_VERSION + " <-> " + latestVersion);
        }
    }

    public void startConnection()
    {
        if (useOffline)
        {
            if (PhotonNetwork.offlineMode) return;// Execute once.
            PhotonNetwork.offlineMode = true;
            OnJoinedLobby();
            return;
        }

        PhotonNetwork.ConnectUsingSettings(Menu.GAME_VERSION);

    }

    public void spawnPlayer()
    {
        // var firstJoin = GameObject.Find("SpawnPoints/FirstJoin");//"FirstJoin"
        // var position = firstJoin.transform.position;//(633.51, 161.38, 415.70)

        Debug.Log("[NetworkManager.spawnPlayer()] ===== STARTING =====");
        Debug.Log("spawnPlayer() called");
        Debug.Log($"Offline Mode: {PhotonNetwork.offlineMode}");
        
        CharacterData character = Service.db.Select<CharacterData>("FROM characters").FirstOrDefault();
        Debug.Log($"Character found: {(character != null ? character.name : "NULL")}");

        if (character == null)
        {
            Debug.LogError("No character data found! Cannot spawn player.");
            return;
        }

        Debug.Log("[NetworkManager.spawnPlayer()] About to enter try block");
        
        try
        {
            Debug.Log("[NetworkManager.spawnPlayer()] Inside try block, about to instantiate");
            GameObject player = PhotonNetwork.Instantiate(
                "Characters/Player",
                new(633.51f, 161.38f, 415.70f),
                Quaternion.identity, 0);

            Debug.LogWarning("[NetworkManager.spawnPlayer()] PhotonNetwork.Instantiate() returned");

            if (player == null)
            {
                Debug.LogError("[NetworkManager] PhotonNetwork.Instantiate returned NULL!");
                return;
            }

            Debug.LogWarning("[NetworkManager.spawnPlayer()] Player is not null");

            __player = player;
            Debug.LogWarning($"[NetworkManager] Player instantiated and stored. ID: {__player.GetInstanceID()}");

            // Disable TimedObjectDestruction if it's on the player or children
            var timedDestruction = player.GetComponent<TimedObjectDestruction>();
            if (timedDestruction != null)
            {
                timedDestruction.enabled = false;
                Debug.LogWarning("[NetworkManager] TimedObjectDestruction disabled on player!");
            }

            // Also check children
            var timedDestructions = player.GetComponentsInChildren<TimedObjectDestruction>();
            foreach (var td in timedDestructions)
            {
                td.enabled = false;
                Debug.LogWarning($"[NetworkManager] TimedObjectDestruction disabled on child: {td.gameObject.name}");
            }

            if (player != null){
                Debug.LogWarning($"Player instantiated at position: {player.transform.position}");
            }

            var playerComponent = player.GetComponent<Player>();
            if (playerComponent == null)
            {
                Debug.LogError("[NetworkManager] Player component not found on instantiated prefab!");
                return;
            }
            playerComponent.characterData = character;

        // Set camera target for CameraController
        var mainCamera = player.transform.Find("Main Camera");
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found as child of player, searching for any Camera component");
            var cameraInPlayer = player.GetComponentInChildren<Camera>();
            if (cameraInPlayer != null)
            {
                mainCamera = cameraInPlayer.transform;
            }
        }
        
        if (mainCamera != null)
        {
            
            Debug.Log($"Main Camera found at position: {mainCamera.position}");
            
            var cameraComponent = mainCamera.GetComponent<Camera>();
            CameraChecker.SetCamera(cameraComponent);
            if (cameraComponent != null)
            {
                cameraComponent.enabled = true;
                Debug.LogWarning($"Camera component enabled. Active: {cameraComponent.isActiveAndEnabled}, Near clip: {cameraComponent.nearClipPlane}, Far clip: {cameraComponent.farClipPlane}, Tag: {mainCamera.gameObject.tag}");
                Debug.LogWarning($"Camera render flags: {cameraComponent.clearFlags}, Culling mask: {cameraComponent.cullingMask}");
            }
            else
            {
                Debug.LogError("No Camera component found on Main Camera GameObject!");
            }
            
            var cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.cameraTarget = player.transform;
                
                // Fix: If desiredDistance is 0, set it to 6 so camera is not inside player
                if (cameraController.desiredDistance <= 0)
                {
                    cameraController.desiredDistance = 6;
                    Debug.Log("Camera desiredDistance was 0, set to 6");
                }
                
                Debug.Log($"Camera target set to player. Player pos: {player.transform.position}");
                
                // Force camera to update
                cameraController.enabled = true;
            }
            else
            {
                Debug.LogWarning("CameraController not found on Main Camera");
            }
            
            mainCamera.gameObject.SetActive(true);
            Debug.Log($"Main Camera GameObject activated. Camera position: {mainCamera.position}, Player position: {player.transform.position}");
        }
        else
        {
            Debug.LogError("Could not find camera on player!");
        }

        // Enable components with null checks
        var thirdPersonControl = player.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
        if (thirdPersonControl != null)
        {
            thirdPersonControl.enabled = true;
            Debug.Log("[NetworkManager] ThirdPersonUserControl enabled");
        }
        else
        {
            Debug.LogWarning("[NetworkManager] ThirdPersonUserControl component not found on player!");
        }

        var thirdPersonChar = player.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
        if (thirdPersonChar != null)
        {
            thirdPersonChar.enabled = true;
            Debug.Log("[NetworkManager] ThirdPersonCharacter enabled");
        }
        else
        {
            Debug.LogWarning("[NetworkManager] ThirdPersonCharacter component not found on player!");
        }

        var playerHotkeys = player.GetComponent<PlayerHotkeys>();
        if (playerHotkeys != null)
        {
            playerHotkeys.enabled = true;
            Debug.Log("[NetworkManager] PlayerHotkeys enabled");
        }
        else
        {
            Debug.LogWarning("[NetworkManager] PlayerHotkeys component not found on player!");
        }

        var playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.enabled = true;
            Debug.Log("[NetworkManager] PlayerCombat enabled");
        }
        else
        {
            Debug.LogWarning("[NetworkManager] PlayerCombat component not found on player!");
        }
        player.transform.Find("NamePlate").gameObject.SetActive(false);

        // Set minimap target (with null checks)
        var miniMapCamera = GameObject.Find("MiniMapCamera");
        if (miniMapCamera != null)
        {
            var miniMap = miniMapCamera.GetComponent<MiniMap>();
            if (miniMap != null)
            {
                miniMap.target = player.transform;
            }
        }

        var miniMapElementsCamera = GameObject.Find("MiniMapElementsCamera");
        if (miniMapElementsCamera != null)
        {
            var miniMapElements = miniMapElementsCamera.GetComponent<MiniMap>();
            if (miniMapElements != null)
            {
                miniMapElements.target = player.transform;
            }
        }

        var configObj = GameObject.Find("Canvas/TopMenu/Config");
        var configMenu = configObj?.GetComponent<ConfigMenu>();
        if (configMenu is { }) configMenu.player = player;

        // Set indicator texture with null checks
        var indicator = player.transform.Find("Indicator");
        if (indicator != null)
        {
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null && mmarow != null)
            {
                renderer.material.mainTexture = mmarow;
                Debug.Log("[NetworkManager] Indicator texture set");
            }
            else
            {
                Debug.LogWarning("[NetworkManager] Renderer component or mmarow texture is null!");
            }
        }
        else
        {
            Debug.LogWarning("[NetworkManager] Indicator child object not found!");
        }
        
        Debug.LogWarning("[NetworkManager] spawnPlayer() completed successfully");
        Debug.Log("[NetworkManager.spawnPlayer()] About to exit try block - NO EXCEPTION");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NetworkManager] *** EXCEPTION CAUGHT IN spawnPlayer() ***");
            Debug.LogError($"[NetworkManager] Exception Type: {ex.GetType().Name}");
            Debug.LogError($"[NetworkManager] Exception Message: {ex.Message}");
            Debug.LogError($"[NetworkManager] Full Stack Trace:\n{ex.StackTrace}");
            Debug.LogError($"[NetworkManager] Environment Stack Trace:\n{System.Environment.StackTrace}");
            
            if (__player != null)
            {
                Debug.LogError($"[NetworkManager] Destroying player due to exception!");
                Destroy(__player);
                __player = null;
            }
        }
        
        Debug.Log("[NetworkManager.spawnPlayer()] ===== COMPLETED =====");
    }
    
    /*
	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		chat.sendMessage(player.name + " left the game");
	}

	void OnPhotonPlayerConnect(PhotonPlayer player)
	{
		chat.sendMessage(player.name + " joined the game");
	}*/

    void OnJoinedLobby()
    {
        Debug.LogWarning("OnJoinedLobby() called");
        PhotonNetwork.LoadLevel(Menu.defaultLevel);
        // PhotonNetwork.JoinRandomRoom();
        //Menu.Instance.showPanel("LoadingPanel");
    }
    //private void OnLevelWasLoaded(int level)
    //{
    //    spawnPlayer();
    //}
    void OnJoinedRoom()
    {
        Debug.LogWarning("OnJoinedRoom() called - about to spawn player");
        spawnPlayer();
    }
    
    void OnPhotonJoinRandomFailed(object[] codeAndMsg)
    {
        Debug.LogError("OnPhotonJoinRandomFailed called: " + codeAndMsg[0]);
    }

    void OnCreatedRoom()
    {
        //OnJoinedRoom ();
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }


    public void OnPhotonCreateRoomFailed()
    {

        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed()
    {

        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {

        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
    }

    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
}
