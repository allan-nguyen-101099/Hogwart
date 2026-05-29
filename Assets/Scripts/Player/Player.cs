using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;

public class Player : Photon.MonoBehaviour {

    const int XP_BASE = 400;
    const int HEALTH_REGEN_BASE = 5;
    const int MANA_REGEN_BASE = 2;

    public int health
    {
        get { return characterData.health; }
        set {
            bool hasResurrected = false;
            //prevent negative health values
            if (value < 0) {
                value = 0;
            }

            // has resurrected
            if (health == 0 && value > 0) {
                hasResurrected = true;
            }

            // We dont check if health > maxHealth here, because player may have some temporal buff
            characterData.health = value;

            if (photonView.isMine)
            {
                characterData.save();
                PlayerPanel.Instance.updateBar(PlayerPanel.BarType.Health, characterData.health, characterData.maxHealth);
                startHealthRegeneration();

                if (hasResurrected) {
                    startManaRegeneration();
                }
            }
        }
    }

    public int maxHealth {
        get {
            return characterData.maxHealth;
        }
    }

    public int maxMana {
        get {
            return characterData.maxMana;
        }
    }

    public int exp
    {
        get { return characterData.exp; }
        set {
            //prevent negative values
            if (value < 0) {
                value = 0;
            }

            characterData.exp = value;

            if (photonView.isMine)
            {
                int nextLevelExp = XP_BASE * level;

                if (exp >= nextLevelExp) {
                    characterData.exp = exp - nextLevelExp;
                    level += 1;
                }
                characterData.save();
                PlayerPanel.Instance.showWonXP(value);
                PlayerPanel.Instance.updateBar(PlayerPanel.BarType.Exp, characterData.exp, XP_BASE * level);
            }
        }
    }

    public int mana
    {
        get { return characterData.mana; }
        set {
            //prevent negative values
            if (value < 0) {
                value = 0;
            }

            // We dont check if mana > maxMana here, because player may have some temporal buff
            characterData.mana = value;

            if (photonView.isMine) {
                characterData.save();
                PlayerPanel.Instance.updateBar(PlayerPanel.BarType.Mana, characterData.mana, characterData.maxMana);
                startManaRegeneration();
            }
        }
    }

    public int money
    {
        get { return characterData.money; }
        set {
            //prevent negative values
            if (value < 0) { value = 0; }

            characterData.money = value;

            if (photonView.isMine) {
                characterData.save();
                try {
                    Inventory.Instance.updateMoney();
                } catch (Exception) { }
            }
        }
    }

    public int level
    {
        get { return characterData.level; }
        set {
            //prevent negative values
            if (value < 0) { value = 0; }

            characterData.level = value;

            if (photonView.isMine) {
                characterData.save();
                SkillsUI.Instance.displayUnlockedSkills();
            }
        }
    }

    public bool isMine {
        get {
            return photonView.isMine;
        }
    }

    public bool isFriendly {
        get {
            return true; // this will change when we have multiple classes
        }
    }

    public int id {
        get {
            return photonView.ownerId;
        }
    }

    public new string name;
    public CharacterData characterData { 
        get; 
        set; 
    }
    private NPC _target;
    public NPC target {
        get {
            return _target;
        }
        set {
            _target = value;

            if (value == null) {
                PlayerPanel.Instance.hideTargetPanel();
            } else {
                PlayerPanel.Instance.showTargetPanel(value.transform);
            }

            SkillsUI.Instance.updateStatus();
        }
    }
    private bool healthRegenStarted = false;
    private bool manaRegenStarted = false;
    public bool isFlying = false;
    public float lastHitTime;
    public bool isInCombat {
        get {
            if (Time.time > (lastHitTime + 10)) {
                return false;
            }
            return true;
        }
        set {
            if (value) {
                lastHitTime = Time.time;
            }
        }
    }
    public bool isDead {
        get {
            if (health < 1) {
                return true;
            }
            return false;
        }
    }

    public Animator anim;
    private bool gotFirstUpdate = false;
    public Text nick;

    public Image healthBar;
    public Image healthBar2;

    public NamePlate namePlate;

	public GameObject trailRenderer;

    public static Player _instance;

    public static Player Instance {
        get
        {
            return _instance;
        }
    }

    private Vector3 correctPlayerPos = Vector3.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;
    private float creationTime;

    void OnEnable()
    {
        creationTime = Time.realtimeSinceStartup;
        Debug.LogWarning($"[Player.OnEnable()] GameObject enabled at {creationTime}");
    }

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.LogWarning($"[Player.OnPhotonInstantiate()] Called! isMine: {photonView.isMine}");
        Debug.LogWarning($"[Player.OnPhotonInstantiate()] photonView data: owner={photonView.owner}, viewID={photonView.viewID}");
    }

    void OnDisable()
    {
        Debug.LogError($"[Player.OnDisable()] GameObject disabled");
    }

    void OnDestroy()
    {
        float lifespan = Time.realtimeSinceStartup - creationTime;
        Debug.LogError($"[Player.OnDestroy()] GameObject being destroyed! isMine: {photonView.isMine}, Lifespan: {lifespan}s");
        Debug.LogError($"[Player.OnDestroy()] Full stack trace:\n{System.Environment.StackTrace}");
    }

    void Start() {
        Debug.LogWarning($"[Player.Start()] isMine: {photonView.isMine}, Owner: {photonView.owner}");
        
        try
        {
            if (photonView.isMine)
            {
                Debug.LogWarning("[Player.Start()] This is MY player, setting up owner components");
                _instance = this;
                
                try
                {
                    if (SkillsUI.Instance != null)
                    {
                        Debug.LogWarning("[Player.Start()] Calling SkillsUI.displayUnlockedSkills()");
                        SkillsUI.Instance.displayUnlockedSkills();
                        Debug.LogWarning("[Player.Start()] SkillsUI.displayUnlockedSkills() completed");
                    }
                    else
                    {
                        Debug.LogWarning("SkillsUI.Instance not found");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Player.Start()] Error in SkillsUI: {ex.Message}\n{ex.StackTrace}");
                }

                try
                {
                    Debug.LogWarning("[Player.Start()] Calling startHealthRegeneration()");
                    startHealthRegeneration();
                    Debug.LogWarning("[Player.Start()] startHealthRegeneration() completed");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Player.Start()] Error in startHealthRegeneration: {ex.Message}\n{ex.StackTrace}");
                }

                try
                {
                    Debug.LogWarning("[Player.Start()] Calling startManaRegeneration()");
                    startManaRegeneration();
                    Debug.LogWarning("[Player.Start()] startManaRegeneration() completed");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Player.Start()] Error in startManaRegeneration: {ex.Message}\n{ex.StackTrace}");
                }

                try
                {
                    if (trailRenderer != null)
                    {
                        Debug.LogWarning("[Player.Start()] Destroying trailRenderer");
                        Destroy(trailRenderer);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Player.Start()] Error destroying trailRenderer: {ex.Message}\n{ex.StackTrace}");
                }

                Debug.LogWarning("[Player.Start()] Owner setup COMPLETED SUCCESSFULLY");
            }
            else
            {
                Debug.LogWarning("[Player.Start()] This is NOT my player, destroying non-owner components");
                try
                {
                    if (Chat.Instance != null && photonView.owner != null)
                    {
                        Chat.Instance.LocalMsg("<color=\"#e8bf00\">[Sistema]</color> " + photonView.owner.NickName + " entró");
                    }
                    else
                    {
                        Debug.LogWarning($"[Player.Start()] Chat.Instance or owner is null. Chat: {Chat.Instance != null}, Owner: {photonView.owner != null}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Player.Start()] Error sending chat message: {ex.Message}");
                }

                // Destroy non-owner components with null checks
                var thirdPersonControl = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
                if (thirdPersonControl != null) Destroy(thirdPersonControl);
                
                var thirdPersonChar = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
                if (thirdPersonChar != null) Destroy(thirdPersonChar);
                
                var rigidbody = GetComponent<Rigidbody>();
                if (rigidbody != null) Destroy(rigidbody);
                
                var npcActivator = GetComponent<NPCActivator>();
                if (npcActivator != null) Destroy(npcActivator);
                
                var npcActivatorChild = gameObject.transform.Find("NPCActivator");
                if (npcActivatorChild != null) Destroy(npcActivatorChild.gameObject);
                
                Debug.LogWarning("[Player.Start()] Non-owner setup completed");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Player.Start()] FATAL EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        try
        {
            if (!photonView.isMine) {
                transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
                transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
            } else {
                if (!gotFirstUpdate) {
                    Debug.LogWarning("[Player.Update()] First update frame, setting up UI");
                    
                    try
                    {
                        Debug.LogWarning("[Player.Update()] Calling photonView.RPC setNick");
                        photonView.RPC("setNick", PhotonTargets.OthersBuffered, PhotonNetwork.player.NickName);
                        Debug.LogWarning("[Player.Update()] RPC setNick completed");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Player.Update()] Error in RPC setNick: {ex.Message}\n{ex.StackTrace}");
                    }

                    try
                    {
                        Debug.LogWarning("[Player.Update()] Finding health bar UI element");
                        healthBar = GameObject.Find("Canvas/PlayerPanel/HP Orb Bg/HP").GetComponent<Image>();
                        if (healthBar == null)
                        {
                            Debug.LogError("[Player.Update()] healthBar is NULL after finding!");
                        }
                        Debug.LogWarning("[Player.Update()] Health bar found");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Player.Update()] Error finding health bar: {ex.Message}\n{ex.StackTrace}");
                    }

                    try
                    {
                        Debug.LogWarning("[Player.Update()] Updating PlayerPanel bars");
                        if (PlayerPanel.Instance != null)
                        {
                            PlayerPanel.Instance.updateBar(PlayerPanel.BarType.Health, characterData.health, characterData.maxHealth);
                            PlayerPanel.Instance.updateBar(PlayerPanel.BarType.Exp, characterData.exp, XP_BASE * level);
                            PlayerPanel.Instance.updateBar(PlayerPanel.BarType.Mana, characterData.mana, characterData.maxMana);
                            Debug.LogWarning("[Player.Update()] PlayerPanel bars updated");
                        }
                        else
                        {
                            Debug.LogError("[Player.Update()] PlayerPanel.Instance is NULL!");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Player.Update()] Error updating PlayerPanel: {ex.Message}\n{ex.StackTrace}");
                    }

                    gotFirstUpdate = true;
                    Debug.LogWarning("[Player.Update()] First update COMPLETED");
                }

                if (healthBar != null && healthBar.fillAmount != health) {
                    healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, health / 270f, 4f * Time.deltaTime);
                    namePlate.health.fillAmount = Mathf.Lerp(namePlate.health.fillAmount, health / 270f, 4f * Time.deltaTime);
                }

                //looks like player is falling
                if (transform.position.y < -100) {
                    Respawn();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Player.Update()] FATAL EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        }
    }

    void LateUpdate()
    {
        if(gotFirstUpdate && isDead && this.anim.GetBool("Dead") == false ) {
            this.anim.SetBool("Dead", true);
            this.freeze();

            Menu.Instance.togglePanel("GameOverPanel");
            Menu.Instance.GetComponent<Animator>().SetTrigger("GameOver");
        }
    }

    public void Reborn()
    {
        Menu.Instance.GetComponent<Animator>().SetBool("GameOver", false);
        Menu.Instance.togglePanel("GameOverPanel");

        this.health = this.maxHealth / 2;
        this.anim.SetBool("Dead", false);

        transform.position = new(633.51f, 161.38f, 415.70f);//GameObject.Find("SpawnPoints/FirstJoin").transform.position;
        this.unfreeze();
    }

    public void Respawn()
    {
        transform.position = new(633.51f, 161.38f, 415.70f);//GameObject.Find("SpawnPoints/FirstJoin").transform.position;
    }

    private void startHealthRegeneration()
    {
        if (healthRegenStarted) {
            return;
        }
        StartCoroutine("regenerateHealth");
    }

    private void startManaRegeneration()
    {
        if (manaRegenStarted) {
            return;
        }
        StartCoroutine("regenerateMana");
    }


    IEnumerator regenerateHealth()
    {
        if (isDead) {
            yield break;
        }
        healthRegenStarted = true;

        while (health != maxHealth)
        {
            if (!isInCombat && !target) {
                health += 1;
            }
            if (health > maxHealth) {
                health = maxHealth;
            }
            yield return new WaitForSeconds(1f / (HEALTH_REGEN_BASE + (level / 15)));
        }
        healthRegenStarted = false;
    }

    IEnumerator regenerateMana()
    {
        if (isDead) {
            yield break;
        }
        manaRegenStarted = true;

        while (mana != maxMana)
        {
            if (!isInCombat && !target) {
                mana += 1;
            }
            if (mana > maxMana) {
                mana = maxMana;
            }
            yield return new WaitForSeconds(1f / (MANA_REGEN_BASE + (level / 15)));
        }
        manaRegenStarted = false;
    }

	private bool broom;

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        try
        {
            Debug.LogWarning($"[Player.OnPhotonSerializeView()] Called, isWriting: {stream.isWriting}, isMine: {photonView.isMine}");
            
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                Debug.LogWarning("[Player.OnPhotonSerializeView()] Writing data");
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(anim.GetFloat("Forward"));
                stream.SendNext(anim.GetFloat("Turn"));
                stream.SendNext(anim.GetFloat("Jump"));
                stream.SendNext(anim.GetBool("OnGround"));
                stream.SendNext(anim.GetBool("InvokeSpell"));
                stream.SendNext(anim.GetInteger("SpellType"));
                stream.SendNext(anim.GetBool("Broomstick"));
                Debug.LogWarning("[Player.OnPhotonSerializeView()] Writing completed");
            }
            else
            {
                // Network player, receive data
                Debug.LogWarning("[Player.OnPhotonSerializeView()] Reading data");
                this.correctPlayerPos = (Vector3)stream.ReceiveNext();
                this.correctPlayerRot = (Quaternion)stream.ReceiveNext();

                if (anim) {
                    anim.SetFloat("Forward", (float)stream.ReceiveNext());
                    anim.SetFloat("Turn", (float)stream.ReceiveNext());
                    anim.SetFloat("Jump", (float)stream.ReceiveNext());
                    anim.SetBool("OnGround", (bool)stream.ReceiveNext());
                    anim.SetBool("InvokeSpell", (bool)stream.ReceiveNext());
                    anim.SetInteger("SpellType", (int)stream.ReceiveNext());
                    broom = (bool)stream.ReceiveNext();
                    anim.SetBool("Broomstick", broom);
                }
                else
                {
                    Debug.LogWarning("[Player.OnPhotonSerializeView()] Animator is null!");
                }

                Debug.LogWarning("[Player.OnPhotonSerializeView()] About to access PlayerHotkeys");
                var playerHotkeys = gameObject.GetComponent<PlayerHotkeys>();
                if (playerHotkeys != null && playerHotkeys.broom != null)
                {
                    if (broom) {
                        playerHotkeys.broom.SetActive(true);
                    } else {
                        playerHotkeys.broom.SetActive(false);
                    }
                    Debug.LogWarning("[Player.OnPhotonSerializeView()] Broom status updated");
                }
                else
                {
                    Debug.LogWarning($"[Player.OnPhotonSerializeView()] PlayerHotkeys or broom is null! PlayerHotkeys: {playerHotkeys != null}, broom: {(playerHotkeys != null ? playerHotkeys.broom != null : false)}");
                }

                if (gotFirstUpdate == false) {
                    transform.position = this.correctPlayerPos;
                    transform.rotation = this.correctPlayerRot;
                    gotFirstUpdate = true;
                }
                
                Debug.LogWarning("[Player.OnPhotonSerializeView()] Reading completed");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Player.OnPhotonSerializeView()] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [PunRPC]
    void setNick(string nickName) {
        //name = nickName;
        namePlate.setName(nickName, NamePlate.COLOR_NORMAL);
		namePlate.gameObject.SetActive (true);
    }

    private static Vector3 getVector3(string rString) {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
        Vector3 rValue = new Vector3(x, y, z);
        return rValue;
    }

    /**
	* Adds an item to current user
	*
	* @param int id item id
	* @param int amount item quantity
	* 
	* @return bool
	 */
    public bool addItem(int id, int amount = 1)
    {
        bool success;
        CharacterItem characterItem = Service.getOne<CharacterItem>("FROM inventory WHERE item == ?", id);

        if (characterItem != null) {
            characterItem.quantity += amount;
            characterItem.save();
            success = true;
        } else {
            CharacterItem item = new CharacterItem {
                item = id,
                quantity = amount
            };
            success = item.create();
        }
        // reload inventory
        if (success) {
            try {
                QuestManager.Instance.sendAction(id, Task.ActorType.Item, Task.ActionType.GetItem, amount);
                Inventory.Instance.reload();
            } catch (Exception) { }
        }

        return success;
    }

    public void freeze() {
        transform.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>().enabled = false;
        anim.SetBool("Jumping", false);
        anim.SetFloat("Speed", 0);

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    public void unfreeze() {
        transform.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>().enabled = true;
    }

    [PunRPC]
    public void getDamage(int amount, int attacker) {
        isInCombat = true;
        health -= amount;
        anim.Play("GettingHit");

        if (target == null) {
            PhotonView.Find(attacker).gameObject.GetComponent<NPC>().setSelected();
        }
    }

    // Has killed someone/NPC or made an assistance
    [PunRPC]
    public void addKill(int id, Task.ActorType type, int level, int damage, int totalHealth, int expValue, int templateId = 0)
    {
        int wonExp = 0;
        int levelDiff = level - level;

        // do not give exp if player has > 3 levels than the killed npc
        if (levelDiff > -2)
        {
            if (damage >= totalHealth) {
                wonExp = expValue;
            } else {
                wonExp = expValue / 2;
            }
            exp += wonExp + (levelDiff * 10);
        }

        QuestManager.Instance.sendAction(id, type, Task.ActionType.Kill, 1, templateId);
    }

}
