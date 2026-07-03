using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSessionResetter : MonoBehaviour
{
    [SerializeField]
    private ARSession arSession;

    private IEnumerator Start()
    {
        if (arSession == null)
        {
            arSession = GetComponent<ARSession>();
        }

        yield return null;

        if (arSession != null)
        {
            arSession.Reset();
        }
    }
}