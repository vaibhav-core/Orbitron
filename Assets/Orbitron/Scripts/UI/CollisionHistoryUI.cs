using TMPro;
using UnityEngine;

public class CollisionHistoryUI : MonoBehaviour
{
    public static CollisionHistoryUI Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private GameObject collisionItemPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddCollision(double simTime, string body1, string body2, string result)
    {
        GameObject item = Instantiate(collisionItemPrefab, content);

        CollisionItemUI ui = item.GetComponent<CollisionItemUI>();

        ui.Setup(simTime, body1, body2, result);
    }
}