using TMPro;
using UnityEngine;

public class CollisionItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text collisionText;

    public void Setup(double simTime, string body1, string body2, string result)
    {
        collisionText.text =
            $"{simTime:F2} yr | {body1} + {body2} → {result}";
    }
}