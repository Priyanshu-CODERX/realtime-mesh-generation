using System;
using System.Collections.Generic;
using Codice.CM.Common.Replication;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDKExamples.RemoteAuthoring;
using Unity.Collections;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Niantic.ARDKExamples.RemoteAuthoring.Editor
{
    [CustomEditor(typeof(LocationManifestManager))]
    public class LocationManifestManagerEditor : UnityEditor.Editor
    {
        private static bool subscribedToPlaymode = false;
        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            if (!subscribedToPlaymode)
            {
                EditModeOnlyBehaviour.RemoteAuthoringAssistant.ActiveManifestUpdated += ActiveManifestChanged;
                subscribedToPlaymode = true;
            }
        }

        public static void SyncOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isLoaded)
                {
                    var rootGameObjects = scene.GetRootGameObjects();
                    foreach (var rootGameObject in rootGameObjects)
                    {
                        var manifestManager = rootGameObject.GetComponentInChildren<LocationManifestManager>();
                        if (manifestManager != null && manifestManager.KeepSynced)
                        {
                            manifestManager.PopulateAnchoredContent();
                            EditorSceneManager.SaveScene(scene);
                        }
                    }
                }
            }
        }
        
        private SerializedProperty spManifests;
        private SerializedProperty spAnchoredContent;
        

        private void OnEnable()
        {
            spManifests = serializedObject.FindProperty("_manifests");
            spAnchoredContent = serializedObject.FindProperty("_anchoredContent");
        }

        private static void ActiveManifestChanged(VPSLocationManifest locationManifest)
        {
            SyncOpenScenes();
        }

        public override void OnInspectorGUI()
        {
            var locationManifestManager = target as LocationManifestManager;
            
            // This gets the current values from all serialized fields into the serialized "clone"
            serializedObject.Update();
            
            EditorGUILayout.Space ();

            DropAreaGUI();

            EditorGUILayout.Space ();
            GUILayout.Label("Step 1: Drag Manifests into manifest list below");
            
            EditorGUILayout.PropertyField(spManifests, true);
            if (serializedObject.ApplyModifiedProperties())
            {
                Debug.LogWarning("Adjusting manifests requires repeating Step 2.");
            }
            EditorGUILayout.PropertyField(spAnchoredContent, true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space ();
            GUILayout.Label("Step 2: After adding in Manifests, Populate Anchors in build: ");
            EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            
            var guiStyle = EditorStyles.toggle;
            guiStyle.alignment = TextAnchor.MiddleLeft;
            locationManifestManager.SyncPrefabSelection = GUILayout.Toggle(locationManifestManager.SyncPrefabSelection,
                new GUIContent("Include Associated Prefabs",
                    "Populated anchors should use associated prefab content from the Remote Authoring Assistant"), guiStyle);
            
            locationManifestManager.KeepSynced = GUILayout.Toggle(locationManifestManager.KeepSynced,
                new GUIContent("Keep Synced",
                    "Keep Synced with local editor manifest"), guiStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button(new GUIContent("Populate Anchors", "Press this to populate anchors in the manager")))
            {
                locationManifestManager.PopulateAnchoredContent();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Manifests and Anchor Content"))
            {
                //Remove All Content;
                spManifests.arraySize = 0;
                spAnchoredContent.arraySize = 0;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
            
        }

        private void DropAreaGUI ()
        {
            var locationManifestManager = target as LocationManifestManager;
            Event evt = Event.current;
            Rect droppingArea = new Rect(0.0f, 35.0f, EditorGUIUtility.currentViewWidth, 25f);
            GUI.Box (droppingArea, "Drag Manifests (or JSON Manifest) Here");
     
            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!droppingArea.Contains (evt.mousePosition))
                        return;
             
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
         
                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag ();
             
                        if(DragAndDrop.objectReferences != null) {
                            locationManifestManager.Manifests = 
                                AddContentAsManifests(DragAndDrop.objectReferences, locationManifestManager.Manifests);
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        }
                    }
                    break;
            }
        }
        public RuntimeVPSLocationManifest[] AddContentAsManifests(Object[] draggedContent, RuntimeVPSLocationManifest[] existingManifests)
        {
            List<RuntimeVPSLocationManifest> manifestList = new List<RuntimeVPSLocationManifest>();
            if (existingManifests != null)
            {
                foreach (var vpsLocationManifest in existingManifests)
                {
                    if (vpsLocationManifest != null)
                    {
                        AddManifest(ref manifestList, vpsLocationManifest);
                    }
                }
            }

            foreach (var obj in draggedContent)
            {
                if (obj is VPSLocationManifest)
                {
                    VPSLocationManifest manifest = obj as VPSLocationManifest;
                    RuntimeVPSLocationManifest runtimeManifest = new RuntimeVPSLocationManifest(manifest);
                    AddManifest(ref manifestList, runtimeManifest);
                }
                else if (obj is TextAsset)
                {
                    var jsonString = (obj as TextAsset).text;
                    var manifest = JsonUtility.FromJson<RuntimeVPSLocationManifest>(jsonString);
                    AddManifest(ref manifestList, manifest);
                }
                else{ Debug.LogError("object " + obj.name + "is not a supported VPS Manifest");}
            }

            return manifestList.ToArray();
        }

        private void AddManifest(ref List<RuntimeVPSLocationManifest> manifestList, RuntimeVPSLocationManifest manifest)
        {
            if (manifestList.FindIndex(f => f.LocationName == manifest.LocationName) < 0)
            {
                manifestList.Add(manifest);
            }
            else
            {
                Debug.LogWarning("Duplicate detected");
            }
        }
    }
}