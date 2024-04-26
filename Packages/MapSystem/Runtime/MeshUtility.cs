using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MapSystem.Runtime
{
    public static class MeshUtility
    {
        private static float Mesh16BitBufferVertexLimit = 65535; //mesh顶点限制

        /// <summary>
        /// 合并网格
        /// </summary>
        /// <param name="parent">合并子物体</param>
        /// <param name="skipTag">有这个tag的物体会从待合并的物体中剔除</param>
        public static void CombineMeshes(Transform parent, bool createMultiMaterial = false, string skipTag = "")
        {
            if (parent.GetComponent<MeshRenderer>() == null)
            {
                parent.gameObject.AddComponent<MeshRenderer>();
            }

            if (parent.GetComponent<MeshFilter>() == null)
            {
                parent.gameObject.AddComponent<MeshFilter>();
            }
            
            var oldParent = parent.parent.transform;
            var oldScale = parent.localScale;
            var oldPosition = parent.position;
            var oldRotation = parent.rotation;
            
            //父物体位置和旋转等信息会影响到合并后的mesh位置,处理方式： 合并前保留信息，合并后再赋值原信息
            parent.transform.parent = null;
            parent.localScale = Vector3.one;
            parent.position = Vector3.zero;
            parent.rotation = Quaternion.identity;
            
            var meshFilters = GetMeshFiltersToCombine(parent, skipTag);
            if (createMultiMaterial)
            {
                CombineMeshesWithMultiMaterial(parent, meshFilters, skipTag);
            }
            else
            {
                CombineMeshesWithSingleMaterial(parent, meshFilters, skipTag);
            }
            
            parent.transform.parent = oldParent;
            parent.localScale = oldScale;
            parent.position = oldPosition;
            parent.rotation = oldRotation;
        }

        /// <summary>
        /// 针对都是同一材质的所有物体进行合并
        /// </summary>
        /// <param name="skipTag"></param>
        private static void CombineMeshesWithSingleMaterial(Transform parent, MeshFilter[] meshFilters, string skipTag)
        {
            if (meshFilters != null)
            {
                long vertexCount = 0;
                var combineInstances = new CombineInstance[meshFilters.Length - 1];
                for (var i = 0; i < meshFilters.Length - 1; i++) //排除parent的meshfilter
                {
                    combineInstances[i].subMeshIndex = 0;
                    combineInstances[i].mesh = meshFilters[i + 1].sharedMesh;
                    combineInstances[i].transform = meshFilters[i + 1].transform.localToWorldMatrix;
                    vertexCount += meshFilters[i + 1].sharedMesh.vertexCount;
                }


                //set material
                MeshRenderer[] meshRenderers = parent.GetComponentsInChildren<MeshRenderer>();
                if (meshRenderers.Length >= 2)
                {
                    meshRenderers[0].sharedMaterials = new Material[1];
                    meshRenderers[0].sharedMaterial = meshRenderers[1].sharedMaterial;
                }
                else
                {
                    meshRenderers[0].sharedMaterials = new Material[0]; // Reset the MeshRenderer's Materials array.
                }

                var combineMesh = new Mesh();
                if (vertexCount >= Mesh16BitBufferVertexLimit)
                {
                    combineMesh.indexFormat = IndexFormat.UInt32;
                }

                combineMesh.CombineMeshes(combineInstances);
                meshFilters[0].sharedMesh = combineMesh;
                DeactivateCombinedGameObjects(meshFilters);
            }
        }

        /// <summary>
        /// 针对不同材质的所有物体进行合并
        /// </summary>
        /// <param name="skipTag"></param>
        private static void CombineMeshesWithMultiMaterial(Transform parent, MeshFilter[] meshFilters, string skipTag)
        {
            var meshRenderers = new MeshRenderer[meshFilters.Length];
            meshRenderers[0] = parent.GetComponent<MeshRenderer>(); //parent meshRenderer
            //get all materials in children
            var childMaterials = new List<Material>();
            for (var i = 0; i < meshFilters.Length - 1; i++)
            {
                meshRenderers[i + 1] = meshFilters[i + 1].GetComponent<MeshRenderer>();
                if (meshRenderers[i + 1] != null)
                {
                    var materials = meshRenderers[i + 1].sharedMaterials;
                    for (int j = 0; j < materials.Length; j++)
                    {
                        if (!childMaterials.Contains(materials[j]))
                        {
                            childMaterials.Add(materials[j]);
                        }
                    }
                }
            }

            List<CombineInstance> finalMeshCombineInstancesList = new List<CombineInstance>();
            long vertextCount = 0;
            for (int i = 0; i < childMaterials.Count; i++)
            {
                var submeshCombineInstancesList = new List<CombineInstance>();
                for (var j = 0; j < meshFilters.Length - 1; j++) //排除parent
                {
                    if (meshRenderers[j + 1] != null)
                    {
                        var subMaterials = meshRenderers[j + 1].sharedMaterials;
                        for (var k = 0; k < subMaterials.Length; k++)
                        {
                            if (childMaterials[i] == subMaterials[k])
                            {
                                var combineInstance = new CombineInstance();
                                combineInstance.subMeshIndex = k;
                                combineInstance.mesh = meshFilters[j + 1].sharedMesh;
                                combineInstance.transform = meshFilters[j + 1].transform.localToWorldMatrix;
                                submeshCombineInstancesList.Add(combineInstance);
                                vertextCount += combineInstance.mesh.vertexCount;
                            }
                        }   
                    }
                }
                
                var subMesh = new Mesh();
                if (vertextCount >= Mesh16BitBufferVertexLimit)
                {
                    subMesh.indexFormat = IndexFormat.UInt32;
                }
                subMesh.CombineMeshes(submeshCombineInstancesList.ToArray(), true);

                var subMeshCombineInstance = new CombineInstance();
                subMeshCombineInstance.subMeshIndex = 0;
                subMeshCombineInstance.mesh = subMesh;
                subMeshCombineInstance.transform = Matrix4x4.identity;
                finalMeshCombineInstancesList.Add(subMeshCombineInstance);
            }

            meshRenderers[0].sharedMaterials = childMaterials.ToArray();
            var mesh = new Mesh();
            if (vertextCount >= Mesh16BitBufferVertexLimit)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }
            mesh.CombineMeshes(finalMeshCombineInstancesList.ToArray(), false);
            meshFilters[0].sharedMesh = mesh;
            DeactivateCombinedGameObjects(meshFilters); //隐藏原来的gameobject
        }

        private static MeshFilter[] GetMeshFiltersToCombine(Transform transform, string skipTag)
        {
            if (transform != null)
            {
                var meshFilters = transform.GetComponentsInChildren<MeshFilter>();
                if (!string.IsNullOrEmpty(skipTag))
                {
                    meshFilters = meshFilters.Where((mesh) => !mesh.CompareTag(skipTag)).ToArray();
                }

                return meshFilters;
            }
            else
            {
                Debug.LogError($"GameObject To Combine is Null, Need Set");
            }

            return null;
        }

        private static void DeactivateCombinedGameObjects(MeshFilter[] meshFilters)
        {
            if (meshFilters != null)
            {
                for (var i = 0; i < meshFilters.Length - 1; i++) // Skip first MeshFilter
                {
                    meshFilters[i + 1].gameObject.SetActive(false);
                }
            }
        }
    }
}