using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.Animations;
using static Thry.TPS.Orifice;
using UnityEditor.Experimental.SceneManagement;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Avatars.Components;
#endif

namespace Thry.TPS
{
    [CustomEditor(typeof(Orifice))]
    public class OrificeEditor : TPSComponentEditor
    {
        Orifice _orifice;

        public override void OnInspectorGUI()
        {
            if(this.target == null) return;
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if(_isPrefabEditing) return;

            _orifice = (Orifice)target;

            using (new SectionScope("3. Settings", 20, new Color(0.0f, 1.0f, 1.0f, 0.5f)))
            {
                GUILayout.Space(5);
                SettingsGUI();
            }

            using (new SectionScope("4. Lights", 20, new Color(0.0f, 0.0f, 1.0f, 0.5f)))
            {
                GUILayout.Space(5);
                LightsGUI();
            }

            #if VRC_SDK_VRCSDK3 && !UDON
            using (new SectionScope("5. Contact Senders", 20, new Color(1.0f, 0.0f, 1.0f, 0.5f)))
            {
                GUILayout.Space(5);
                ContactSendersGUI();
            }

            using (new SectionScope("6. Animator", 20, new Color(1.0f, 0.0f, 0.0f, 0.5f)))
            {
                GUILayout.Space(5);
                AnimatorGUI();	
            }
            #endif

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_component);
            }
        }

        private void OnDisable() 
        {
            if(this.target == null && !_isPrefabEditing)
            {
                OnHasBeenRemoved();
            }
        }

        void OnHasBeenRemoved()
        {
            if(_orifice.LightPosition != null)
                GameObject.DestroyImmediate(_orifice.LightPosition.gameObject);
            if(_orifice.LightNormal != null)
                GameObject.DestroyImmediate(_orifice.LightNormal.gameObject);

#if VRC_SDK_VRCSDK3 && !UDON && UNITY_EDITOR
            if(_orifice.ContactSenderPosition != null)
                GameObject.DestroyImmediate(_orifice.ContactSenderPosition.gameObject);
            if(_orifice.ContactSenderNormal != null)
                GameObject.DestroyImmediate(_orifice.ContactSenderNormal.gameObject);
            if(_orifice.MasterTransform != null)
                GameObject.DestroyImmediate(_orifice.MasterTransform.gameObject);

            if(_animator == null) return;
            OrificeSetup.DeleteOrificeLayers(_orifice, _animator);
            OrificeSetup.DeleteOrificeParameters(_orifice, _animator);
            OrificeSetup.RemoveContactReceivers(_orifice);
#endif
        }

        protected override void SettingsGUI()
        {
            base.SettingsGUI();
            EditorGUI.BeginChangeCheck();
            OrificeType newType = (OrificeType)EditorGUILayout.EnumPopup("Type", _orifice.Type, GUILayout.Height(INPUT_HEIGHT));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_orifice, "Changed Type");
                _orifice.Type = newType;
                _component.IsAnimatorDirty = true;
            }
        }

        void LightsGUI()
        {
            OrificeSetup.ValidateLights(_orifice);

            if(!_showDebugInformation) return;

            EditorGUI.BeginChangeCheck();
            bool newUseNormalLight = EditorGUILayout.Toggle("Use Normal Light", _orifice.UseNormalLight, GUILayout.Height(INPUT_HEIGHT));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_orifice, "Changed Use Normal Light");
                _orifice.UseNormalLight = newUseNormalLight;
                OrificeSetup.ValidateLights(_orifice);
            }
            EditorGUI.BeginDisabledGroup(true);
            // List lights
            EditorGUILayout.ObjectField("Position Light", _orifice.LightPosition, typeof(Light), true, GUILayout.Height(INPUT_HEIGHT));
            if(_orifice.UseNormalLight)
                EditorGUILayout.ObjectField("Normal Light", _orifice.LightNormal, typeof(Light), true, GUILayout.Height(INPUT_HEIGHT));
            EditorGUI.EndDisabledGroup();
        }

        #if VRC_SDK_VRCSDK3 && !UDON
        void ContactSendersGUI()
        {
            OrificeSetup.ValidateContactSenders(_orifice);

            if(!_showDebugInformation) return;
            EditorGUI.BeginDisabledGroup(true);
            // List contact senders
            EditorGUILayout.ObjectField("Position Contact Sender", _orifice.ContactSenderPosition, typeof(VRCContactSender), true, GUILayout.Height(INPUT_HEIGHT));
            EditorGUILayout.ObjectField("Normal Contact Sender", _orifice.ContactSenderNormal, typeof(VRCContactSender), true, GUILayout.Height(INPUT_HEIGHT));
            EditorGUI.EndDisabledGroup();
        }

        AnimatorController _animator;
        string[] _blendshapeNames;
        void AnimatorGUI()
        {
            EditorGUI.BeginChangeCheck();
            bool doAnimatorSetup = EditorGUILayout.Toggle("Setup Animator", _orifice.DoAnimatorSetup, GUILayout.Height(INPUT_HEIGHT));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_orifice, "Changed Setup Animator");
                _orifice.DoAnimatorSetup = doAnimatorSetup;
                _orifice.IsAnimatorDirty = true;
                if(!doAnimatorSetup)
                {
                    if(_animator != null)
                    {
                        OrificeSetup.DeleteOrificeLayers(_orifice, _animator);
                        OrificeSetup.DeleteOrificeParameters(_orifice, _animator);
                        OrificeSetup.RemoveContactReceivers(_orifice);
                    }
                    
                    _orifice.AnimatorVersion = "";
                    _orifice.IsAnimatorDirty = false;
                }
            }

            if(!_orifice.DoAnimatorSetup)
                return;

            VRCAvatarDescriptor descriptor = _avatarRoot.GetComponent<VRCAvatarDescriptor>();
            if(descriptor == null)
            {
                EditorGUILayout.HelpBox("No VRCAvatarDescriptor found on the avatar root!", MessageType.Error);
                return;
            }
            if(_animator == null)
                _animator = AnimatorHelper.GetFXController(descriptor);

            OrificeSetup.AssertContactReceivers(_orifice);

            this.GUIGizmosAll();
            GUILayout.Space(15);
            
            if(_showDebugInformation)
            {
                EditorGUILayout.ObjectField("Animator", _animator, typeof(AnimatorController), true, GUILayout.Height(INPUT_HEIGHT));
                GUILayout.Space(15);
            }

            ShapekeyGUI();
            GUILayout.Space(15);

            if(_orifice.IsAnimatorDirty)
            {
                EditorGUILayout.HelpBox("Animator is dirty! Please run the setup!", MessageType.Warning);
            }
            if(_orifice.AnimatorVersion != Helper.Version)
            {
                EditorGUILayout.HelpBox("Animator version is outdated! Please run the setup!", MessageType.Warning);
            }
            OrificeSetup.AssertOrificeLayers(_orifice, _animator, createIfMissing: false);
            if(GUILayout.Button("Run Layer Setup", GUILayout.Height(BUTTON_HEIGHT)))
            {
                OrificeSetup.AssertOrificeLayers(_orifice, _animator);
                OrificeSetup.SetupOrificeLayers(_orifice, _animator, _avatarRoot);
                _orifice.IsAnimatorDirty = false;
                _orifice.AnimatorVersion = Helper.Version;
            }
            EditorGUI.BeginDisabledGroup(_orifice.Layer_Depth == null && _orifice.Layer_Width == null);
            if(GUILayout.Button("Delete Layer Setup"))
            {
                OrificeSetup.DeleteOrificeLayers(_orifice, _animator);
                _orifice.IsAnimatorDirty = true;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(15);

            if(!_showDebugInformation) return;
            EditorGUI.BeginDisabledGroup(true);
            // List contact receivers
            GUILayout.Label("Contact Revievers", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Contact Reciever Is Penetrating", _orifice.ContactReceiver_IsPenetrating, typeof(VRCContactReceiver), true, GUILayout.Height(INPUT_HEIGHT));
            EditorGUILayout.ObjectField("Contact Reciever Width 0", _orifice.ContactReceiver_Width0, typeof(VRCContactReceiver), true, GUILayout.Height(INPUT_HEIGHT));
            EditorGUILayout.ObjectField("Contact Reciever Width 1", _orifice.ContactReceiver_Width1, typeof(VRCContactReceiver), true, GUILayout.Height(INPUT_HEIGHT));
        
            GUILayout.Space(15);
            // List layer names
            GUILayout.Label("Layer Names", EditorStyles.boldLabel);
            EditorGUILayout.TextField("Layer Width", _orifice.LayerName_Width, GUILayout.Height(INPUT_HEIGHT));
            EditorGUILayout.TextField("Layer Depth", _orifice.LayerName_Depth, GUILayout.Height(INPUT_HEIGHT));
            EditorGUI.EndDisabledGroup();
        }

        void ShapekeyGUI()
        {
            GUILayout.Label("Shapekeys Setup", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            RendererGUI();
            if(EditorGUI.EndChangeCheck() || (_orifice.Renderer != null && _blendshapeNames == null))
            {
                _blendshapeNames = OrificeSetup.LoadBlendshapeNames(_orifice.Renderer);
            }

            if(_orifice.Renderer == null)
                return;

            // Shapekey list
            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Shapekey");
            GUILayout.Label("Normalized Depth");
            GUILayout.EndHorizontal();
            for(int i = 0; i < _orifice.OpeningShapekeys.Length; i++)
            {
                ShapekeyConfig shapekey = _orifice.OpeningShapekeys[i];
                int idx = 0;
                if(string.IsNullOrEmpty(shapekey.shapekeyName) == false)
                    idx = ArrayUtility.IndexOf(_blendshapeNames, shapekey.shapekeyName);
                if(idx == -1) idx = 0;

                GUILayout.BeginHorizontal();
                idx = EditorGUILayout.Popup(idx, _blendshapeNames, GUILayout.Height(INPUT_HEIGHT));
                shapekey.shapekeyName = idx > 0 ? _blendshapeNames[idx] : "";
                shapekey.depth = EditorGUILayout.FloatField(shapekey.depth, GUILayout.Height(INPUT_HEIGHT));
                _orifice.OpeningShapekeys[i] = shapekey;
                if(GUILayout.Button("X", GUILayout.Height(INPUT_HEIGHT), GUILayout.Width(20)))
                {
                    ArrayUtility.RemoveAt(ref _orifice.OpeningShapekeys, i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+", GUILayout.Height(INPUT_HEIGHT), GUILayout.Width(20)))
            {
                ArrayUtility.Add(ref _orifice.OpeningShapekeys, new ShapekeyConfig());
            }
            if(GUILayout.Button("-", GUILayout.Height(INPUT_HEIGHT), GUILayout.Width(20)))
            {
                ArrayUtility.RemoveAt(ref _orifice.OpeningShapekeys, _orifice.OpeningShapekeys.Length - 1);
            }
            EditorGUILayout.EndHorizontal();

            if(EditorGUI.EndChangeCheck())
            {
                _orifice.IsAnimatorDirty = true;
            }
        }
        #endif

        private void OnSceneGUI()
        {
            Orifice orifice = (Orifice)target;
            Transform root = orifice.Root;

            if(root == null || !orifice.ShowHandles)
                return;

            Vector3 globalPosition = root.TransformPoint(orifice.LocalPosition);
            Quaternion globalRotation = root.rotation * orifice.LocalRotation;
            Vector3 forward = globalRotation * Vector3.forward;

            EditorGUI.BeginChangeCheck();
            globalPosition = Handles.PositionHandle(globalPosition, globalRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(orifice, "Changed Position");
                orifice.LocalPosition = root.InverseTransformPoint(globalPosition);
                orifice.IsAnimatorDirty = true;
            }

            EditorGUI.BeginChangeCheck();
            globalRotation = Handles.RotationHandle(globalRotation, globalPosition);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(orifice, "Changed Rotation");
                orifice.LocalRotation = Quaternion.Inverse(root.rotation) * globalRotation;
                orifice.IsAnimatorDirty = true;
            }

            if(orifice.HandlesOnlyPosition)
                return;

            float radius = orifice.Radius;
            float depth = orifice.Depth;
            Vector3 middle = globalPosition - forward * depth / 2;
            Vector3 start = globalPosition;
            Vector3 end = globalPosition - forward * depth;

            EditorGUI.BeginChangeCheck();
            end = Handles.Slider(end, -forward, HandleUtility.GetHandleSize(end) * 0.1f, DepthHandleCap, 0.0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(orifice, "Changed Depth");
                orifice.Depth = Mathf.Max(0.01f, Vector3.Distance(start, end));
                orifice.IsAnimatorDirty = true;
            }

            EditorGUI.BeginChangeCheck();
            radius = Handles.RadiusHandle(globalRotation, middle, radius, true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(orifice, "Changed Radius");
                orifice.Radius = Mathf.Max(0.01f, radius);
                orifice.IsAnimatorDirty = true;
            }   
        }

        void DepthHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            Orifice orifice = (Orifice)target;
            Handles.CircleHandleCap(controlID, position, rotation, orifice.Radius, eventType);
        }
    }
}