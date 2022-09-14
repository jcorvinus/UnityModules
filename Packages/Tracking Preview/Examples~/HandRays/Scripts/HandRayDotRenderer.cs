using Leap.Unity.Preview.HandRays;
using UnityEngine;

public class HandRayDotRenderer : HandRayRenderer
{
    public float rayDistance = 10f;

    protected override bool UpdateLineRendererLogic(HandRayDirection handRayDirection, out RaycastHit raycastResult)
    {
        Vector3 lineRendererEndPos;
        raycastResult = new RaycastHit();
        bool hit = false;
        if (Physics.Raycast(new Ray(handRayDirection.RayOrigin, handRayDirection.Direction), out raycastResult, rayDistance, _layerMask))
        {
            hit = true;
            lineRendererEndPos = raycastResult.point + handRayDirection.Direction * -(_lineRenderer.widthMultiplier /2);
        }
        else
        {
            lineRendererEndPos = handRayDirection.RayOrigin + handRayDirection.Direction * rayDistance;
        }

        Vector3 lineRendererStartPos = lineRendererEndPos + handRayDirection.Direction * -_lineRenderer.widthMultiplier;
        UpdateLineRendererPositions(2, new Vector3[] { lineRendererStartPos, lineRendererEndPos });
        return hit;
    }
}
