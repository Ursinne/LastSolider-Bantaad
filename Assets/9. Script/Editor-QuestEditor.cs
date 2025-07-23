//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using UnityEditorInternal;

//// �ndra till QuestManager ist�llet f�r AutoQuestManager
//[CustomEditor(typeof(QuestManager))]
//public class QuestSequenceEditor : Editor
//{
//    private ReorderableList questSequenceList;
//    private SerializedProperty questSequence;
//    private SerializedProperty maxActiveQuests;
//    private SerializedProperty questUI;

//    private void OnEnable()
//    {
//        // H�mta properties - anpassa efter QuestManager
//        questSequence = serializedObject.FindProperty("availableQuests");
//        maxActiveQuests = serializedObject.FindProperty("maxActiveQuests");
//        questUI = serializedObject.FindProperty("questUI");

//        // Skapa ReorderableList f�r quest-sekvensen
//        questSequenceList = new ReorderableList(serializedObject, questSequence, true, true, true, true);

//        // Anpassa header
//        questSequenceList.drawHeaderCallback = (Rect rect) => {
//            EditorGUI.LabelField(rect, "Tillg�ngliga Quests");
//        };

//        // Anpassa element-ritning
//        questSequenceList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
//            var element = questSequence.GetArrayElementAtIndex(index);
//            rect.y += 2;
//            rect.height = EditorGUIUtility.singleLineHeight;

//            // H�mta quest f�r att visa namn
//            Quest quest = (Quest)element.objectReferenceValue;
//            string label = quest != null ? quest.questName : "Tomt quest";

//            // Visa element
//            EditorGUI.PropertyField(rect, element, new GUIContent($"Quest {index + 1}: {label}"));
//        };

//        // L�gg till callbacks f�r att hantera listan
//        questSequenceList.onAddCallback = (ReorderableList list) => {
//            list.serializedProperty.arraySize++;
//            list.index = list.serializedProperty.arraySize - 1;
//        };

//        questSequenceList.onRemoveCallback = (ReorderableList list) => {
//            ReorderableList.defaultBehaviours.DoRemoveButton(list);
//        };
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Quest Manager", EditorStyles.boldLabel);
//        EditorGUILayout.HelpBox("Hantera tillg�ngliga quests f�r spelaren.", MessageType.Info);

//        EditorGUILayout.Space(5);
//        EditorGUILayout.PropertyField(maxActiveQuests, new GUIContent("Max aktiva quests", "Maximalt antal quests som kan vara aktiva samtidigt."));
//        EditorGUILayout.PropertyField(questUI, new GUIContent("Quest UI", "UI-komponent f�r att visa quests."));

//        EditorGUILayout.Space(10);
//        questSequenceList.DoLayoutList();

//        if (questSequence.arraySize > 0)
//        {
//            EditorGUILayout.Space(5);

//            if (GUILayout.Button("�ppna vald quest i Inspector", GUILayout.Height(30)))
//            {
//                if (questSequenceList.index >= 0 && questSequenceList.index < questSequence.arraySize)
//                {
//                    var selectedElement = questSequence.GetArrayElementAtIndex(questSequenceList.index);
//                    if (selectedElement.objectReferenceValue != null)
//                    {
//                        Selection.activeObject = selectedElement.objectReferenceValue;
//                    }
//                }
//            }
//        }

//        EditorGUILayout.Space(10);

//        // Visa varning f�r tomma quests
//        for (int i = 0; i < questSequence.arraySize; i++)
//        {
//            var element = questSequence.GetArrayElementAtIndex(i);
//            if (element.objectReferenceValue == null)
//            {
//                EditorGUILayout.HelpBox($"Quest {i + 1} �r tom!Quest-listan kan inte ha tomma quests.", MessageType.Error);
//            }
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
//#endif