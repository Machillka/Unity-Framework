using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Framework.Core.StateMachine.Graph
{
    public class StateGraphEditor : EditorWindow
    {
        // 当前编辑的 asset
        StateGraph graph;
        // 编辑器状态
        Vector2 canvasScroll = Vector2.zero;    // 当前滚动 / 平移偏移
        float canvasZoom = 1f;                  // 缩放因子（1 = 100%）
        const float MIN_ZOOM = 0.5f;
        const float MAX_ZOOM = 2.0f;

        // 选择、拖动状态
        StateGraph.StateNode selectedNode = null;
        StateGraph.StateEdge selectedEdge = null;
        bool isDraggingNode = false;
        Vector2 dragStartMouse;
        Rect nodeDragStartRect;
        bool isPanning = false;
        Vector2 panStartMouse;
        Vector2 panStartOffset;

        // 连接线创建模式
        bool isConnecting = false;
        string connectFromNodeId = null; // 起始节点 id
        Vector2 mousePositionWhenConnecting;

        // GUI 样式
        GUIStyle nodeStyle;
        GUIStyle nodeSelectedStyle;
        GUIStyle edgeLabelStyle;
        GUIStyle miniButton;

        // 上次鼠标位置（用于 zoom/pan）
        Vector2 lastMousePos;

        // 打开窗口
        [MenuItem("Window/SimpleSM/State Graph Editor")]
        public static void OpenWindow()
        {
            var w = GetWindow<StateGraphEditor>();
            w.titleContent = new GUIContent("StateGraph");
            w.minSize = new Vector2(600, 400);
        }

        void OnEnable()
        {
            // 初始化样式
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            nodeStyle.padding = new RectOffset(10, 10, 8, 8);
            nodeStyle.normal.textColor = Color.white;

            nodeSelectedStyle = new GUIStyle(nodeStyle);
            nodeSelectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;

            edgeLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.yellow }, fontStyle = FontStyle.Bold };
            miniButton = new GUIStyle(EditorStyles.miniButton) { fixedHeight = 20 };
        }

        void OnGUI()
        {
            DrawToolbar();

            // 当没有选择 graph 时显示帮助
            if (graph == null)
            {
                DrawNoGraph();
                ProcessNoGraphShortcuts();
                return;
            }

            // 处理输入（放在绘制之前以便处理拖拽等）
            ProcessInput(Event.current);

            // 画布坐标系变换（缩放 + 平移）
            var canvasRect = new Rect(0, 20, position.width, position.height - 20);
            GUI.BeginGroup(canvasRect);
            // 使用Handles.matrix 简化缩放与位移逻辑：先 translate 到中心，再 scale，再 translate 回
            Matrix4x4 oldMatrix = GUI.matrix;
            var translate = Matrix4x4.Translate(new Vector3(canvasScroll.x, canvasScroll.y, 0f));
            GUI.matrix = translate * Matrix4x4.Scale(new Vector3(canvasZoom, canvasZoom, 1)) * translate.inverse * GUI.matrix;

            // Draw grid
            DrawGrid(canvasRect);

            // Draw edges first
            DrawEdges();

            // Draw nodes
            DrawNodes();

            // Draw connection preview if any
            if (isConnecting && !string.IsNullOrEmpty(connectFromNodeId))
            {
                var from = graph.FindNode(connectFromNodeId);
                if (from != null)
                {
                    var start = from.rect.center;
                    var end = ScreenToCanvas(Event.current.mousePosition);
                    DrawBezier(start, end, Color.cyan, 3f);
                    Repaint();
                }
            }

            GUI.matrix = oldMatrix;
            GUI.EndGroup();

            // Draw inspector panel on right
            DrawInspectorPanel();

            // Consume repaint if required
            if (GUI.changed) Repaint();
        }

        // ------------------ 基本 UI 元素 ------------------

        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("New Graph", EditorStyles.toolbarButton)) CreateNewGraph();
            if (GUILayout.Button("Open Graph", EditorStyles.toolbarButton)) OpenGraphAsset();
            if (graph != null)
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton)) SaveGraph();
                if (GUILayout.Button("Ensure IDs", EditorStyles.toolbarButton)) { graph.EnsureIds(); EditorUtility.SetDirty(graph); SaveGraph(); }
            }
            GUILayout.FlexibleSpace();
            // zoom UI
            GUILayout.Label("Zoom");
            canvasZoom = GUILayout.HorizontalSlider(canvasZoom, MIN_ZOOM, MAX_ZOOM, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }

        void DrawNoGraph()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("No StateGraph selected", EditorStyles.boldLabel);
            if (GUILayout.Button("Create new StateGraph")) CreateNewGraph();
            if (GUILayout.Button("Open existing StateGraph")) OpenGraphAsset();
            GUILayout.EndVertical();
        }

        // ------------------ 画布绘制 ------------------

        void DrawGrid(Rect canvasRect)
        {
            // 简单背景网格，随缩放与平移移动
            int gridSpacing = Mathf.RoundToInt(20 * canvasZoom);
            Handles.BeginGUI();
            Color c = new Color(0.18f, 0.18f, 0.18f);
            Handles.color = c;
            for (int x = -2000; x < 2000; x += gridSpacing)
            {
                Handles.DrawLine(new Vector3(x + canvasScroll.x, -2000 + canvasScroll.y), new Vector3(x + canvasScroll.x, 2000 + canvasScroll.y));
            }
            for (int y = -2000; y < 2000; y += gridSpacing)
            {
                Handles.DrawLine(new Vector3(-2000 + canvasScroll.x, y + canvasScroll.y), new Vector3(2000 + canvasScroll.x, y + canvasScroll.y));
            }
            Handles.EndGUI();
        }

        void DrawNodes()
        {
            BeginWindows();
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];
                GUIStyle style = node == selectedNode ? nodeSelectedStyle : nodeStyle;
                node.rect = GUI.Window(i, node.rect, id => DrawNodeWindow(id, node), node.Title, style);
            }
            EndWindows();
        }

        void DrawNodeWindow(int id, StateGraph.StateNode node)
        {
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            node.Title = EditorGUILayout.TextField("Title", node.Title);
            if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(graph); }
            node.Comment = EditorGUILayout.TextField("Comment", node.Comment);
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("→", miniButton, GUILayout.Width(24))) { StartConnectFrom(node); }
            if (GUILayout.Button("Delete", miniButton, GUILayout.Width(60))) { DeleteNode(node); GUIUtility.hotControl = 0; return; }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        void DrawEdges()
        {
            Handles.BeginGUI();
            for (int i = 0; i < graph.edges.Count; i++)
            {
                var e = graph.edges[i];
                var a = graph.FindNode(e.FromNodeId);
                var b = graph.FindNode(e.ToNodeId);
                if (a == null || b == null) continue;

                var start = a.rect.center;
                var end = b.rect.center;
                Color lineColor = (e == selectedEdge) ? Color.cyan : Color.white;
                DrawBezier(start, end, lineColor, 3f);

                // label in middle
                Vector2 mid = (start + end) * 0.5f;
                var labelRect = new Rect(mid.x - 60, mid.y - 12, 120, 24);
                GUI.Label(labelRect, string.IsNullOrEmpty(e.Trigger) ? "<implicit>" : e.Trigger, edgeLabelStyle);
            }
            Handles.EndGUI();
        }

        void DrawBezier(Vector2 start, Vector2 end, Color color, float width)
        {
            Handles.DrawBezier(start, end, start + Vector2.right * 50, end + Vector2.left * 50, color, null, width);
        }

        // ------------------ 输入处理 ------------------

        void ProcessInput(Event e)
        {
            // map mouse position to canvas coords for hit detection
            var canvasMouse = ScreenToCanvas(e.mousePosition);

            // Left mouse down: selection or begin drag
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // check nodes from top to bottom (reverse order)
                StateGraph.StateNode clicked = null;
                for (int i = graph.nodes.Count - 1; i >= 0; i--)
                {
                    if (graph.nodes[i].rect.Contains(canvasMouse)) { clicked = graph.nodes[i]; break; }
                }

                if (clicked != null)
                {
                    selectedNode = clicked;
                    selectedEdge = null;
                    isDraggingNode = true;
                    dragStartMouse = canvasMouse;
                    nodeDragStartRect = clicked.rect;
                    GUI.FocusControl(null);
                    e.Use();
                }
                else
                {
                    // click empty canvas: deselect
                    selectedNode = null;
                    selectedEdge = null;
                    GUI.FocusControl(null);
                }
            }

            // Node drag
            if (e.type == EventType.MouseDrag && isDraggingNode && e.button == 0 && selectedNode != null)
            {
                var delta = canvasMouse - dragStartMouse;
                selectedNode.rect.position = nodeDragStartRect.position + delta;
                EditorUtility.SetDirty(graph);
                e.Use();
            }

            // Left mouse up end drag
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDraggingNode = false;
                // if we were connecting, check release on node to finalize connection
                if (isConnecting && !string.IsNullOrEmpty(connectFromNodeId))
                {
                    // attempt to connect to a node under mouse
                    StateGraph.StateNode target = null;
                    foreach (var n in graph.nodes)
                    {
                        if (n.rect.Contains(canvasMouse)) { target = n; break; }
                    }
                    if (target != null)
                    {
                        // create edge
                        var edge = new StateGraph.StateEdge { Id = Guid.NewGuid().ToString(), FromNodeId = connectFromNodeId, ToNodeId = target.Id, Trigger = "" };
                        graph.edges.Add(edge);
                        EditorUtility.SetDirty(graph);
                    }
                    isConnecting = false;
                    connectFromNodeId = null;
                    e.Use();
                }
            }

            // Right mouse down: context menu
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                ShowContextMenu(e.mousePosition);
                e.Use();
            }

            // Middle mouse or Alt+drag: pan
            if ((e.button == 2 && (e.type == EventType.MouseDrag)) || (e.alt && e.type == EventType.MouseDrag))
            {
                if (e.type == EventType.MouseDown) { isPanning = true; panStartMouse = e.mousePosition; panStartOffset = canvasScroll; }
                canvasScroll += e.delta;
                foreach (var n in graph.nodes) n.rect.position += e.delta; // simple implement: move nodes (alternatively offset view)
                EditorUtility.SetDirty(graph);
                e.Use();
            }

            if (e.type == EventType.ScrollWheel)
            {
                // zoom centered at mouse
                var oldZoom = canvasZoom;
                float zoomDelta = -e.delta.y * 0.01f;
                canvasZoom = Mathf.Clamp(canvasZoom + zoomDelta, MIN_ZOOM, MAX_ZOOM);
                e.Use();
            }

            // Delete key remove selected node/edge
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
            {
                if (selectedNode != null) { DeleteNode(selectedNode); selectedNode = null; selectedEdge = null; e.Use(); }
                else if (selectedEdge != null) { graph.edges.Remove(selectedEdge); selectedEdge = null; EditorUtility.SetDirty(graph); e.Use(); }
            }
        }

        void ProcessNoGraphShortcuts()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.N) CreateNewGraph();
        }

        // ------------------ 操作函数 ------------------

        void ShowContextMenu(Vector2 mousePos)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add State"), false, () =>
            {
                var canvasPos = ScreenToCanvas(mousePos);
                AddNodeAt(canvasPos);
            });
            menu.AddItem(new GUIContent("Add Transition (drag from node arrow)"), false, () => { /* hint only */ });
            menu.ShowAsContext();
        }

        void CreateNewGraph()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create StateGraph", "StateGraph", "asset", "create state graph");
            if (string.IsNullOrEmpty(path)) return;
            var g = CreateInstance<StateGraph>();
            g.name = System.IO.Path.GetFileNameWithoutExtension(path);
            g.EnsureIds();
            AssetDatabase.CreateAsset(g, path);
            AssetDatabase.SaveAssets();
            graph = g;
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = graph;
        }

        void OpenGraphAsset()
        {
            string path = EditorUtility.OpenFilePanel("Open StateGraph", Application.dataPath, "asset");
            if (string.IsNullOrEmpty(path)) return;
            path = "Assets" + path.Substring(Application.dataPath.Length);
            graph = AssetDatabase.LoadAssetAtPath<StateGraph>(path);
            if (graph != null) Selection.activeObject = graph;
        }

        void SaveGraph()
        {
            if (graph == null) return;
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
        }

        void AddNodeAt(Vector2 canvasPos)
        {
            var n = new StateGraph.StateNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = "State",
                rect = new Rect(canvasPos.x - 80, canvasPos.y - 32, 160, 64)
            };
            graph.nodes.Add(n);
            EditorUtility.SetDirty(graph);
        }

        void DeleteNode(StateGraph.StateNode node)
        {
            if (node == null) return;
            // remove edges referencing node
            graph.edges.RemoveAll(e => e.FromNodeId == node.Id || e.ToNodeId == node.Id);
            graph.nodes.Remove(node);
            EditorUtility.SetDirty(graph);
        }

        void StartConnectFrom(StateGraph.StateNode from)
        {
            isConnecting = true;
            connectFromNodeId = from.Id;
        }

        // ------------------ 右侧属性面板 ------------------

        void DrawInspectorPanel()
        {
            float panelWidth = 300;
            Rect r = new Rect(position.width - panelWidth, 20, panelWidth, position.height - 20);
            GUILayout.BeginArea(r, GUI.skin.box);

            GUILayout.Label("Inspector", EditorStyles.boldLabel);
            if (graph == null)
            {
                GUILayout.EndArea(); return;
            }

            // Graph-level
            GUILayout.Label("Graph", EditorStyles.miniBoldLabel);
            graph.initialStateNodeId = EditorGUILayout.TextField("Initial Node Id", graph.initialStateNodeId);

            GUILayout.Space(8);
            if (selectedNode != null)
            {
                GUILayout.Label("Node", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                selectedNode.Title = EditorGUILayout.TextField("Title", selectedNode.Title);
                selectedNode.rect.position = EditorGUILayout.Vector2Field("Position", selectedNode.rect.position);
                selectedNode.Comment = EditorGUILayout.TextField("Comment", selectedNode.Comment);
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(graph);
                if (GUILayout.Button("Focus Node")) FocusOnNode(selectedNode);
            }
            else if (selectedEdge != null)
            {
                GUILayout.Label("Edge", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                selectedEdge.Trigger = EditorGUILayout.TextField("Trigger", selectedEdge.Trigger);
                selectedEdge.Comment = EditorGUILayout.TextField("Comment", selectedEdge.Comment);
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(graph);
                if (GUILayout.Button("Select From Node")) SelectNodeById(selectedEdge.FromNodeId);
                if (GUILayout.Button("Select To Node")) SelectNodeById(selectedEdge.ToNodeId);
            }
            else
            {
                GUILayout.Label("No selection");
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Ensure IDs")) { graph.EnsureIds(); EditorUtility.SetDirty(graph); }
            if (GUILayout.Button("Save")) SaveGraph();
            GUILayout.EndArea();
        }

        void FocusOnNode(StateGraph.StateNode n)
        {
            // center canvas (simple implementation: move all nodes so target in center)
            Vector2 center = new Vector2(position.width / 2f - 150, position.height / 2f - 60);
            Vector2 delta = center - n.rect.position;
            foreach (var node in graph.nodes) node.rect.position += delta;
            EditorUtility.SetDirty(graph);
        }

        void SelectNodeById(string id)
        {
            selectedNode = graph.FindNode(id);
            selectedEdge = null;
        }

        // ------------------ 工具函数 ------------------

        // 把屏幕坐标(窗口内)映射到画布坐标（考虑缩放和平移）
        Vector2 ScreenToCanvas(Vector2 screenPos)
        {
            // group area offset (20 px toolbar)
            var local = screenPos - new Vector2(0, 20);
            // apply inverse zoom transform; we used GUI.matrix trick, but for simplicity treat nodes positions as screen positions
            // In this editor implementation node.rect is maintained in screen coords, so just return local
            return local;
        }
    }
}