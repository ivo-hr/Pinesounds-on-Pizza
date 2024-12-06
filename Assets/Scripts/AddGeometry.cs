using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AddGeometry : MonoBehaviour
{
     private FMOD.Geometry geometry;

    void Start()
    {
        // Obtener el Mesh del GameObject
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("No MeshFilter found on this GameObject!");
            return;
        }

        // Crear geometr�a en FMOD
        RuntimeManager.CoreSystem.createGeometry(10, 100, out geometry);

        // Obtener datos del Mesh
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Agregar pol�gonos al sistema de geometr�a
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Los tri�ngulos est�n definidos en conjuntos de 3 �ndices
            int index1 = triangles[i];
            int index2 = triangles[i + 1];
            int index3 = triangles[i + 2];

            // Convertir v�rtices al sistema local de FMOD
            FMOD.VECTOR fmodVertex1 = UnityToFMODVector(transform.TransformPoint(vertices[index1]));
            FMOD.VECTOR fmodVertex2 = UnityToFMODVector(transform.TransformPoint(vertices[index2]));
            FMOD.VECTOR fmodVertex3 = UnityToFMODVector(transform.TransformPoint(vertices[index3]));

            // Agregar un tri�ngulo a la geometr�a de FMOD
            geometry.addPolygon(1.0f, 1.0f, true, 3, new[] { fmodVertex1, fmodVertex2, fmodVertex3 }, out _);
        }
    }

    private FMOD.VECTOR UnityToFMODVector(Vector3 vector)
    {
        FMOD.VECTOR fmodVector;
        fmodVector.x = vector.x;
        fmodVector.y = vector.y;
        fmodVector.z = vector.z;
        return fmodVector;
    }

}
