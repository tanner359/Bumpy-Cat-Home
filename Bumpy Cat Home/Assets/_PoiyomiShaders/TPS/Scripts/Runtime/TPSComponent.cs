using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VRC_NEW_HOOK_API
using VRC.SDKBase;
#endif

namespace Thry.TPS
{
    public class TPSComponent : MonoBehaviour
#if VRC_NEW_HOOK_API
    , IEditorOnly
#endif 
    {
        public string AnimatorVersion;
        public Transform Root;
        public Renderer Renderer;
        public string Id;
        public Transform MasterTransform; // used to parent lights etc. to
        public bool IsAnimatorDirty;
        public int Channel;

        public Vector3 LocalPosition;
        public Quaternion LocalRotation = Quaternion.identity;
        
        [NonSerialized]
        public bool ShowHandles = false;
        public bool HandlesOnlyPosition = false;
    }
}