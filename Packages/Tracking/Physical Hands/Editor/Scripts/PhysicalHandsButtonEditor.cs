using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Leap.Unity.PhysicalHands
{
    [CustomEditor(typeof(PhysicalHandsButtonBase), true)]
    [CanEditMultipleObjects]
    public class PhysicalHandsButtonEditor : CustomEditorBase<PhysicalHandsButtonBase>
    {
        private bool eventsFoldedOut = false;

        public override void OnInspectorGUI()
        {
            EditorUtils.DrawScriptField((MonoBehaviour)target);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pressableObject"), new GUIContent("Pressable Object", "The GameObject representing the button that can be pressed."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_buttonPreset"), new GUIContent("Button Type", "The preset type of button. each one changes the buttons responsiveness and how it reacts to being pressed or springing back. \n \n Test out the presets to find the right one for you"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_automaticTravelDistance"), new GUIContent("Use Automatic Travel Distance", "Travel distance should be calculated based on how far the pressable object is from this object"));
            
            EditorGUI.indentLevel = 1;
            if (!serializedObject.FindProperty("_automaticTravelDistance").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_buttonTravelDistance"), new GUIContent("Button Travel Distance", "The distance the button can travel when pressed."));
            }
            EditorGUI.indentLevel = 0;
  
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_canBePressedByObjects"), new GUIContent("Can Be Pressed By Objects", "Determines whether the button can be pressed by objects which are not the hand."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_whichHandCanPressButton"), new GUIContent("Which Hand Can Activate Button Presses", "Specifies which hand(s) can press the button."));
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_freezeButtonTravelOnMovement"), new GUIContent("Freeze Button Travel If Moving", "Freeze button travel when base object is moving."));
            if (serializedObject.FindProperty("_freezeButtonTravelOnMovement").boolValue)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_buttonVelocityThreshold"), new GUIContent("Button Movement Velocity Allowance", "How fast can the button move before we consider it moving for the sake of freezing button travel on movement"));
                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.Space(5);
            // Events
            eventsFoldedOut = EditorGUILayout.BeginFoldoutHeaderGroup(eventsFoldedOut, "Button Events");

            if (eventsFoldedOut)
            {
                EditorGUILayout.LabelField("Button Press", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnButtonPressed"), new GUIContent("Button Pressed Event", "Event triggered when the button is pressed."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnButtonUnPressed"), new GUIContent("Button UnPressed Event", "Event triggered when the slider button is un-pressed."));
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Button Contact", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHandContact"), new GUIContent("Hand Contact Event", "Event triggered when a physical hand contacts the button."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHandContactExit"), new GUIContent("Hand Leave Contact Event", "Event triggered when a physical hand stops contacting the button."));
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Button Hover", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHandHover"), new GUIContent("Hand Hover Event", "Event triggered when a physical hand hovers the button."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHandHoverExit"), new GUIContent("Hand Leave Hover Event", "Event triggered when a physical hand stops hoverring the button."));
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            target.UpdateInspectorValues();

            serializedObject.ApplyModifiedProperties();

        }
    }

    [CustomEditor(typeof(PhysicalHandsButtonToggle), true)]
    public class PhysicalHandsButtonToggleEditor : PhysicalHandsButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}
