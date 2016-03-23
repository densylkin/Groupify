using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
#if UNITY_5_3
using UnityEditor.SceneManagement;
#endif

namespace Groupify
{
    public class GroupifyWindow : EditorWindow
    {
        public Groupify Groupify;
        private SerializedObject serilizedGroupify;

        private string searchStr = "";
        private Vector2 scroll;

        private Texture2D eyeIcon;
        private Texture2D handIcon;
        private Texture2D lockIcon;
        private Texture2D listIcon;
        private Texture2D lightIcon;
        private Texture2D upIcon;
        private Texture2D downicon;
        private Texture2D cubesIcon;

        private string curScene;

        private int btnSize = 20;

        private List<Group> results = new List<Group>();

        [MenuItem("Tools/Groupify/Open groupify")]
        public static void Init()
        {
            GroupifyWindow window = EditorWindow.GetWindow<GroupifyWindow>();

            window.minSize = new Vector2(370, 250);
            window.Show();

            if (window.Groupify == null)
                window.Groupify = FindObjectOfType<Groupify>();
        }

        public static void Init(Groupify curGroupify)
        {
            GroupifyWindow window = EditorWindow.GetWindow<GroupifyWindow>();

            window.minSize = new Vector2(300, 250);
            window.Show();

            window.Groupify = curGroupify;
            if(window.Groupify == null)
                window.Groupify = FindObjectOfType<Groupify>();
        }

        [MenuItem("Tools/Groupify/Create group from selected %g")]
        public static void CreateGroupFromSelection()
        {
            if (Groupify.Instance == null)
                CreateGroupify();

            Groupify.Instance.CreateFrom(Selection.gameObjects);
            EditorWindow.GetWindow<GroupifyWindow>().Repaint();
        }

        [MenuItem("Tools/Groupify/Create Groupify")]
        public static void CreateGroupify()
        {
            if (FindObjectOfType<Groupify>() != null)
                return;

            var obj = new GameObject("Groupify").AddComponent<Groupify>();
            obj.gameObject.tag = "EditorOnly";
            GetWindow<GroupifyWindow>().Groupify = obj;
            Selection.activeObject = obj.gameObject;
        }

        [MenuItem("Assets/Save Editor Skin")]
        static public void SaveEditorSkin()
        {
            GUISkin skin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
            AssetDatabase.CreateAsset(skin, "Assets/EditorSkin.guiskin");
        }

        private void OnEnable()
        {
            EditorApplication.hierarchyWindowChanged += OnSceneChanged;

            
#if UNITY_5_3
            curScene = EditorSceneManager.GetActiveScene().name;
#else
            curScene = EditorApplication.currentScene;
#endif
            LoadTextures();
            if (!Groupify)
                Groupify = FindObjectOfType<Groupify>();

            if(Groupify != null)
                serilizedGroupify = new SerializedObject(Groupify);
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
            if (!Groupify)
                return;
        }

        private void OnSceneChanged()
        {
#if UNITY_5_3
            if (curScene != EditorSceneManager.GetActiveScene().name)
#else
            if(curScene != EditorApplication.currentScene)
#endif
            {
                OnDisable();
                OnEnable();
                Repaint();
#if UNITY_5_3
                curScene = EditorSceneManager.GetActiveScene().name;
#else
                curScene = EditorApplication.currentScene;
#endif
            }
        }

        private void LoadTextures()
        {
            eyeIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "eye_pro" : "eye_indie");
            handIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "hand_pro" : "hand_indie");
            lockIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "lock_pro" : "lock_indie");
            listIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "list_pro" : "list_indie");
            lightIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "light_pro" : "light_indie");
            upIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "arrowup_pro" : "arrowup_indie");
            downicon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "arrowdown_pro" : "arrowdown_indie");
            cubesIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "cubes_pro" : "cubes_indie");
        }

        private void OnGUI()
        {
            if (Groupify)
            {
                Undo.RecordObject(Groupify, "Groupify");
                TopToolbar();
                DropAreaCreateGroup();
                GUILayout.Space(3f);
                GroupsArea();
                EditorUtility.SetDirty(Groupify);
                
            }
            else
                NoGroupify();
        }

        private void GroupsArea()
        {
            if (!string.IsNullOrEmpty(searchStr))
                UpdateSearchResults();
            else
                results = Groupify.groups;
            if (results.Count > 0)
            {
                using (new VerticalBlock())
                {
                    using (new ScrollviewBlock(ref scroll))
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            var group = results[i];
                            DrawGroup(group, i);
                            GUILayout.Space(2f);
                        }
                    }
                }
            }
            GUILayout.FlexibleSpace();
            Groupify.Visible = GUILayout.Toggle(Groupify.Visible, (Groupify.Visible ? "Hide" : "Show") + " groupify object", EditorStyles.toolbarButton);
        }

        private void NoGroupify()
        {
            using (new VerticalBlock())
            {
                GUILayout.FlexibleSpace();
                using (new HorizontalBlock())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Create groupify"))
                        GroupifyWindow.CreateGroupify();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void TopToolbar()
        {
            using (new HorizontalBlock(EditorStyles.toolbar))
            {
                SearchField();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent(listIcon, "Organize hierarchy"), EditorStyles.toolbarButton, GUILayout.Width(btnSize)))
                    EditorApplication.delayCall += Groupify.OrganizeHierarchy;

                if (GUILayout.Button(new GUIContent(cubesIcon, "Create empty group"), EditorStyles.toolbarButton, GUILayout.Width(btnSize)))
                    EditorApplication.delayCall += Groupify.CreateEmpty;

                Groupify.highlight = GUILayout.Toggle(Groupify.highlight, new GUIContent(lightIcon, "Allow highlighting"), EditorStyles.toolbarButton, GUILayout.Width(btnSize));
            }
        }

        private void SearchField()
        {
            searchStr = GUILayout.TextField(searchStr, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true), GUILayout.MinWidth(150));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                searchStr = "";
                GUI.FocusControl(null);
            }
        }

        private void DrawGroup(Group group, int i)
        {
            using (new HorizontalBlock(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                GUILayout.Space(5f);
                group.editing = EditorGUILayout.Toggle(group.editing, EditorStyles.foldout, GUILayout.Width(10));
                if (group.editing)
                    group.Name = GUILayout.TextField(group.Name, GUILayout.Width(100));
                else
                {
                    using (new ColoredBlock(group.Locked || group.Hidden ? EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f) : Color.white))
                    {
                        GUILayout.Label(group.Name + "(" + group.Count + ")", GUILayout.Width(100));
                    }
                }

                //Select
                if (GUILayout.Button(new GUIContent(handIcon, "Select objects"), EditorStyles.toolbarButton, GUILayout.Width(btnSize))) group.Select();
                //Hide
                group.Hidden = GUILayout.Toggle(group.Hidden, new GUIContent(eyeIcon, "Hide objects"), EditorStyles.toolbarButton, GUILayout.Width(btnSize));
                //Lock
                group.Locked = GUILayout.Toggle(group.Locked, new GUIContent(lockIcon, "Freeze objects"), EditorStyles.toolbarButton, GUILayout.Width(btnSize));
                //Highlight
                group.Highlighted = GUILayout.Toggle(group.Highlighted, new GUIContent(lightIcon, "Highlight group objects"), EditorStyles.toolbarButton, GUILayout.Width(btnSize));

                GUILayout.FlexibleSpace();
                MoveGroupBtns(i, group);

                if (GUILayout.Button(new GUIContent("X", "Delete this group"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    Groupify.RemoveGroup(group);
                    SceneView.onSceneGUIDelegate -= group.OnSceneGUI;
                }
            }

            DropArea(GUILayoutUtility.GetLastRect(), (objects) =>
            {
                group.Add(objects);
            });

            EditGroup(group);
        }

        private void MoveGroupBtns(int i, Group group)
        {
            if (i > 0)
            {
                if (GUILayout.Button(new GUIContent(upIcon, "Move group up"), EditorStyles.toolbarButton, GUILayout.Width(22)))
                    Groupify.MoveGroup(i, true);
            }
            else
                GUILayout.Space(20f);

            if (i < Groupify.groups.Count - 1)
            {
                if (GUILayout.Button(new GUIContent(downicon, "Move group down"), EditorStyles.toolbarButton, GUILayout.Width(22)))
                    Groupify.MoveGroup(i, false);
            }
            else
                GUILayout.Space(20f);
        }

        private void EditGroup(Group group)
        {
            if (group.editing)
            {
                GUILayout.Space(7f);
                group.color = EditorGUILayout.ColorField("Color: ", group.color);

                GUILayout.Space(5f);
                if (!group.Empty)
                {
                    using (new VerticalBlock(GUI.skin.box))
                    {
                        for (int i = 0; i < group.Count; i++)
                        {
                            using (new HorizontalBlock())
                            {
                                GUILayout.Label(group.objects[i].name);
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("select", EditorStyles.miniButton))
                                    Selection.objects = new GameObject[] { group.objects[i] };
                                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(25)))
                                    group.RemoveObject(i);
                            }
                        }
                    }
                }
            }
        }

        private void DropAreaCreateGroup()
        {
            GUILayout.Box("Drop objects here to create a new group", GUI.skin.GetStyle("flow node 0"), GUILayout.ExpandWidth(true), GUILayout.Height(25f));

            DropArea(GUILayoutUtility.GetLastRect(), (objects) =>
            {
                Groupify.CreateFrom(objects);
            });
        }

        private void UpdateSearchResults()
        {
            results = string.IsNullOrEmpty(searchStr) ? Groupify.groups : Groupify.groups
                .Where(group => group.Name.Contains(searchStr))
                .ToList();
        }

        private void DropArea(Rect rect, System.Action<IEnumerable<GameObject>> Callback)
        {
            EventType eventType = Event.current.type;
            if (!rect.Contains(Event.current.mousePosition) || !(eventType == EventType.DragUpdated || eventType == EventType.DragPerform))
                return;

            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
            }

            if (isAccepted)
            {
                var objects = DragAndDrop.objectReferences
                    .Where(obj => obj.GetType() == typeof(GameObject))
                    .Cast<GameObject>()
                    .Where(obj => PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab && PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab);
                Callback(objects);
            }
        }

    }
}