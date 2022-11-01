using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    [SerializeField] private Camera cam;

    public void StartGameCamera(System.Action<object> onComplete)
    {
        LeanTween.value(gameObject, updateValueExampleCallback, 7f, 12.8f, 2.5f).setEase(LeanTweenType.easeInOutCubic).setOnComplete(onComplete);
        void updateValueExampleCallback(float val, float ratio)
        {
            cam.orthographicSize = val;
        }
    }
}
