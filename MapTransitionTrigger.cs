using UnityEngine;
using Assets.Script;

public class MapTransitionTrigger : MonoBehaviour
{
    public enum TransitionType { Next, Previous }
    public TransitionType transitionType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("LocalPlayer"))
        {
            return;
        }

        int currentMapId = GameManager.instance.CurrentMapId;
        int newMapId = transitionType == TransitionType.Next ? currentMapId + 1 : currentMapId - 1;

        if (newMapId < 1)
        {
            Debug.LogWarning($"[MapTransitionTrigger] Invalid newMapId: {newMapId}. Cannot transition.");
            return;
        }
        AudioManager.Instance.StopAllBGM();
        AudioManager.Instance.StopSFX("Run");

        GameManager.instance.SetNextSpawnType(transitionType == TransitionType.Next ? "Start" : "End");
        GameManager.instance.SendLeavermap();
        GameManager.instance.ChangeMap(newMapId, true);
    }
}