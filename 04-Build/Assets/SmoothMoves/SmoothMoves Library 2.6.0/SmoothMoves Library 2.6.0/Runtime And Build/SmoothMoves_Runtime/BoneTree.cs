using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    public class BoneTree
    {
        public BoneTree parent;
        public List<BoneTree> children;

        public int boneDataIndex;
        public int depth;

        public bool IsLeaf { get { return children.Count == 0; } }

        public int Count
        {
            get
            {
                int count = 1;
                foreach (BoneTree child in children)
                {
                    count += child.Count;
                }
                return count;
            }
        }

        public BoneTree()
        {
            Initialize(null, 0, 0);
        }

        public BoneTree(BoneTree p, int bdIndex, int d)
        {
            Initialize(p, bdIndex, d);
        }

        private void Initialize(BoneTree p, int bdIndex, int d)
        {
            parent = p;
            children = new List<BoneTree>();

            boneDataIndex = bdIndex;
            depth = d;
        }

        public BoneTree AddNewChild(int bdIndex)
        {
            BoneTree child = new BoneTree(this, bdIndex, depth + 1);
            children.Add(child);

            return child;
        }

        public void AddExistingChild(BoneTree child)
        {
            child.SetParent(this);
            children.Add(child);
        }

        public void SetParent(BoneTree p)
        {
            Orphan();
            parent = p;
            SetDepth((parent == null ? 0 : parent.depth + 1));
        }

        public void Orphan()
        {
            if (parent != null)
            {
                parent.RemoveChild(this);
            }
            parent = null;

            SetDepth(0);
        }

        public void RemoveChild(BoneTree child)
        {
            children.Remove(child);
        }

        public BoneTree GetChild(int i)
        {
            foreach (BoneTree n in children)
                if (i-- == 0) return n;
            return null;
        }

        public void Clear()
        {
            if (children.Count > 0)
            {
                BoneTree child = children[0];
                while (child != null)
                {
                    child.Clear();
                    if (children.Count > 0)
                    {
                        child = children[0];
                    }
                    else
                    {
                        child = null;
                    }
                }

                children.Clear();
            }

            if (parent != null)
            {
                parent.RemoveChild(this);
            }
        }

        public void AddDFSChildren(ref List<DFSBoneNode> list, int dfsParentIndex, ref int dfsIndex)
        {
            DFSBoneNode node = new DFSBoneNode(dfsParentIndex, boneDataIndex, depth);
            list.Add(node);

            dfsParentIndex = dfsIndex;

            foreach (BoneTree child in children)
            {
                dfsIndex++;
                child.AddDFSChildren(ref list, dfsParentIndex, ref dfsIndex);
            }
        }

        public bool HasDescendant(BoneTree descendant)
        {
            if (descendant == this)
                return true;

            foreach (BoneTree child in children)
            {
                if (child.HasDescendant(descendant))
                    return true;
            }

            return false;
        }

        public void SetDepth(int d)
        {
            depth = d;

            foreach (BoneTree child in children)
            {
                child.SetDepth(depth + 1);
            }
        }

        public BoneTree GetBoneTreeWithDataIndex(int bdIndex)
        {
            if (bdIndex == -1)
                return null;

            if (boneDataIndex == bdIndex)
                return this;

            BoneTree bone;

            foreach (BoneTree child in children)
            {
                bone = child.GetBoneTreeWithDataIndex(bdIndex);
                if (bone != null)
                    return bone;
            }

            return null;
        }
    }
}