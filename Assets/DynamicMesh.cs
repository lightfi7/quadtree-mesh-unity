#region _
/*using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


public class DynamicMesh : MonoBehaviour
{
    public static DynamicMesh instance;
    public int maxDepth = 5;
    public float minSize = 1f;
    public Camera mainCamera;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Mesh mesh;
    private MeshFilter meshFilter;
    public QuadtreeNode rootNode;
    private bool meshNeedsUpdate = false;
    private List<QuadtreeNode> visibleChildren = new List<QuadtreeNode>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        Vector2 mapSize = new Vector2(1000f, 1000f);
        rootNode = new QuadtreeNode(Vector2.zero, mapSize, 0, 0, 1);
    }


    private void OnDrawGizmos()
    {
        if (rootNode != null)
        {
            DrawGizmos(rootNode);
        }
    }

    void DrawGizmos(QuadtreeNode node)
    {
        foreach (var child in node.children)
        {
            DrawGizmos(child);
        }
        Gizmos.color = Color.red;
        int n = node.neighbours[0] + node.neighbours[1] * 2 + node.neighbours[2] * 4 + node.neighbours[3] * 8;
        for (int i = 0; i < n; i++)
            Gizmos.DrawSphere(new Vector3(node.center.x + (int)i / 4, node.center.y + i % 4, 0), 1f);
    }

    void Update()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        UpdateQuadtree(rootNode, cameraPosition);

        GetNeighBours(rootNode);

        if (meshNeedsUpdate)
        {
            GenerateMesh();
            meshNeedsUpdate = false;
        }

        
    }

    void GetNeighBours(QuadtreeNode node)
    {
        node.GetNeighbours();
        foreach (var child in node.children)
        {
            GetNeighBours(child);
        }
    }


    void UpdateQuadtree(QuadtreeNode node, Vector3 cameraPosition)
    {

        if (node.depth >= maxDepth || node.size.x <= minSize || node.size.y <= minSize)
        {
            return;
        }


        float distance = Vector2.Distance(node.center, new Vector2(cameraPosition.x, cameraPosition.y));
        float threshold = node.size.magnitude * .75f;

        if (distance < threshold)
        {
            if (node.children.Length == 0)
            {
                node.Split();
                meshNeedsUpdate = true;
            }

            foreach (QuadtreeNode child in node.children)
            {
                UpdateQuadtree(child, cameraPosition);
            }
        }
        else
        {
            if (node.children.Length > 0)
            {
                node.ClearChildren();
                meshNeedsUpdate = true;
            }
        }

    }


    void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        GenerateNodeMesh(rootNode, vertices, triangles);

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void GenerateNodeMesh(QuadtreeNode node, List<Vector3> vertices, List<int> triangles)
    {
        if (node.children.Length != 0)
        {
            foreach (QuadtreeNode child in node.children)
            {
                
                GenerateNodeMesh(child, vertices, triangles);

            }
        }
        else
        {
            int baseIndex = vertices.Count;
            int quadIndex = node.neighbours[0] + node.neighbours[1] * 2 + node.neighbours[2] * 4 + node.neighbours[3] * 8;

            Vector3[] quadVertices = Presets.quadTemplateVertices[quadIndex];

            for (int i = 0; i < quadVertices.Length; i++)
            {
                Vector3 vertex = new Vector3(quadVertices[i].x, quadVertices[i].y, 0) * (node.size.x / 2) + new Vector3(node.center.x, node.center.y, 0);
               *//* int x = (int)((vertex.x + 1800 * 31f) / 31f) % 3600;
                int y = (int)((vertex.z + 1800 * 31f) / 31f) % 3600;
                float height = (float)DSMLoader.instance.dSMs[x * 3600 + y].height * 2f;
                vertex.y = height;*//*
                vertices.Add(vertex);
            }

            int[] quadTriangles = Presets.quadTemplateTriangles[quadIndex];
            for (int i = 0; i < quadTriangles.Length; i++)
            {
                triangles.Add(quadTriangles[i] + baseIndex);
            }
        }
    }
}

public class QuadtreeNode
{
    public Vector2 center;
    public Vector2 size;
    public int depth;
    public int corner;
    public QuadtreeNode[] children;
    public byte[] neighbours;
    public uint hash = 0;

    public QuadtreeNode(Vector2 center, Vector2 size, int depth, int corner, uint hash)
    {
        this.center = center;
        this.size = size;
        this.depth = depth;
        this.corner = corner;
        this.neighbours = new byte[4];
        this.children = new QuadtreeNode[0];
        this.hash = hash;
    }

    public void Split()
    {
        Vector2 halfSize = size * 0.5f;
        Vector2 quarterSize = size * 0.25f;
        children = new QuadtreeNode[4];
        children[1] = new QuadtreeNode(center + new Vector2(-quarterSize.x, quarterSize.y), halfSize, depth + 1, 1, hash * 4 + 1);
        children[0] = new QuadtreeNode(center + new Vector2(quarterSize.x, quarterSize.y), halfSize, depth + 1, 0, hash * 4 + 0);
        children[3] = new QuadtreeNode(center + new Vector2(quarterSize.x, -quarterSize.y), halfSize, depth + 1, 3, hash * 4 + 3);
        children[2] = new QuadtreeNode(center + new Vector2(-quarterSize.x, -quarterSize.y), halfSize, depth + 1, 2, hash * 4 + 2);
    }

    public void ClearChildren()
    {
        if (children.Length > 0)
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = null;
            }
            children = new QuadtreeNode[0];
        }
    }

  
    public void GetNeighbours()
    {
        byte[] _neighbours = new byte[4];
        if (corner == 0) // Top left
        {
            _neighbours[1] = CheckNeighbour(1, hash); // West
            _neighbours[2] = CheckNeighbour(2, hash); // North
        }
        else if (corner == 1) // Top right
        {
            _neighbours[0] = CheckNeighbour(0, hash); // East
            _neighbours[2] = CheckNeighbour(2, hash); // North
        }
        else if (corner == 2) // Bottom right
        {
            _neighbours[0] = CheckNeighbour(0, hash); // East
            _neighbours[3] = CheckNeighbour(3, hash); // South
        }
        else if (corner == 3) // Bottom left
        {
            _neighbours[1] = CheckNeighbour(1, hash); // West
            _neighbours[3] = CheckNeighbour(3, hash); // South
        }
        neighbours = _neighbours;
    }

    public byte CheckNeighbour(int side, uint hash_)
    {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < depth * 2)
        {
            count += 2;
            twoLast = hash_ & 3;
            bitmask = bitmask * 4;

            if (side == 2 || side == 3)
            {
                bitmask += 3;
            }
            else
            {
                bitmask += 1;
            }

            if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
                (side == 1 && (twoLast == 1 || twoLast == 2)) ||
                (side == 2 && (twoLast == 3 || twoLast == 2)) ||
                (side == 3 && (twoLast == 0 || twoLast == 1)))
            {
                break;
            }

            hash_ >>= 2;
        }

        int neighbourDepth = DynamicMesh.instance.rootNode.GetNeighbourDepth(hash ^ bitmask, depth);
        
        return neighbourDepth < depth ? (byte)1 : (byte)0;
    }

    public int GetNeighbourDepth(uint queryHash, int dl)
    {
        int dlResult = 0;

        if (hash == queryHash)
        {
            dlResult = depth;
        }
        else
        {
            if (children.Length > 0)
            {
                dlResult += children[((queryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDepth(queryHash, dl - 1);
            }
        }
        return dlResult; 
    }
}
*/

// using UnityEngine;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine.UIElements;

// public class DynamicMesh : MonoBehaviour
// {
//     public static DynamicMesh instance;
//     public int maxDepth = 5;
//     public float minSize = 1f;
//     public Camera mainCamera;
//     private List<Vector3> vertices = new List<Vector3>();
//     private List<int> triangles = new List<int>();
//     private List<Vector3> normals = new List<Vector3>();
//     private Mesh mesh;
//     private MeshFilter meshFilter;
//     public QuadtreeNode rootNode;
//     private bool meshNeedsUpdate = false;
//     private List<QuadtreeNode> visibleChildren = new List<QuadtreeNode>();

//     private void Awake()
//     {
//         instance = this;
//     }

//     void Start()
//     {
//         mesh = new Mesh();
//         meshFilter = GetComponent<MeshFilter>();
//         meshFilter.mesh = mesh;
//         mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//         Vector2 mapSize = new Vector2(3600 * 31f, 3600 * 31f);
//         rootNode = new QuadtreeNode(Vector2.zero, mapSize, 0, 0, 1);
//     }

//     private void OnDrawGizmos()
//     {
//         if (rootNode != null)
//         {
//             DrawGizmos(rootNode);
//         }
//     }

//     void DrawGizmos(QuadtreeNode node)
//     {
//         foreach (var child in node.children)
//         {
//             DrawGizmos(child);
//         }
//         Gizmos.color = Color.red;
//         int n = node.neighbours[0] + node.neighbours[1] * 2 + node.neighbours[2] * 4 + node.neighbours[3] * 8;
//         for (int i = 0; i < n; i++)
//             Gizmos.DrawSphere(new Vector3(node.center.x + (int)i / 4, 0, node.center.y + i % 4), 1f);
//     }

//     void Update()
//     {
//         Vector3 cameraPosition = mainCamera.transform.position;
//         UpdateQuadtree(rootNode, cameraPosition);
//         GetNeighBours(rootNode);

//         if (meshNeedsUpdate)
//         {
//             GenerateMesh();
//             meshNeedsUpdate = false;
//         }
//     }

//     void GetNeighBours(QuadtreeNode node)
//     {
//         node.GetNeighbours();
//         foreach (var child in node.children)
//         {
//             GetNeighBours(child);
//         }
//     }

//     void UpdateQuadtree(QuadtreeNode node, Vector3 cameraPosition)
//     {
//         if (node.depth >= maxDepth || node.size.x <= minSize || node.size.y <= minSize)
//         {
//             return;
//         }

//         float distance = Vector2.Distance(node.center, new Vector2(cameraPosition.x, cameraPosition.z));
//         float threshold = node.size.magnitude * 1.6f;

//         if (distance < threshold)
//         {
//             if (node.children.Length == 0)
//             {
//                 node.Split();
//                 meshNeedsUpdate = true;
//             }
//             else
//             {

//                 foreach (QuadtreeNode child in node.children)
//                 {
//                     UpdateQuadtree(child, cameraPosition);
//                 }
//             }
//         }
//         else
//         {
//             if (node.children.Length > 0)
//             {
//                 node.ClearChildren();
//                 meshNeedsUpdate = true;
//             }
//         }
//     }

//     void GenerateMesh()
//     {
//         vertices.Clear();
//         triangles.Clear();
//         normals.Clear();

//         GenerateNodeMesh(rootNode, vertices, triangles, normals);
//         CalculateSmoothNormals(vertices, triangles, normals);

//         mesh.Clear();
//         mesh.vertices = vertices.ToArray();
//         mesh.triangles = triangles.ToArray();
//         mesh.normals = normals.ToArray();
//         mesh.RecalculateNormals();

//     }

//     void GenerateNodeMesh(QuadtreeNode node, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
//     {
//         if (node.children.Length != 0)
//         {
//             foreach (QuadtreeNode child in node.children)
//             {
//                 GenerateNodeMesh(child, vertices, triangles, normals);
//             }
//         }
//         else
//         {
//             int baseIndex = vertices.Count;
//             int quadIndex = node.neighbours[0] + node.neighbours[1] * 2 + node.neighbours[2] * 4 + node.neighbours[3] * 8;

//             Vector3[] quadVertices = Presets.quadTemplateVertices[quadIndex];

//             for (int i = 0; i < quadVertices.Length; i++)
//             {
//                 Vector3 vertex = new Vector3(quadVertices[i].x, 0, quadVertices[i].y) * (node.size.x / 2) + new Vector3(node.center.x, 0, node.center.y);
//                 int x = (int)((vertex.x + 1800 * 31f) / 31f) % 3600;
//                 int y = (int)((vertex.z + 1800 * 31f) / 31f) % 3600;
//                 float height = (float)DSMLoader.instance.dSMs[x * 3600 + y].height * 1f;
//                 vertex.y = height;
//                 vertices.Add(vertex);
//                 normals.Add(Vector3.up); // Temporary normal
//             }

//             int[] quadTriangles = Presets.quadTemplateTriangles[quadIndex];
//             for (int i = 0; i < quadTriangles.Length; i++)
//             {
//                 triangles.Add(quadTriangles[i] + baseIndex);
//             }
//         }
//     }

//     void GenerateNodeMesh(QuadtreeNode node, List<Vector3> vertices, List<int> triangles)
//     {
//         if (node.children.Length != 0)
//         {
//             foreach (QuadtreeNode child in node.children)
//             {
//                 GenerateNodeMesh(child, vertices, triangles);
//             }
//         }
//         else
//         {
//             int baseIndex = vertices.Count;
//             int quadIndex = node.neighbours[0] + node.neighbours[1] * 2 + node.neighbours[2] * 4 + node.neighbours[3] * 8;

//             Vector3[] quadVertices = Presets.quadTemplateVertices[quadIndex];

//             for (int i = 0; i < quadVertices.Length; i++)
//             {
//                 Vector3 vertex = new Vector3(quadVertices[i].x, 0, quadVertices[i].y) * (node.size.x / 2) + new Vector3(node.center.x, 0, node.center.y);
//                 int x = (int)((vertex.x + 1800 * 31f) / 31f) % 3600;
//                 int y = (int)((vertex.z + 1800 * 31f) / 31f) % 3600;
//                 float height = (float)DSMLoader.instance.dSMs[x * 3600 + y].height * 1f;
//                 vertex.y = height;
//                 vertices.Add(vertex);
//             }

//             int[] quadTriangles = Presets.quadTemplateTriangles[quadIndex];
//             for (int i = 0; i < quadTriangles.Length; i++)
//             {
//                 triangles.Add(quadTriangles[i] + baseIndex);
//             }
//         }
//     }

//     void CalculateSmoothNormals(List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
//     {
//         Dictionary<Vector3, List<Vector3>> vertexToNormals = new Dictionary<Vector3, List<Vector3>>();

//         for (int i = 0; i < triangles.Count; i += 3)
//         {
//             Vector3 v0 = vertices[triangles[i]];
//             Vector3 v1 = vertices[triangles[i + 1]];
//             Vector3 v2 = vertices[triangles[i + 2]];

//             Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

//             if (!vertexToNormals.ContainsKey(v0))
//                 vertexToNormals[v0] = new List<Vector3>();
//             if (!vertexToNormals.ContainsKey(v1))
//                 vertexToNormals[v1] = new List<Vector3>();
//             if (!vertexToNormals.ContainsKey(v2))
//                 vertexToNormals[v2] = new List<Vector3>();

//             vertexToNormals[v0].Add(normal);
//             vertexToNormals[v1].Add(normal);
//             vertexToNormals[v2].Add(normal);
//         }

//         for (int i = 0; i < vertices.Count; i++)
//         {
//             Vector3 vertex = vertices[i];
//             Vector3 smoothNormal = Vector3.zero;

//             if (vertexToNormals.ContainsKey(vertex))
//             {
//                 foreach (Vector3 normal in vertexToNormals[vertex])
//                 {
//                     smoothNormal += normal;
//                 }
//                 smoothNormal.Normalize();
//             }

//             normals[i] = smoothNormal;
//         }
//     }

//     }

// public class QuadtreeNode
// {
//     public Vector2 center;
//     public Vector2 size;
//     public int depth;
//     public int corner;
//     public QuadtreeNode[] children;
//     public byte[] neighbours;
//     public uint hash = 0;

//     public QuadtreeNode(Vector2 center, Vector2 size, int depth, int corner, uint hash)
//     {
//         this.center = center;
//         this.size = size;
//         this.depth = depth;
//         this.corner = corner;
//         this.neighbours = new byte[4];
//         this.children = new QuadtreeNode[0];
//         this.hash = hash;
//     }

//     public void Split()
//     {
//         Vector2 halfSize = size * 0.5f;
//         Vector2 quarterSize = size * 0.25f;
//         children = new QuadtreeNode[4];
//         children[1] = new QuadtreeNode(center + new Vector2(-quarterSize.x, quarterSize.y), halfSize, depth + 1, 1, hash * 4 + 1);
//         children[0] = new QuadtreeNode(center + new Vector2(quarterSize.x, quarterSize.y), halfSize, depth + 1, 0, hash * 4 + 0);
//         children[3] = new QuadtreeNode(center + new Vector2(quarterSize.x, -quarterSize.y), halfSize, depth + 1, 3, hash * 4 + 3);
//         children[2] = new QuadtreeNode(center + new Vector2(-quarterSize.x, -quarterSize.y), halfSize, depth + 1, 2, hash * 4 + 2);
//     }

//     public void ClearChildren()
//     {
//         if (children.Length > 0)
//         {
//             for (int i = 0; i < children.Length; i++)
//             {
//                 children[i] = null;
//             }
//             children = new QuadtreeNode[0];
//         }
//     }

//     public void GetNeighbours()
//     {
//         byte[] _neighbours = new byte[4];
//         if (corner == 0) // Top left
//         {
//             _neighbours[1] = CheckNeighbour(1, hash); // West
//             _neighbours[2] = CheckNeighbour(2, hash); // North
//         }
//         else if (corner == 1) // Top right
//         {
//             _neighbours[0] = CheckNeighbour(0, hash); // East
//             _neighbours[2] = CheckNeighbour(2, hash); // North
//         }
//         else if (corner == 2) // Bottom right
//         {
//             _neighbours[0] = CheckNeighbour(0, hash); // East
//             _neighbours[3] = CheckNeighbour(3, hash); // South
//         }
//         else if (corner == 3) // Bottom left
//         {
//             _neighbours[1] = CheckNeighbour(1, hash); // West
//             _neighbours[3] = CheckNeighbour(3, hash); // South
//         }
//         neighbours = _neighbours;
//     }

//     public byte CheckNeighbour(int side, uint hash_)
//     {
//         uint bitmask = 0;
//         byte count = 0;
//         uint twoLast;

//         while (count < depth * 2)
//         {
//             count += 2;
//             twoLast = hash_ & 3;
//             bitmask = bitmask * 4;

//             if (side == 2 || side == 3)
//             {
//                 bitmask += 3;
//             }
//             else
//             {
//                 bitmask += 1;
//             }

//             if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
//                 (side == 1 && (twoLast == 1 || twoLast == 2)) ||
//                 (side == 2 && (twoLast == 3 || twoLast == 2)) ||
//                 (side == 3 && (twoLast == 0 || twoLast == 1)))
//             {
//                 break;
//             }

//             hash_ >>= 2;
//         }

//         int neighbourDepth = DynamicMesh.instance.rootNode.GetNeighbourDepth(hash ^ bitmask, depth);

//         if (neighbourDepth < depth - 1)
//         {
//             SplitNeighbour(hash ^ bitmask, neighbourDepth);
//             return 1;
//         }
//         else if (neighbourDepth > depth + 1)
//         {
//             SplitNeighbour(hash ^ bitmask, neighbourDepth);
//             return 1;
//         }
//         else if (neighbourDepth < depth)
//         {
//             return 1;
//         }
//         else
//         {
//             return 0;
//         }
//     }

//     private void SplitNeighbour(uint neighbourHash, int neighbourDepth)
//     {
//         QuadtreeNode neighbourNode = DynamicMesh.instance.rootNode.GetNodeByHash(neighbourHash, neighbourDepth);
//         if (neighbourNode != null && neighbourNode.depth == neighbourDepth)
//         {
//             neighbourNode.Split();
//         }
//     }

//     public QuadtreeNode GetNodeByHash(uint queryHash, int queryDepth)
//     {
//         if (hash == queryHash)
//         {
//             return this;
//         }
//         else if (depth < queryDepth)
//         {
//             foreach (QuadtreeNode child in children)
//             {
//                 QuadtreeNode result = child.GetNodeByHash(queryHash, queryDepth);
//                 if (result != null)
//                 {
//                     return result;
//                 }
//             }
//         }
//         return null;
//     }

//     public int GetNeighbourDepth(uint queryHash, int dl)
//     {
//         int dlResult = 0;

//         if (hash == queryHash)
//         {
//             dlResult = depth;
//         }
//         else
//         {
//             if (children.Length > 0)
//             {
//                 dlResult += children[((queryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDepth(queryHash, dl - 1);
//             }
//         }
//         return dlResult;
//     }
// }
#endregion

using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.IO;

public class DynamicMesh : MonoBehaviour
{
    public float meterPerPixel = 0.1493f;
    public float meterPerDegree = 111321f;
    
    public static DynamicMesh instance;
    public int maxDepth = 8;
    public float minSize = 1f; // Each deep node is 38*256 meters
    public Camera mainCamera;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private Mesh mesh;
    private MeshFilter meshFilter;
    public QuadtreeNode rootNode;
    private bool meshNeedsUpdate = false;
    private List<QuadtreeNode> visibleChildren = new List<QuadtreeNode>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeQuadTree();
    }


    private void InitializeQuadTree()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Vector2 mapSize = new Vector2(0.1493f * 256 * Mathf.Pow(2, maxDepth), 0.1493f * 256 * Mathf.Pow(2, maxDepth));
        rootNode = new QuadtreeNode(Vector2.zero, mapSize, 0, 0, 1);
    }

    private void OnDrawGizmos()
    {
        if (rootNode == null)
        {
            InitializeQuadTree();
            return;
        }
        rootNode.OnDrawGizmo();
    }

    void Update()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        UpdateQuadtree(rootNode, cameraPosition);
        GetNeighBours(rootNode);

        if (meshNeedsUpdate)
        {
            GenerateMesh();
            meshNeedsUpdate = false;
        }
    }

    void GetNeighBours(QuadtreeNode node)
    {
            node.GetNeighbours();
            foreach (var child in node.children)
            {
                GetNeighBours(child);
            }
    }

    void UpdateQuadtree(QuadtreeNode node, Vector3 cameraPosition)
    {
        if (node.depth >= maxDepth || node.size.x <= minSize || node.size.y <= minSize)
        {
            return;
        }

        float distance = Vector3.Distance(new Vector3(node.center.x, 0, node.center.y), cameraPosition);
        float threshold = node.size.magnitude * 1.6f;

        if (distance < threshold)
        {
            if (node.children.Length == 0)
            {
                node.Split();
                meshNeedsUpdate = true;
            }
            else
            {
                foreach (QuadtreeNode child in node.children)
                {
                    UpdateQuadtree(child, cameraPosition);
                }
            }
        }
        else
        {
            if (node.children.Length > 0)
            {
                node.ClearChildren();
                meshNeedsUpdate = true;
            }
        }
    }

    void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        GenerateNodeMesh(rootNode, vertices, triangles, uvs);
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        // ApplyTexture();
    }

    void GenerateNodeMesh(QuadtreeNode node, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if (node.children.Length != 0)
        {
            foreach (QuadtreeNode child in node.children)
            {
                GenerateNodeMesh(child, vertices, triangles, uvs);
            }
        }
        else
        {
            int baseIndex = vertices.Count;
            int quadIndex = node.neighbours[0] + node.neighbours[1] * 2 + node.neighbours[2] * 4 + node.neighbours[3] * 8;

            Vector3[] quadVertices = Presets.quadTemplateVertices[quadIndex];

            for (int i = 0; i < quadVertices.Length; i++)
            {
                Vector3 vertex = new Vector3(quadVertices[i].x, 0, quadVertices[i].y) * (node.size.x / 2) + new Vector3(node.center.x, 0, node.center.y);
                vertices.Add(vertex);
                float x = vertex.x;
                float y = vertex.z;
                uvs.Add(new Vector2(x, y));
            }

            int[] quadTriangles = Presets.quadTemplateTriangles[quadIndex];
            for (int i = 0; i < quadTriangles.Length; i++)
            {
                triangles.Add(quadTriangles[i] + baseIndex);
            }

            // Load and apply texture
            ApplyTexture();
        }
    }


    void ApplyTexture()
    {
        string texturePath = $"D:\\20\\443429\\ges_443429_320328_20.jpg";
        if (File.Exists(texturePath))
        {
            byte[] fileData = File.ReadAllBytes(texturePath);
            Texture2D texture = new Texture2D(256, 256);
            texture.LoadImage(fileData);

            // Apply the texture to the mesh
            // Assuming you have a material with a shader that supports textures
            Material material = meshFilter.GetComponent<Renderer>().material;
            material.mainTexture = texture;
        }
    }

}

public class QuadtreeNode
{
    public int depth;
    public int corner;
    public uint hash = 0;
    public Vector2 size;
    public Vector2 center;
    public byte[] neighbours;
    public QuadtreeNode[] children;

    public QuadtreeNode(Vector2 center, Vector2 size, int depth, int corner, uint hash)
    {
        this.center = center;
        this.size = size;
        this.depth = depth;
        this.corner = corner;
        this.neighbours = new byte[4];
        this.children = new QuadtreeNode[0];
        this.hash = hash;
    }

    public void Split()
    {
        Vector2 halfSize = size * 0.5f;
        Vector2 quarterSize = size * 0.25f;
        children = new QuadtreeNode[4];
        children[1] = new QuadtreeNode(center + new Vector2(-quarterSize.x, quarterSize.y), halfSize, depth + 1, 1, hash * 4 + 1);
        children[2] = new QuadtreeNode(center + new Vector2(-quarterSize.x, -quarterSize.y), halfSize, depth + 1, 2, hash * 4 + 2);
        children[3] = new QuadtreeNode(center + new Vector2(quarterSize.x, -quarterSize.y), halfSize, depth + 1, 3, hash * 4 + 3);
        children[0] = new QuadtreeNode(center + new Vector2(quarterSize.x, quarterSize.y), halfSize, depth + 1, 0, hash * 4 + 0);
    }

    public void ClearChildren()
    {
        if (children.Length > 0)
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = null;
            }
            children = new QuadtreeNode[0];
        }
    }

    public void GetNeighbours()
    {
        byte[] _neighbours = new byte[4];
        if (corner == 0) // Top left
        {
            _neighbours[1] = CheckNeighbour(1, hash); // West
            _neighbours[2] = CheckNeighbour(2, hash); // North
        }
        else if (corner == 1) // Top right
        {
            _neighbours[0] = CheckNeighbour(0, hash); // East
            _neighbours[2] = CheckNeighbour(2, hash); // North
        }
        else if (corner == 2) // Bottom right
        {
            _neighbours[0] = CheckNeighbour(0, hash); // East
            _neighbours[3] = CheckNeighbour(3, hash); // South
        }
        else if (corner == 3) // Bottom left
        {
            _neighbours[1] = CheckNeighbour(1, hash); // West
            _neighbours[3] = CheckNeighbour(3, hash); // South
        }
        neighbours = _neighbours;
    }

    public byte CheckNeighbour(int side, uint hash_)
    {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < depth * 2)
        {
            count += 2;
            twoLast = hash_ & 3;
            bitmask = bitmask * 4;

            if (side == 2 || side == 3)
            {
                bitmask += 3;
            }
            else
            {
                bitmask += 1;
            }

            if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
                (side == 1 && (twoLast == 1 || twoLast == 2)) ||
                (side == 2 && (twoLast == 3 || twoLast == 2)) ||
                (side == 3 && (twoLast == 0 || twoLast == 1)))
            {
                break;
            }

            hash_ >>= 2;
        }

        int neighbourDepth = DynamicMesh.instance.rootNode.GetNeighbourDepth(hash ^ bitmask, depth);

        return neighbourDepth < depth ? (byte)1 : (byte)0;
    }

    public int GetNeighbourDepth(uint queryHash, int dl)
    {
        int dlResult = 0;

        if (hash == queryHash)
        {
            dlResult = depth;
        }
        else
        {
            if (children.Length > 0)
            {
                dlResult += children[((queryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDepth(queryHash, dl - 1);
            }
        }
        return dlResult;
    }

    public void OnDrawGizmo()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(center.x, 0, center.y), new Vector3(size.x, 0, size.y));

        foreach (var child in children)
        {
            child?.OnDrawGizmo();
        }
    }

}