using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Slider bar;
    private HPStat target;
    private Transform camRef;

    public void Initialize(HPStat stat)
    {
        target = stat;
        camRef = Camera.main != null ? Camera.main.transform : null;

        if (bar != null)
        {
            bar.minValue = 0;
            bar.maxValue = target.MaxHP;
            bar.value = target.Current;
        }
    }

    private void LateUpdate()
    {
        if (camRef != null)
        {
            Vector3 dir = transform.position + camRef.forward;
            transform.LookAt(dir);
        }

        if (bar != null && target != null)
            bar.value = target.Current;
    }

    public void RefreshBar()
    {
        if (bar != null && target != null)
            bar.value = target.Current;
    }
}