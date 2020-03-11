using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraSystem))]
public class CameraSystemEditor : Editor
{
    private bool showCameraInfo = true;
    private string[] excludedVariables;

    readonly string cameraSystemLabel = "Camera System:";
    readonly string cameraTypeLabel = "Camera Type:";
    readonly string cameraModeLabel = "Camera Mode:";
    readonly string showCameraInfoLabel = "Show Camera Info";
    readonly string firstPersonCameraLabel = "First Person Camera.\nA camera that lies inside the character " +
                                             "and replicates humans way of seeing things through their eyes.";
    readonly string fixedCameraLabel = "As the name says, a tracking camera follows the characters from behind. " +
                                       "The player does not control the camera in any way - he/she cannot for " +
                                       "example rotate it or move it to a different position. This type of camera " +
                                       "system was very common in early 3D games such as Crash Bandicoot or " +
                                       "Tomb Raider since it is very simple to implement. However, there are a " +
                                       "number of issues with it. In particular, if the current view is not " +
                                       "suitable (either because it is occluded by an object, or because it is " +
                                       "not showing what the player is interested in), it cannot be changed " +
                                       "since the player does not control the camera. Sometimes this " +
                                       "viewpoint causes difficulty when a character turns or stands face out " +
                                       "against a wall. The camera may jerk or end up in awkward positions.";
    readonly string trackingCameraLabel = "In this kind of system, the developers set the properties of the camera, " +
                                          "such as its position, orientation or field of view, during the game " +
                                          "creation.The camera views will not change dynamically, so the same " +
                                          "place will always be shown under the same set of views. An early example " +
                                          "of this kind of camera system can be seen in Alone in the Dark. While the " +
                                          "characters are in 3D, the background on which they evolve has been " +
                                          "pre-rendered.The early Resident Evil games are notable examples of games " +
                                          "that use fixed cameras.The God of War series of video games is also known " +
                                          "for this technique.One advantage of this camera system is that it allows " +
                                          "the game designers to use the language of film.Indeed, like filmmakers, " +
                                          "they have the possibility to create a mood through camerawork and careful " +
                                          "selection of shots.Games that use this kind of technique are often praised " +
                                          "for their cinematic qualities.";
    readonly string interactiveCameraLabel = "This type of camera system is an improvement over the tracking camera system. " +
                                             "While the camera is still tracking the character, some of its parameters, such " +
                                             "as its orientation or distance to the character, can be changed. On video game " +
                                             "consoles, the camera is often controlled by an analog stick to provide a good " +
                                             "accuracy, whereas on PC games it is usually controlled by the mouse. This is the " +
                                             "case in games such as Super Mario Sunshine or The Legend of Zelda: The Wind Waker. " +
                                             "Fully interactive camera systems are often difficult to implement in the right way. " +
                                             "Thus GameSpot argues that much of the Super Mario Sunshine' difficulty comes from " +
                                             "having to control the camera. The Legend of Zelda: The Wind Waker was more " +
                                             "successful at it - IGN called the camera system \"so smart that it rarely needs " +
                                             "manual correction\".";
    readonly string freemodeCameraLabel = "As the name suggests a free mode camera is a camera the player can move " +
                                          "as they wish. The camera is not locked to anything and can be moved " +
                                          "anywhere the player desires.";
    public void OnEnable()
    {
        excludedVariables = new string[] { "cameraType", "thirdPersonCameraType", "m_Script", "useRightClickToRotateCamera" };
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(cameraSystemLabel, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        CameraSystem cameraSystem = target as CameraSystem;

        EditorGUI.BeginChangeCheck();
        cameraSystem.cameraType = (CameraType)EditorGUILayout.EnumPopup(cameraTypeLabel, cameraSystem.cameraType);

        if (cameraSystem.cameraType == CameraType.ThirdPersonCamera)
        {
            cameraSystem.thirdPersonCameraType = (ThirdPersonCameraType)EditorGUILayout.EnumPopup(cameraModeLabel, cameraSystem.thirdPersonCameraType);
        }

        if (EditorGUI.EndChangeCheck())
        {
            if (cameraSystem.cameraType == CameraType.ThirdPersonCamera)
            {
                cameraSystem.ChangeCameraOffset(cameraSystem.GetDefaultOffset(cameraSystem.thirdPersonCameraType));
            }
            else
            {
                cameraSystem.ChangeCameraOffset(cameraSystem.GetDefaultOffset(cameraSystem.cameraType));
            }

            cameraSystem.UpdateCameraPosition();
        }
        EditorGUILayout.Space();

        EditorGUILayout.GetControlRect(true, 16.0f, EditorStyles.foldout);
        Rect foldRect = GUILayoutUtility.GetLastRect();

        showCameraInfo = EditorGUI.Foldout(foldRect, showCameraInfo, showCameraInfoLabel);

        if (showCameraInfo)
        {
            string cameraInformation = string.Empty;

            if (cameraSystem.cameraType == CameraType.FirstPersonCamera)
            {
                cameraInformation = firstPersonCameraLabel;
            }
            else if (cameraSystem.cameraType == CameraType.ThirdPersonCamera)
            {
                switch (cameraSystem.thirdPersonCameraType)
                {
                    case ThirdPersonCameraType.Tracking:
                        cameraInformation = trackingCameraLabel;
                        break;
                    case ThirdPersonCameraType.Fixed:
                        cameraInformation = fixedCameraLabel;
                        break;
                    case ThirdPersonCameraType.Interactive:
                        cameraInformation = interactiveCameraLabel;
                        break;
                }
            }
            else
            {
                cameraInformation = freemodeCameraLabel;
            }

            EditorGUILayout.HelpBox(cameraInformation, MessageType.Info);
        }

        EditorGUILayout.Space(20, true);

        DrawPropertiesExcluding(serializedObject, excludedVariables);
        serializedObject.ApplyModifiedProperties();
    }
}