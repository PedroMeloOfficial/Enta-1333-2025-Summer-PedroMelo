using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;

[RequireComponent(typeof(Collider))]
public class HPStat : MonoBehaviour, IDamageReceiver
{
    [SerializeField] private int maximum = 100;
    [SerializeField] private GameObject numberPrefab;
    [SerializeField] private HPBar barPrefab;
    [SerializeField] private Vector3 offsetPosition = new Vector3(0f, 2f, 0f);

    public int Current => _current;
    public int MaxHP => maximum;
    public UnityEvent OnZero;

    private int _current;
    private HPBar barRef;

    private void Start()
    {
        _current = maximum;

        if (barPrefab != null)
        {
            GameObject barObj = Instantiate(barPrefab.gameObject, transform.position + offsetPosition, quaternion.identity);
            barObj.transform.SetParent(transform);
            barRef = barObj.GetComponent<HPBar>();
            barRef.Initialize(this);
        }
    }

    public void TakeDamage(int amount, GameObject from)
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.unitHitClip);
        
        int temp = _current - amount;
        if (temp < 0)
            _current = 0;
        else
            _current = temp;

        if (numberPrefab != null)
        {
            Vector3 displayPosition = transform.position + Vector3.up * 1.5f;
            GameObject obj = Instantiate(numberPrefab, displayPosition, Quaternion.identity);
            DamagePopup text = obj.GetComponent<DamagePopup>();
            if (text != null) text.Initialize(amount);
        }

        if (barRef != null)
            barRef.RefreshBar();

        if (_current <= 0)
        {
            if (OnZero != null)
            {
                OnZero.Invoke();
            }

            Destroy(gameObject);
        }
    }
}