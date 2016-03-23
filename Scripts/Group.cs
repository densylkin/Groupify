using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Groupify
{
    [System.Serializable]
    public class Group
    {
        private bool locked;
        private bool hidden;
        public bool prevHidden;
        public bool highlighted;

        public bool foldout;
        public bool editing = false;

        public string name = "New group";
        public Color color = Color.white;

        public List<GameObject> objects = new List<GameObject>();
        private Transform rootObj;

        public bool Locked
        {
            get { return locked; }
            set { Lock(value); }
        }

        public bool Hidden
        {
            get { return hidden; }
            set { Hide(value); }
        }

        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                if(value != highlighted)
                {
                    highlighted = value;
                    if(value)
                        SceneView.onSceneGUIDelegate += OnSceneGUI;
                    else
                        SceneView.onSceneGUIDelegate -= OnSceneGUI;
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    if (rootObj != null)
                        rootObj.name = name;
                }     
            }
        }

        public bool Empty
        {
            get { return objects.Count == 0; }
        }

        public int Count
        {
            get { return objects.Count;  }
        }

        public void Add(IEnumerable<GameObject> objs)
        {
            objects.AddRange(objs.Except(objects));
        }

        public void RemoveObject(int index)
        {
            objects.RemoveAt(index);
        }

        public void Select()
        {
            if (Empty)
                return;

            Selection.objects = objects.ToArray();
        }

        public void Lock(bool state)
        {
            if (Empty || state == locked)
                return;

            HideFlags f = state ? HideFlags.NotEditable : HideFlags.None;

            foreach (var obj in objects)
                obj.hideFlags = f;

            locked = state;

#if UNITY_EDITOR
            foreach (var obj in objects)
                EditorUtility.SetDirty(obj);
#endif

        }

        public void Hide(bool state)
        {
            if (Empty || state == hidden)
                return;
            foreach (var obj in objects)
                obj.SetActive(!state);

            hidden = state;
        }

#if UNITY_EDITOR

        public void Organize()
        {
            CreateRoot();
            foreach (var obj in objects)
            {
                if (obj.transform.parent != rootObj)
                    Undo.SetTransformParent(obj.transform, rootObj, "");
            }
        }

        public void CreateRoot()
        {
            if (rootObj != null)
                return;

            rootObj = new GameObject(Name).transform;
            Undo.RegisterCreatedObjectUndo(rootObj.gameObject, "Group root");
        }
#endif
        public void OnSceneGUI(SceneView sceneView)
        {
            if (!Groupify.Instance.highlight || Empty || Hidden || !Groupify.Instance)
                return;

            Handles.color = new Color(color.r,
                color.g,
                color.b,
                0.5f);

            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                var renderer = obj.GetComponent<Renderer>();
                float size = renderer == null ? 1f : renderer.bounds.size.magnitude;
                Handles.CubeCap(0, obj.transform.position, obj.transform.rotation, size + 0.02f);
            }

            HandleUtility.Repaint();
        }
    }

}