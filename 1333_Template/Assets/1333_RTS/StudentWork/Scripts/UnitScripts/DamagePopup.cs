using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private float riseRate = 1.2f;
    [SerializeField] private float lifeSpan = 1.0f;
    [SerializeField] private TextMeshProUGUI uiText;

    private float timeAlive = 0f;

    public void Initialize(int amount)
    {
        if (uiText != null)
            uiText.text = amount.ToString();

        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        Destroy(gameObject, lifeSpan);
    }

    private void Update()
    {
        transform.position += Vector3.up * riseRate * Time.deltaTime;

        if (Camera.main != null)
        {
            Vector3 forward = Camera.main.transform.forward;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}