using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairResponse : MonoBehaviour
{
    [SerializeField] float minCrosshairOffset;
    [SerializeField] float maxCrosshairOffset;
    [SerializeField] float crosshairReturnSpeed;
    float crosshairOffset;

    [SerializeField] RectTransform upCrosshair;
    [SerializeField] RectTransform downCrosshair;
    [SerializeField] RectTransform leftCrosshair;
    [SerializeField] RectTransform rightCrosshair;

    private void Update()
    {
        if(crosshairOffset > minCrosshairOffset)
        {
            crosshairOffset = Mathf.Max(crosshairOffset - crosshairReturnSpeed*Time.deltaTime, minCrosshairOffset);
        }

        // Move the crosshair elements
        upCrosshair.anchoredPosition = new Vector2(0, crosshairOffset);
        rightCrosshair.anchoredPosition = new Vector2(crosshairOffset, 0);
        downCrosshair.anchoredPosition = new Vector2(0, -crosshairOffset);
        leftCrosshair.anchoredPosition = new Vector2(-crosshairOffset, 0);
    }
    public void SetCrosshairResponse()
    {
        crosshairOffset = maxCrosshairOffset;
    }
}
