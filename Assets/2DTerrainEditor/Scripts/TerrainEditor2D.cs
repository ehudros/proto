using UnityEditor;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class TerrainEditor2D : MonoBehaviour
{
    public int MeshId;

    // --- Terrain size and quality
    public int Width = 50;
    public int Height = 50;
    public int Resolution = 2;
    
    // --- Main terrain settings
    public Material MainMaterial;
    public int TextureSize = 15;
    public bool FixSides;
    public float LeftFixedPoint = 25;
    public float RightFixedPoint = 25;
    
    // --- Rnd values for Randomizer
    public float RndAmplitude = 5;
    public int RndHillsCount = 5;
    public float RndHeight = 25;

    // --- Cap settings
    public bool CreateCap;
    public Material CapMaterial;
    public int CapTextureTiling = 50;
    public float CapHeight = 1;
    public float CapOffset;

    public GameObject CapObj;

    public void CreateTerrain() //Create new terrain
    {
        Mesh pathMesh;
        if (gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            pathMesh = new Mesh();
            pathMesh.name = "Terrain2D_mesh_" + MeshId;
        }
        else
        {
            pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            pathMesh.Clear();
        }

        Vector3[] verticles = new Vector3[((Width * 2) * Resolution) + 2];

        for (int i = 0; i < verticles.Length; i += 2) //Generate mesh verts
        {
            float vertsInterval = (i * 0.5f) / Resolution;

            verticles[i] = new Vector3(vertsInterval, Height * 0.5f, 0);
            verticles[i + 1] = new Vector3(vertsInterval, 0, 0);
        }
        if (FixSides)
        {
            verticles[0].y = LeftFixedPoint;
            verticles[verticles.Length - 2].y = RightFixedPoint;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion
        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;

        UpdateCollider();

        if (CreateCap)
            GenerateCap(GetVertsPos());
        else
        {
            if (CapObj != null)
                DestroyImmediate(CapObj);
        }
    }

    public void GenerateCap(Vector3[] vertsPos) //Create cap of this terrain
    {
        if (CapObj == null)
        {
            CapObj = new GameObject("Terrain2D Cap");
            CapObj.transform.position = transform.position;
            CapObj.transform.parent = transform;
            CapObj.AddComponent<MeshFilter>();
            CapObj.AddComponent<MeshRenderer>();
        }

        Mesh pathMesh;
        if (CapObj.GetComponent<MeshFilter>().sharedMesh == null)
        {
            pathMesh = new Mesh();
            pathMesh.name = "Terrain2D_cap_mesh_" + MeshId;
        }
        else
        {
            pathMesh = CapObj.GetComponent<MeshFilter>().sharedMesh;
            pathMesh.Clear();
        }

        Vector3[] verticles = vertsPos;

        for (int i = 0; i < verticles.Length; i += 2) //Generate mesh verts
        {
            verticles[i] = new Vector3(verticles[i].x, verticles[i].y, -0.01f);
            verticles[i + 1] = new Vector3(verticles[i + 1].x, verticles[i].y - CapHeight, -0.01f);
            verticles[i].y += CapOffset;
            verticles[i + 1].y += CapOffset;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * CapTextureTiling, 1);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * CapTextureTiling, 0);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion
        CapObj.GetComponent<MeshFilter>().mesh = pathMesh;
        CapObj.renderer.material = CapMaterial;

    }

    public void UpdateCollider() //Generate new path for Edge Collider based on positions of top verticles
    {
        Vector3[] verticles = GetVertsPos();
        Vector2[] points = new Vector2[verticles.Length / 2];

        int point = 0;
        for (int i = 0; i < verticles.Length; i += 2)
        {
            points[point] = new Vector2(verticles[i].x, verticles[i].y);
            point++;
        }
        gameObject.GetComponent<EdgeCollider2D>().points = points;
    }

    public void EditMesh(Vector3[] newVertsPos)
    {
        //DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);

        Mesh pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticles = newVertsPos;

        if (FixSides)
        {
            verticles[0].y = LeftFixedPoint;
            verticles[verticles.Length - 2].y = RightFixedPoint;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;

        if (CreateCap)
            GenerateCap(GetVertsPos());
        else
        {
            if (CapObj != null)
                DestroyImmediate(CapObj);
        }
    } //Edit mesh using new verticles array

    public void RandomizeTerrain()//Generate terrain based on Rnd values
    {
        Mesh pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticles = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;

        int v = (Width * Resolution) / RndHillsCount;
        int step = 0;

        float a = RandomizeVertexPoint();

        if (FixSides) 
            verticles[0].y = LeftFixedPoint;
        else
        {
            verticles[0].y = a;
            a = RandomizeVertexPoint();
        }
        
        for (int i = 2; i < verticles.Length; i += 2) //Generate mesh verts
        {
            if (step >= v)
            {
                a = RandomizeVertexPoint();
                step = 0;
            }

            verticles[i].y = Mathf.Lerp(verticles[i - 2].y, a, ((float)step / v) / Resolution);

            step++;

        }

        if (FixSides)
            RightFixedPoint = verticles[verticles.Length - 2].y;
        
        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;
        gameObject.GetComponent<MeshRenderer>().material = MainMaterial;

        if (CreateCap)
            GenerateCap(GetVertsPos());
        else
        {
            if (CapObj != null)
                DestroyImmediate(CapObj);
        }
    }

    public void RandomizeTerrain(float lastVertexPoint) //Generate terrain based on Rnd values and last vertex point
    {
        Mesh pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticles = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;

        int v = (Width * Resolution) / RndHillsCount;
        int step = 0;

        float a = RandomizeVertexPoint();
        verticles[0].y = lastVertexPoint;

        for (int i = 2; i < verticles.Length; i += 2) //Generate mesh verts
        {
            if (step >= v)
            {
                a = RandomizeVertexPoint();
                step = 0;
            }

            if (!FixSides)
                verticles[i].y = Mathf.Lerp(verticles[i - 2].y, a, ((float)step / v) / Resolution);
            else
            {
                verticles[i].y = Mathf.Lerp(verticles[i - 2].y, a, ((float)step / v) / Resolution);
            }

            step++;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;
        gameObject.GetComponent<MeshRenderer>().material = MainMaterial;

        if (CreateCap)
            GenerateCap(GetVertsPos());
        else
        {
            if (CapObj != null)
                DestroyImmediate(CapObj);
        }
    }

    public float GetLastVertexPoint() //Get last verticle point of this mesh for use it in next random generated terrain
    {
        return gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[gameObject.GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2].y;
    }

    public Vector3[] GetVertsPos() //Get verticles array of this mesh
    {
        if (gameObject.GetComponent<MeshFilter>().sharedMesh != null)
            return gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
        return null;
    }

    public bool IsPlayMode()
    {
        return Application.isPlaying;
    }

    float RandomizeVertexPoint() //Set random verticle point based on randomizer values
    {
        float a = RndHeight - RndAmplitude;
        float b = RndHeight + RndAmplitude;

        if (a < 0.1f)
            a = 0.1f;
        if (b > Height)
            b = Height;

        return Random.Range(a, b);
    }

    public static GameObject InstantiateTerrain2D(Vector3 position)
    {
        GameObject newTerrain = new GameObject("New Terrain2D");
        newTerrain.transform.position = position;
        newTerrain.AddComponent<MeshFilter>();
        newTerrain.AddComponent<MeshRenderer>();
        newTerrain.AddComponent<EdgeCollider2D>();
        newTerrain.AddComponent<TerrainEditor2D>();
        
        newTerrain.GetComponent<TerrainEditor2D>().CreateTerrain();

        return newTerrain;
    }
}


