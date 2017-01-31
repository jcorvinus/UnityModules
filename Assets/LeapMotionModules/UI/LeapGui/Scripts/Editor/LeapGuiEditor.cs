﻿using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Leap.Unity;
using Leap.Unity.Query;

[CustomEditor(typeof(LeapGui))]
public class LeapGuiEditor : CustomEditorBase {
  private const int BUTTON_WIDTH = 23;

  private LeapGui gui;
  private ReorderableList _featureList;
  private GenericMenu _addFeatureMenu;

  private GenericMenu _addRendererMenu;
  private Editor _rendererEditor;

  protected override void OnEnable() {
    base.OnEnable();

    gui = target as LeapGui;

    _featureList = new ReorderableList(gui.features,
                                       typeof(LeapGuiFeatureBase),
                                       draggable: true,
                                       displayHeader: false,
                                       displayAddButton: false,
                                       displayRemoveButton: false);

    _featureList.showDefaultBackground = false;
    _featureList.headerHeight = 0;
    _featureList.elementHeight = EditorGUIUtility.singleLineHeight;
    _featureList.elementHeightCallback = featureHeightCallback;
    _featureList.drawElementCallback = drawFeatureCallback;
    _featureList.onReorderCallback = onReorderFeaturesCallback;

    var allTypes = Assembly.GetAssembly(typeof(LeapGui)).GetTypes();

    var allFeatures = allTypes.Query().
                               Where(t => !t.IsAbstract).
                               Where(t => !t.IsGenericType).
                               Where(t => t.IsSubclassOf(typeof(LeapGuiFeatureBase)));

    _addFeatureMenu = new GenericMenu();
    foreach (var feature in allFeatures) {
      _addFeatureMenu.AddItem(new GUIContent(LeapGuiFeatureNameAttribute.GetFeatureName(feature)),
                              false,
                              () => gui.features.Add(ScriptableObject.CreateInstance(feature) as LeapGuiFeatureBase));
    }

    var allRenderers = allTypes.Query().
                                Where(t => !t.IsAbstract).
                                Where(t => !t.IsGenericType).
                                Where(t => t.IsSubclassOf(typeof(LeapGuiRenderer)));

    _addRendererMenu = new GenericMenu();
    foreach (var renderer in allRenderers) {
      _addRendererMenu.AddItem(new GUIContent(renderer.Name),
                               false,
                               () => {
                                 gui.SetRenderer(ScriptableObject.CreateInstance(renderer) as LeapGuiRenderer);
                                 _rendererEditor = Editor.CreateEditor(gui.renderer);
                                 serializedObject.Update();
                               });
    }

    if (gui.renderer != null) {
      _rendererEditor = Editor.CreateEditor(gui.renderer);
    }

    specifyCustomDrawer("features", drawFeatures);
    specifyCustomDrawer("renderer", drawRenderer);
  }

  private void drawFeatures(SerializedProperty property) {
    Rect rect = EditorGUILayout.GetControlRect(GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
    Rect left, middle, right;
    rect.SplitHorizontallyWithRight(out middle, out right, BUTTON_WIDTH);
    middle.SplitHorizontallyWithRight(out left, out middle, BUTTON_WIDTH);

    EditorGUI.LabelField(left, "Element Features", EditorStyles.miniButtonLeft);

    EditorGUI.BeginDisabledGroup(gui.features.Count == 0);
    if (GUI.Button(middle, "-", EditorStyles.miniButtonMid)) {
      gui.features.RemoveAt(_featureList.index);
      EditorUtility.SetDirty(gui);
    }
    EditorGUI.EndDisabledGroup();

    if (GUI.Button(right, "+", EditorStyles.miniButtonRight)) {
      _addFeatureMenu.ShowAsContext();
      EditorUtility.SetDirty(gui);
    }

    EditorGUI.BeginChangeCheck();

    Undo.RecordObject(target, "Changed feature list.");
    _featureList.DoLayoutList();

    if (EditorGUI.EndChangeCheck()) {
      EditorUtility.SetDirty(gui);
    }
  }

  private void drawRenderer(SerializedProperty property) {
    Rect rect = EditorGUILayout.GetControlRect(GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
    Rect left, right;
    rect.SplitHorizontallyWithRight(out left, out right, BUTTON_WIDTH);


    EditorGUI.LabelField(left, "Renderer", EditorStyles.miniButtonLeft);
    if (GUI.Button(right, "+", EditorStyles.miniButtonRight)) {
      _addRendererMenu.ShowAsContext();
    }

    if (_rendererEditor != null) {
      _rendererEditor.DrawDefaultInspector();
    }
  }


  // Feature list callbacks

  private void drawFeatureHeaderCallback(Rect rect) {
    Rect left, right;
    rect.SplitHorizontallyWithRight(out left, out right, BUTTON_WIDTH);

    EditorGUI.LabelField(left, "Gui Element Features", EditorStyles.miniButtonLeft);

    if (GUI.Button(right, "+", EditorStyles.miniButtonRight)) {
      _addFeatureMenu.ShowAsContext();
    }
  }

  private float featureHeightCallback(int index) {
    return gui.features[index].GetEditorHeight() + EditorGUIUtility.singleLineHeight;
  }

  private void drawFeatureCallback(Rect rect, int index, bool isActive, bool isFocused) {
    var feature = gui.features[index];

    string featureName = LeapGuiFeatureNameAttribute.GetFeatureName(gui.features[index].GetType());
    GUIContent featureLabel = new GUIContent(featureName);

    var supportInfo = gui.supportInfo[index];
    switch (supportInfo.support) {
      case SupportType.Warning:
        GUI.color = Color.yellow;
        featureLabel.tooltip = supportInfo.message;
        break;
      case SupportType.Error:
        GUI.color = Color.red;
        featureLabel.tooltip = supportInfo.message;
        break;
    }

    Vector2 size = EditorStyles.label.CalcSize(featureLabel);

    Rect labelRect = rect;
    labelRect.width = size.x;

    GUI.Box(labelRect, "");
    EditorGUI.LabelField(labelRect, featureLabel);

    GUI.color = Color.white;

    Undo.RecordObject(feature, "Modified Gui Feature");

    EditorGUI.BeginChangeCheck();
    feature.DrawFeatureEditor(rect.NextLine().Indent(), isActive, isFocused);
    if (EditorGUI.EndChangeCheck()) {
      EditorUtility.SetDirty(feature);
    }
  }

  private void onReorderFeaturesCallback(ReorderableList list) {
    EditorUtility.SetDirty(target);
    serializedObject.Update();
  }
}

