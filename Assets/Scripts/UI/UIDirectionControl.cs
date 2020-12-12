using UnityEngine;

namespace Complete
{
    public class UIDirectionControl : MonoBehaviour
    {
        private void Update ()
        {
            transform.rotation = transform.parent.localRotation;
        }
    }
}