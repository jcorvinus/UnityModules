using Leap.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandPoseValidator : MonoBehaviour
{
    /// <summary>
    /// Which hand would you like to use for gesture validation?
    /// If this is left blank, It will search for all hands in the scene
    /// </summary>
    [SerializeField]
    private List<CapsuleHand> validationHands = new List<CapsuleHand>();

    private List<CapsuleHand> storedValidationHands = new List<CapsuleHand>();
    private int validationHandsActive = 0;
    private int validationHandsActivePrevFrame = 0;

    [SerializeField]
    private LeapProvider leapProvider = null;

    [SerializeField]
    private GameObject capsuleHands;

    private Color[] _leftCapsuleHandColours = new Color[32];
    private Color[] _rightCapsuleHandColours = new Color[32];
    private HandPoseDetector _detector;

    private void Start()
    {
        _detector = FindObjectOfType<HandPoseDetector>();
        _detector.EnablePoseCaching();
        if(validationHands.Count == 0 )
        {
            var instCapsuleHands = Instantiate(capsuleHands);

            var capsuleHandScript = instCapsuleHands.GetComponentsInChildren<CapsuleHand>(true);
            foreach (var script in capsuleHandScript)
            {
                script.leapProvider = leapProvider;
                storedValidationHands.Add(script);
                validationHandsActive++;
            }
        }
        else
        {
            storedValidationHands = validationHands;
        }
        
    }
    List<GameObject> lineRenderers = new List<GameObject>();

    private void Update()
    {
        foreach (var hand in storedValidationHands)
        {
            if(!hand.isActiveAndEnabled)
            {
                validationHandsActive--;
            }
        }
        if(validationHandsActive != validationHandsActivePrevFrame)
        {
            validationHandsActivePrevFrame = validationHandsActive;
            foreach (var lineRenderer in lineRenderers)
            {
                DestroyImmediate(lineRenderer);
            }
            lineRenderers.Clear();
        }

        if (_detector != null) 
        {
            ColorHandJoints();
            RenderDirectionRays();
        }
    }

    private void ColorHandJoints()
    {
        var colourCapsuleHand = storedValidationHands.FirstOrDefault();
        if (colourCapsuleHand != null)
        {
            Utils.Fill(_leftCapsuleHandColours, Color.grey);
            Utils.Fill(_rightCapsuleHandColours, Color.grey);
        }

        if (storedValidationHands.Count > 0)
        {
            var validationData = _detector.GetValidationData();

            foreach (var visHand in storedValidationHands)
            {
                foreach (var data in validationData)
                {
                    if (data.withinThreshold)
                    {
                        if (visHand.Handedness == Chirality.Left && data.chirality == Chirality.Left)
                        {
                            _leftCapsuleHandColours[(data.fingerNum * 4) + data.jointNum] = Color.green;
                        }
                        else if (visHand.Handedness == Chirality.Right && data.chirality == Chirality.Right)
                        {
                            _rightCapsuleHandColours[(data.fingerNum * 4) + data.jointNum] = Color.green;
                        }
                    }
                    else
                    {
                        if (visHand.Handedness == Chirality.Left && data.chirality == Chirality.Left)
                        {
                            _leftCapsuleHandColours[(data.fingerNum * 4) + data.jointNum] = Color.red;
                        }
                        else if (visHand.Handedness == Chirality.Right && data.chirality == Chirality.Right)
                        {
                            _rightCapsuleHandColours[(data.fingerNum * 4) + data.jointNum] = Color.red;
                        }
                    }
                }
                if (visHand != null)
                {
                    visHand.SetIndividualSphereColors = true;
                    if (visHand.Handedness == Chirality.Left)
                    {
                        visHand.SphereColors = _leftCapsuleHandColours;
                    }
                    if (visHand.Handedness == Chirality.Right)
                    {
                        visHand.SphereColors = _rightCapsuleHandColours;
                    }


                }
            }
        }
        else
        {
            return;
        }
    }

    private void RenderDirectionRays()
    {
        var lineRenderCount = 0;
        for (int j = 0; j < storedValidationHands.Count; j++)
        {
            if (storedValidationHands.ElementAt(j).enabled)
            {
                for (int i = 0; i < _detector.Sources.Count; i++)
                {
                    var boneDirectionTarget = _detector.Sources.ElementAt(i);

                    if (boneDirectionTarget.enabled)
                    {
                        bool AtleastOneDirectionActive = false;
                        foreach (var direction in boneDirectionTarget.direction)
                        {
                            if (direction.enabled)
                            {
                                AtleastOneDirectionActive = true;
                            }
                        }

                        if (AtleastOneDirectionActive)
                        {
                            var capsuleHand = storedValidationHands.ElementAt(j);

                            if (lineRenderers.ElementAtOrDefault(lineRenderCount) == null)
                            {
                                var lineRendChild = new GameObject();
                                lineRendChild.transform.SetParent(capsuleHand.gameObject.transform);
                                lineRendChild.AddComponent<LineRenderer>();
                                lineRenderers.Add(lineRendChild);
                            }

                            var lineRend = lineRenderers.ElementAt(lineRenderCount).GetComponent<LineRenderer>();
                            if (lineRend)
                            {
                                lineRend.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                                lineRend.startWidth = 0.005f;
                                lineRend.endWidth = 0.005f;
                                lineRend.material.color = new Color(0, 235, 133, 0.7f);

                                if (capsuleHand != null && capsuleHand.enabled)
                                {
                                    if ((int)boneDirectionTarget.finger != 5 &&
                                        boneDirectionTarget.finger != (int)Leap.Finger.FingerType.TYPE_UNKNOWN &&
                                        boneDirectionTarget.bone != (int)Leap.Bone.BoneType.TYPE_INVALID)
                                    {
                                        int fingNum = (int)boneDirectionTarget.finger;
                                        int boneNum = (int)boneDirectionTarget.bone;
                                        if (capsuleHand.GetLeapHand() != null)
                                        {
                                            var directionBone = capsuleHand.GetLeapHand().Fingers[fingNum].bones[boneNum];
                                            if (directionBone.PrevJoint != null)
                                            {
                                                Ray ray = new Ray(directionBone.PrevJoint, directionBone.Direction);

                                                lineRend.SetPosition(0, directionBone.PrevJoint);
                                                lineRend.SetPosition(1, ray.GetPoint(10));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (capsuleHand.GetLeapHand() != null &&
                                            capsuleHand.GetLeapHand().PalmPosition != null &&
                                            capsuleHand.GetLeapHand().PalmNormal != null)
                                        {
                                            Ray ray = new Ray(capsuleHand.GetLeapHand().PalmPosition, capsuleHand.GetLeapHand().PalmNormal);
                                            lineRend.SetPosition(0, capsuleHand.GetLeapHand().PalmPosition);
                                            lineRend.SetPosition(1, ray.GetPoint(100));
                                        }
                                    }
                                }
                            }
                            lineRenderCount++;
                        }
                    }
                }
            }
        }
    }
}
