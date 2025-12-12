/*--------------------------------------------------------------------------------*
  File Name: CombineMeshes.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections.Generic;

public class CombineChildrenMeshes : MonoBehaviour
{
    [Header("Options")]
    public bool combineAtStart = true;
    public bool destroyChildren = false;

    void Start()
    {
        if (combineAtStart)
            CombineMeshes();
    }

    [ContextMenu("Combine Meshes")]
    public void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        // Parent MeshFilter
        MeshFilter parentFilter = gameObject.GetComponent<MeshFilter>();
        if (parentFilter == null) parentFilter = gameObject.AddComponent<MeshFilter>();

        // Parent Renderer
        MeshRenderer parentRenderer = gameObject.GetComponent<MeshRenderer>();
        if (parentRenderer == null) parentRenderer = gameObject.AddComponent<MeshRenderer>();

        // Grab the first material as default
        Material firstMaterial = null;

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == parentFilter) continue; // skip parent

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr && firstMaterial == null) firstMaterial = mr.sharedMaterial;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = mf.transform.localToWorldMatrix;
            combineInstances.Add(ci);
        }

        // Create combined mesh
        Mesh combined = new Mesh();
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combined.CombineMeshes(combineInstances.ToArray(), true, true);

        parentFilter.sharedMesh = combined;
        parentRenderer.sharedMaterial = firstMaterial;

        Debug.Log("Combined " + combineInstances.Count + " meshes into parent.");

        // Optional destroy
        if (destroyChildren)
        {
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf == parentFilter) continue;
                Destroy(mf.gameObject);
            }
        }
    }
}
