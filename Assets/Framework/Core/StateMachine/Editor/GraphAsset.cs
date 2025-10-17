using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.StateMachine.Graph
{
    [CreateAssetMenu(menuName = "StateMachine/StateGraph")]
    public class StateGraph : ScriptableObject
    {
        public List<StateNode> nodes = new();
        public List<StateEdge> edges = new();
        public string initialStateNodeId;

        [Serializable]
        public class StateNode
        {
            public string Id;
            public string Title;
            public Vector2 Position;
            public string Comment;
            public Rect rect = new Rect(100, 100, 160, 64);
        }

        [Serializable]
        public class StateEdge
        {
            public string Id;
            public string FromNodeId;
            public string ToNodeId;
            public string Trigger;
            public string Comment;
        }

        public StateNode FindNode(string id) => nodes.Find(n => n.Id == id);
        public StateEdge FindEdge(string id) => edges.Find(e => e.Id == id);

        // 编辑器使用：确保每个节点/连线有 id
        public void EnsureIds()
        {
            foreach (var n in nodes) if (string.IsNullOrEmpty(n.Id)) n.Id = Guid.NewGuid().ToString();
            foreach (var e in edges) if (string.IsNullOrEmpty(e.Id)) e.Id = Guid.NewGuid().ToString();
        }
    }


}