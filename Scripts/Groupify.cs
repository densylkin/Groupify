using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Groupify
{
    public class Groupify : MonoBehaviour
    {
        #region Singleton

        private static Groupify instance;

        public static Groupify Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = FindObjectOfType<Groupify>();
                    //if (instance == null)
                    //    Debug.LogError("An instance of Groupify is needed in the scene, but there is none.");
                }
                return instance;
            }
        }

        #endregion

        public List<Group> groups = new List<Group>();
        public bool highlight;

        public int GroupsCount
        {
            get { return groups.Count; }
        } 

        public bool Empty
        {
            get { return groups.Count == 0; }
        }

        public bool Visible
        {
            get
            {
                if (gameObject.hideFlags == HideFlags.None)
                    return true;
                else if (gameObject.hideFlags == HideFlags.HideInHierarchy)
                    return false;
                else
                    return false;
            }
            set
            { 
                gameObject.hideFlags = value ? HideFlags.None : HideFlags.HideInHierarchy;
            }
        }

        public void CreateFrom(IEnumerable<GameObject> objects)
        {
            var newGroup = new Group();
            newGroup.Add(objects);
            groups.Add(newGroup);
        }

        public void CreateEmpty()
        {
            groups.Add(new Group());
        }

        public void RemoveGroup(Group group)
        {
            groups.Remove(group);
        }

        public void OrganizeHierarchy()
        {
            if (Empty)
                return;

            foreach (var group in groups)
                group.Organize();
        }

        public void MoveGroup(int index, bool dir)
        {
            var group = groups[index];
            groups.RemoveAt(index);
            groups.Insert(index + (dir ? -1 : 1), group);
        }
    }
}