using UnityEngine;
using UnityEngine.Events;


public class FlickEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent OnEvent;
    [SerializeField] private UnityEvent OffEvent;
    
    private bool flag = false;
    
    
    public void Invoke()
    {
        flag = !flag;
        if (flag)
        {
            OnEvent.Invoke();
        }
        else
        {
            OffEvent.Invoke();
        }
    }
}
