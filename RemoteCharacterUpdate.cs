using Newtonsoft.Json.Linq;
using UnityEngine;
using Assets.Script;
using UnityEngine.TextCore.Text;

public class RemoteCharacterUpdate :MonoBehaviour
{
    private ClientManager clientManager;

    private void Start()
    {
        clientManager = ClientManager.Instance;
        clientManager.Dispatcher.RegisterHandler("CharactermoveUpdate", HandleCharacterMove);
        clientManager.Dispatcher.RegisterHandler("CharacterJump", HandleJump);
        clientManager.Dispatcher.RegisterHandler("Player_attack", HandleAttack);
       
    }
    private void HandleCharacterMove(JObject data)
    {
        Debug.Log($"[CharactermoveUpdate] {data}");

        if (!data.ContainsKey("characterid") || !data.ContainsKey("dir") ||
            !data.ContainsKey("movespeed") || !data.ContainsKey("isMoving") )
          
        {
            Debug.LogWarning("Missing required fields in CharactermoveUpdate data.");
            return;
        }

        int characterId = data["characterid"].ToObject<int>();
        if (characterId == -1)
        {
            Debug.LogWarning("Invalid characterid in movement update.");
            return;
        }

        var activePlayers = GameManager.instance.ActivePlayers;
        if (!activePlayers.TryGetValue(characterId, out GameObject playerObj) || playerObj == null)
        {
            Debug.LogWarning($"Player with id {characterId} not found in ActivePlayers.");
            return;
        }

        int dir = data["dir"].ToObject<int>();
        float speed = data["movespeed"].ToObject<float>();
        bool isMoving = data["isMoving"].ToObject<bool>();
    
        var remoteMove = playerObj.GetComponent<RemoteController>();
        if (remoteMove != null)
        {
            remoteMove.SetMovementState(dir, isMoving, speed);
        }
        else
        {
            Debug.LogWarning($"RemoteController missing on player {characterId}");
        }
    }
    public void HandleJump(JObject data)
    {
        int characterId = data["characterid"].ToObject<int>();
        if (characterId == -1)
        {
            Debug.LogWarning("Invalid characterid in movement update.");
            return;
        }
        var force = data["force"]?.ToObject<float>() ?? 0f;
        var activePlayers = GameManager.instance.ActivePlayers;
        if (!activePlayers.TryGetValue(characterId, out GameObject playerObj) || playerObj == null)
        {
            Debug.LogWarning($"Player with id {characterId} not found in ActivePlayers.");
            return;
        }
        var remoteMove = playerObj.GetComponent<RemoteController>();
        if (remoteMove != null)
        {
            remoteMove.SetJump(force);
        }
        else
        {
            Debug.LogWarning($"RemoteController missing on player {characterId}");
        }

    }
    public void HandleAttack(JObject data)
    {
        var skillid = data["Skill"]?.ToObject<int>() ?? 0;
        if (skillid == 0)
        {
            Debug.Log("not skill data");
            return;
            
        }
        var character = data["characterID"]?.ToObject<int>() ?? 0;
        if (character == 0)
        {
            Debug.Log("Character is null data");
            return;

        }
        var target = data["Target"]?.ToObject<int>() ?? 0;
        var facing = data["facingRight"].ToObject<bool>();
       
        var activePlayers = GameManager.instance.ActivePlayers;
        if (!activePlayers.TryGetValue(character, out GameObject playerObj) || playerObj == null)
        {
            Debug.LogWarning($"Player with id {character} not found in ActivePlayers.");
            return;
        }
        var remoteMove = playerObj.GetComponent<RemoteController>();
        var remoteTransform = playerObj.transform;
     
        if (remoteMove != null)
        {
            remoteMove.ChangeState("Attack");
        }
        else
        {
            Debug.LogWarning($"RemoteController missing on player {character}");
        }

        var monsters = MonsterManager.instance.monsters;
        if (!monsters.TryGetValue(target, out GameObject monsterGO) || monsterGO == null)
        {
            Debug.LogWarning($"Monster with id {target} not found in Monster.");
            return;
        }
        var monsterTranform = monsterGO.gameObject.transform;
        if (monsterTranform != null)
        {
            SkillManager.Instance.PlaySkill(skillid, remoteTransform.localPosition,facing, monsterTranform, false);
            var state = monsterGO.GetComponent<MonsterVisual>();
            state.PlayHit();
        }

    }
    public void HandleHit(JObject data)
    {

    }

    private void OnDestroy()
    {
        //if (clientManager != null)
        //{
        //   // clientManager.Dispatcher.UnregisterHandler("CharactermoveUpdate", HandleCharacterMove);
        //}
    }
}