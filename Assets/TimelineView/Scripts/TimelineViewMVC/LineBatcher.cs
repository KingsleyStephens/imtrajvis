using UnityEngine;

namespace KingsleyPhD
{
    public class LineBatcher : MonoBehaviour
    {
        // PUBLIC DATA

        // Material to render line
        [SerializeField]
        private Material lineMaterial;

        
        // PUBLIC
        public void Create(Vector3[] vertices, int[] indices)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshRenderer>().material = lineMaterial;
        }
    }
}
