using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
[CanEditMultipleObjects]
public class InspectorGameManager : Editor
{
    int prevMockLevelNum;
    bool autoUpdateLevel;

    private void OnEnable()
    {
        prevMockLevelNum = (target as GameManager).mockLevelNum;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameManager manager = target as GameManager;

        if (GUILayout.Button("Create Level"))
        {
            manager.CreateLevel(manager.mockLevelNum);
        }

        autoUpdateLevel = GUILayout.Toggle(autoUpdateLevel, "Auto Update Level");
        if (autoUpdateLevel && manager.mockLevelNum != prevMockLevelNum)
        {
            manager.CreateLevel(manager.mockLevelNum);
            prevMockLevelNum = manager.mockLevelNum;
        }
    }
}
#endif
