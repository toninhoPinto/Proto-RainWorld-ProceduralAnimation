using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector3 position;
    public Vector3 velocity;
    public float weight;

    public Point(Vector3 pos, float w)
    {
        position = pos;
        velocity = Vector3.zero;
        weight = w;
    }

}

public class Tentacle : MonoBehaviour {

    public float maxDist = 0.4f;
    public int nPoints = 5;
    public bool alive = true;
    public bool visual = true;
    public float width = 0.1f;
    public Transform target;

    const float GRAVITY = 0.05f;

    List<Point> points;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    Vector3[] vertices;
    Vector2[] uvs;
    int[] tris;

    void Start () {
        points = new List<Point>();

        for(int i = 0; i < nPoints; i++)
        {
            float w = (i / (float)nPoints / 2) + .2f;
            points.Add(new Point(transform.position + transform.up * i * maxDist/2, w ));
        }

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter != null)
        {
            if (meshFilter.mesh != null)
                mesh = meshFilter.mesh;
            else
                mesh = new Mesh();

            meshFilter.mesh = mesh;

            mesh.Clear();
            int nVerts = (((nPoints - 2) * 2) + 1) * 3;
            Debug.Log(nVerts);
            vertices = new Vector3[nVerts];
            uvs = new Vector2[nVerts];
            tris = new int[nVerts];
        }
        if (visual)
            DrawVisuals();
    }

    void Update () {

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        //move the base of the tentacles
        points[0].position = transform.position;

        Vector3 nextDir;
        float nextDist;
        Vector3 prevDir;
        float prevDist;

        for (int i = 1; i < points.Count-1; i++)
        {
            //air friction
            points[i].velocity *= 0.98f;

            //gravity
            points[points.Count - 1].velocity.y -= GRAVITY;

            //elastic force
            //closer to the base
            prevDir = (points[i - 1].position - points[i].position).normalized;
            nextDist = Vector3.Distance(points[i].position, points[i - 1].position);

            points[i].velocity += prevDir * (nextDist - maxDist) * points[i].weight;

            //closer to the end
            nextDir = (points[i + 1].position - points[i].position).normalized;
            prevDist = Vector3.Distance(points[i].position, points[i + 1].position);

            points[i].velocity += nextDir * (prevDist - maxDist) * points[i].weight;

            points[i].position += points[i].velocity * Time.deltaTime;
        }

        //move end of tentacle towards mouse
        if (alive)
        {
            if (target == null)
            {
                nextDir = (mousePos - points[points.Count - 1].position).normalized;
                nextDist = Vector3.Distance(points[points.Count - 1].position, mousePos);
            }else
            {
                nextDir = (target.position - points[points.Count - 1].position).normalized;
                nextDist = Vector3.Distance(points[points.Count - 1].position, target.position);
            }
            points[points.Count - 1].velocity += nextDir * points[points.Count - 1].weight;
            Debug.DrawRay(points[points.Count - 1].position, nextDir * points[points.Count - 1].weight, Color.green);
        }

        //air friction
        points[points.Count - 1].velocity *= 0.98f;

        //gravity
        points[points.Count - 1].velocity.y -= GRAVITY;

        //elastic force
        prevDir = (points[points.Count - 2].position - points[points.Count - 1].position).normalized;
        prevDist = Vector3.Distance(points[points.Count - 1].position, points[points.Count - 2].position);

        points[points.Count - 1].velocity += prevDir * (prevDist - maxDist) * points[points.Count - 1].weight;
        Debug.DrawRay(points[points.Count - 1].position, prevDir * (prevDist - maxDist) * points[points.Count - 1].weight, Color.cyan);


        Debug.DrawRay(points[points.Count - 1].position, prevDir * maxDist, Color.blue);

        points[points.Count - 1].position += points[points.Count - 1].velocity * Time.deltaTime;

        if (meshFilter!=null)
            meshRenderer.enabled = visual;
        if (visual)
        {
            DrawVisuals();
        }
    }


    void DrawVisuals()
    {
        //Call this before assigning vertices to get better performance when continually updating the Mesh. 
        //Internally, this makes the Mesh use "dynamic buffers" in the underlying graphics API, which are more efficient when Mesh data changes often.
        mesh.MarkDynamic();

        Vector3 vert = Vector3.zero;
        Vector3 side;
        Vector3 side2;

        //nPoints-2
        for (int i = 0; i < nPoints - 2; i++)
        {
            int j = i + 1;
            int meshIndex;
            side = i == 0 ? transform.right : Vector3.Cross((points[i].position - points[i - 1].position).normalized, transform.forward);
            side2 = Vector3.Cross((points[j].position - points[j - 1].position).normalized, transform.forward);


            meshIndex = i * 6;
            vert = points[i].position + side * width;
            vertices[meshIndex] = transform.InverseTransformPoint(vert);
            tris[meshIndex] = meshIndex;

            meshIndex++;
            vert = points[i].position - side * width;
            vertices[meshIndex] = transform.InverseTransformPoint(vert);
            tris[meshIndex] = meshIndex;

            meshIndex++;
            vert = points[j].position + side2 * width;
            vertices[meshIndex] = transform.InverseTransformPoint(vert);
            tris[meshIndex] = meshIndex;

            //----------------------------------

            meshIndex++;
            vert = points[i].position - side * width;
            vertices[meshIndex] = transform.InverseTransformPoint(vert);
            tris[meshIndex] = meshIndex;

            meshIndex++;
            vert = points[j].position - side2 * width;
            vertices[meshIndex] = transform.InverseTransformPoint(vert);
            tris[meshIndex] = meshIndex;

            meshIndex++;
            vert = points[j].position + side2 * width;
            vertices[meshIndex] = transform.InverseTransformPoint(vert);
            tris[meshIndex] = meshIndex;
        }

        side = nPoints - 2 == 0 ? transform.right : Vector3.Cross((points[nPoints - 2].position - points[nPoints - 2 - 1].position).normalized, transform.forward);
        vert = points[nPoints - 2].position + side * width;
        vertices[vertices.Length - 3] = transform.InverseTransformPoint(vert);

        vert = points[nPoints - 2].position - side * width;
        vertices[vertices.Length - 2] = transform.InverseTransformPoint(vert);

        vertices[vertices.Length - 1] = transform.InverseTransformPoint(points[nPoints - 1].position);
        
        tris[tris.Length - 3] = vertices.Length - 3;
        tris[tris.Length - 2] = vertices.Length - 2;
        tris[tris.Length - 1] = vertices.Length - 1;
        

        mesh.vertices = vertices;
        mesh.triangles = tris;
        //mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //Call UploadMeshData to immediately send the modified data to the graphics API, to avoid a possible problem later. 
        //Passing true in a markNoLogerReadable argument makes Mesh data not be readable from the script anymore, and frees up system memory copy of the data.
        mesh.UploadMeshData(false);
    }


    void OnDrawGizmos()
    {
        if (points != null)
        {
            for (int i = points.Count - 1; i > 0; i--)
            {
                Gizmos.color = new Color(points[i].weight, 0, 0);
                Gizmos.DrawLine(points[i].position, points[i - 1].position);
                Gizmos.DrawSphere(points[i].position, 0.1f);
            }
            Gizmos.DrawSphere(points[0].position, 0.1f);

            if (mesh != null && false)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.05f);
                }

                Gizmos.color = Color.green;
                for (int i = 0; i < tris.Length; i += 3)
                {
                    Gizmos.DrawLine(transform.TransformPoint(vertices[tris[i]]), transform.TransformPoint(vertices[tris[i + 1]]));
                    Gizmos.DrawLine(transform.TransformPoint(vertices[tris[i + 1]]), transform.TransformPoint(vertices[tris[i + 2]]));
                    Gizmos.DrawLine(transform.TransformPoint(vertices[tris[i + 2]]), transform.TransformPoint(vertices[tris[i]]));
                }
            }
        }
    }


}
