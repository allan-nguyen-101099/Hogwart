# Hogwarts Online Mode - 2-Client Test Checklist

## Pre-Test Setup

1. **Build the game:**
   - File → Build Settings → ensure both "MainMenu" and "Hogwarts" scenes are in build list
   - Build as Windows Standalone .exe to a folder (e.g., `Builds/Hogwarts`)

2. **Prepare two game windows:**
   - Open command prompt in build folder
   - Run: `Hogwarts.exe` (this is Client A)
   - Run another: `Hogwarts.exe` (this is Client B)
   - Resize windows side-by-side so you can see both

3. **Create characters:**
   - Client A: Create character "Alice" (any house)
   - Client B: Create character "Bob" (any house)
   - Both should now see their characters on MainPanel with "Join" button

---

## Test Execution

### Phase 1: Connection & Spawn (Expected: Both players visible to each other)

**Step 1.1: Join Game**
- Client A: Click "Join" button → watch console for: `OnJoinedLobby() called`
- Client B: Click "Join" button → same log
- ✅ **PASS** if both consoles show `OnJoinedRoom() called - about to spawn player` within 3 seconds
- ⚠️ **WARN** if only one shows OnJoinedRoom (room join failed for the other)
- ❌ **FAIL** if neither reaches OnJoinedRoom (network/room creation broken)

**Step 1.2: Verify Both Players Spawn**
- Within 5 seconds, you should see both character avatars in the Hogwarts world
- Client A should see "Bob" walking around (remote player)
- Client B should see "Alice" walking around (remote player)
- ✅ **PASS** if you see the other player's nameplate above their head
- ❌ **FAIL** if only your own character appears, or if console shows: `[NetworkManager] Player instantiated returned NULL!`

**Step 1.3: Verify Chat System**
- Client A: Press "T", type "Hello from Alice", press Enter
- Check Client B's chat window: should show `[Alice] Hello from Alice`
- ✅ **PASS** if message appears on both clients
- ❌ **FAIL** if message doesn't sync or one client doesn't receive

---

### Phase 2: Player Movement Sync (Expected: Smooth position/rotation updates)

**Step 2.1: Move on Client A**
- Client A: Use WASD to move your character 20+ units away
- Watch Client B's view: Bob's avatar should move smoothly to the new position
- ✅ **PASS** if movement syncs with <500ms delay
- ⚠️ **WARN** if movement is jerky or delayed >1 second (network latency but acceptable for local)
- ❌ **FAIL** if remote player doesn't move at all or teleports

**Step 2.2: Rotate Camera on Client B**
- Client B: Use mouse to rotate camera around Alice
- Watch Client A: Alice's nameplate should track the camera but her position shouldn't change
- ✅ **PASS** if camera/rotation syncs correctly without affecting position
- ❌ **FAIL** if rotation causes position desync or nameplate glitches

---

### Phase 3: Combat System (Expected: Spells cast, damage dealt, XP earned)

**Step 3.1: Target an NPC on Client A**
- Client A: Move near an aggressive NPC (e.g., Castle Spider)
- Mouse over it until cursor changes to sword icon
- Left-click to select it
- Verify nameplate appears above NPC with health bar
- ✅ **PASS** if NPC is selected and health bar shows
- ❌ **FAIL** if NPC selection doesn't work or health bar is missing

**Step 3.2: Cast Spell on Client A**
- Client A: Press "1" to cast Fireball at the NPC
- Watch console: should show `[PlayerCombat.spellCast] Spell: Fireball` and `SPELL CAST COMPLETE!`
- Verify casting animation plays (wand animation)
- ✅ **PASS** if spell animation and console logs appear
- ⚠️ **WARN** if animation is missing but console logs show spell was cast
- ❌ **FAIL** if console shows `NO TARGET!` or spell doesn't cast

**Step 3.3: Verify Spell Effect on Client B**
- Client B: Watch the same NPC that Alice is attacking
- You should see the Fireball projectile fly from Alice toward the NPC
- Within 2 seconds, you should see a collision/impact effect at the NPC's position
- ✅ **PASS** if spell projectile and impact are visible on both clients
- ⚠️ **WARN** if impact is delayed or jerky
- ❌ **FAIL** if Client B sees nothing or projectile doesn't sync

**Step 3.4: Verify Damage & Health Sync**
- After spell hits, NPC's health bar should decrease on both clients
- Client A should see damage number (e.g., "-45") float above NPC
- Client B should also see the same health reduction and damage number
- ✅ **PASS** if health reduction matches on both clients
- ❌ **FAIL** if health bars diverge (de-sync) between clients

**Step 3.5: Kill the NPC**
- Client A: Keep casting spells (press 1, 2, or 3) until NPC dies
- Watch NPC play death animation and collapse
- On Client A console: should see `[Player.addKill]` message
- ✅ **PASS** if NPC dies on both clients and kill is registered on Client A
- ❌ **FAIL** if NPC dies on one client but not the other, or kill message doesn't appear

**Step 3.6: Verify XP Gain**
- After NPC death, Client A should see XP notification (floating text like "+120 XP")
- Check Player.Level or XP bar in PlayerPanel should increase
- ✅ **PASS** if Client A gains XP and sees the notification
- ⚠️ **WARN** if XP gain works but lags or isn't visible
- ❌ **FAIL** if Client A gains no XP or notification is missing

**Step 3.7: NPC Respawn**
- After ~10 seconds, the killed NPC should respawn at its original location
- ✅ **PASS** if NPC respawns visibly on both clients
- ❌ **FAIL** if NPC doesn't respawn or respawns only on one client

---

### Phase 4: NPC AI & Combat (Expected: NPCs attack both players, damage is consistent)

**Step 4.1: Both Players Target Same NPC**
- Client A & B: Both move near a different aggressive NPC
- Client A: Click to select it
- Client B: Click to select the same NPC
- Both should show the NPC's nameplate and health bar
- ✅ **PASS** if both players can target the same NPC
- ❌ **FAIL** if only one can select it or selection breaks

**Step 4.2: NPC Attacks Back**
- Client A: Don't cast spells, just stand near NPC for 10 seconds
- NPC should aggro (turn red) and move toward Client A
- Client A console: should see `getDamage` RPC calls
- Client A's health bar should decrease
- ✅ **PASS** if NPC attacks and deals damage
- ⚠️ **WARN** if NPC moves very slowly or attack delay is >2 seconds
- ❌ **FAIL** if NPC ignores Client A or doesn't deal damage

**Step 4.3: Verify Damage Received on Both Clients**
- While NPC is attacking Client A, check Client B's view
- Client B should see Client A's health bar decrease (if visible in UI)
- ✅ **PASS** if health sync is visible
- ⚠️ **WARN** if health update is delayed
- ❌ **FAIL** if Client B doesn't see health changes

---

### Phase 5: Network Stability (Expected: No disconnects or desync for 5 minutes)

**Step 5.1: Idle Test**
- Both clients: Let characters stand still for 2 minutes
- Watch for any console errors or position jumps
- ✅ **PASS** if no unexpected disconnects or errors
- ❌ **FAIL** if either client disconnects or gets `OnDisconnectedFromPhoton` message

**Step 5.2: Rapid Movement & Spell Casting**
- Client A: Move rapidly (WASD spam) + cast spells repeatedly (press 1,2,3 rapidly) for 30 seconds
- Client B: Watch Alice for position jerks or jitter
- Check console for RPC errors or photon-related warnings
- ✅ **PASS** if movement remains smooth despite heavy action
- ⚠️ **WARN** if movement is choppy but playable
- ❌ **FAIL** if Client B can't keep up or sees position desync

**Step 5.3: Disconnect & Reconnect**
- Client A: Disconnect from the game (close window or Alt+F4)
- Client B: Watch for chat message or UI change indicating Alice left
- Client B: Should see something like `[Sistema] Alice left the game` in chat (if implemented)
- Restart Client A and rejoin
- ✅ **PASS** if Client A can rejoin without errors
- ⚠️ **WARN** if rejoin is slow (>10 seconds) but succeeds
- ❌ **FAIL** if Client A can't rejoin or gets stuck on loading screen

---

## Summary & Scoring

**All tests passed (✅):** Online mode is ready for beta testing  
**1-2 warnings (⚠️):** Online mode works but needs performance/UX polish  
**3+ failures (❌):** Critical blocker remains; review NetworkManager and combat sync logic

### Quick Log Search for Debugging

If tests fail, search console for these keywords:

| Error | Meaning |
|-------|---------|
| `OnPhotonJoinRandomFailed` | Room join failed; fallback room creation should happen |
| `PhotonNetwork.Instantiate returned NULL` | Player prefab not found in Resources/Characters/Player |
| `getHit` missing RPC | NPC damage not syncing to player |
| `OnJoinedRoom() called` | Room join succeeded; player spawn should begin |
| `Destroy(gameObject)` in Player.Start | Remote player being destroyed correctly |
| `getDamage RPC` | Player receiving damage from NPC |

---

## Next Steps (If All Passing)

1. Test with 3+ clients simultaneously
2. Test with variable network lag (use packet loss tool)
3. Verify quest progress syncs
4. Test inventory/item pickup between clients
5. Load-test 10+ players in one room (check master client authority holds up)
