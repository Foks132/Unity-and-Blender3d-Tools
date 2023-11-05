using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AutoGeneratePrefabTool : MonoBehaviour
{
    [Header("Path")]
    public GameObject meshPath;
    public string prefabSavePath = "ExoticPrefabs";
    [Header("Colider")]
    public string coliderName = "_Clip";
    [Header("Lods")]
    public string lodNameNoIndex = "_LOD_";
    public string lod0Name = "_LOD_0";
    public string lod1Name = "_LOD_1";
    public string lod2Name = "_LOD_2";
    [Header("Prefab")]
    public string outputLodNameNoIndex = "Model_";
    public string outputLod0Name = "Model_0";
    public string outputLod1Name = "Model_1";
    public string outputLod2Name = "Model_2";
    [Header("LodGroupSettings")]
    [Range(1, 3)]
    public int lodCount = 2;
    [Range(0.0f, 1f)]
    public float lod1Transition = 0.4f;
    [Range(0.0f, 1f)]
    public float lod2Transition = 0.2f;
    [Range(0.0f, 0.1f)]
    public float culledTransition = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        string _path = Application.dataPath + $"/{prefabSavePath}";
        if (System.IO.Directory.Exists(_path))
        {
            var instanceRoot = PrefabUtility.InstantiatePrefab(meshPath);
            var variantRoot = PrefabUtility.SaveAsPrefabAsset((GameObject)instanceRoot, $"{_path}/Variant.prefab");
            var allobject = new List<string>();
            Component[] meshes = variantRoot.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter mesh in meshes)
            {
                Transform objTransform = mesh.transform;
                string objName = mesh.name;

                if (objName.Contains(lod0Name) & allobject.Find(x => x.Equals(objName.ToLower())) == null) //Поиск первой модели (LOD 0)
                {
                    allobject.Add(objName.ToLower());
                    GameObject obj = new GameObject();

                    obj.transform.position = objTransform.position; //Задать позицию
                    obj.transform.rotation = objTransform.rotation; //Задать вращение
                    obj.name = objName.Remove(objName.IndexOf($"{lodNameNoIndex}")); //Задать имя новому объекту

                    obj.transform.localScale = objTransform.transform.localScale;
                    Instantiate(mesh, obj.transform, true).name = outputLod0Name; //Создать объект нулевой LOD 

                    var objClip = obj.AddComponent<MeshCollider>();
                    var objLodGroup = obj.AddComponent<LODGroup>();

                    foreach (MeshFilter _mesh in meshes) 
                    {
                        if (_mesh.name.Contains(obj.name) & _mesh.name.Contains(lod1Name)) //Модель первой LOD
                        {
                            _mesh.transform.localScale = new Vector3(1, 1, 1);
                            Instantiate(_mesh, mesh.transform.position, obj.transform.rotation, obj.transform).name = outputLod1Name; //Создать объект первой LOD 
                        }

                        if (_mesh.name.Contains(obj.name) & _mesh.name.Contains(lod2Name)) //Модель второй LOD
                        {
                            _mesh.transform.localScale = new Vector3(1, 1, 1);
                            Instantiate(_mesh, mesh.transform.position, obj.transform.rotation, obj.transform).name = outputLod2Name; //Создать объект второй LOD 
                        }

                        if (_mesh.name.Contains(obj.name) & _mesh.name.Contains(coliderName)) //Collider для объекта
                        {
                            objClip.sharedMesh = _mesh.sharedMesh;
                        }
                    }

                    if (objClip.sharedMesh == null) //Если Collider не найден зададим модель нулевой LOD
                    {
                        objClip.sharedMesh = mesh.sharedMesh;
                    }

                    LOD[] lods = new LOD[lodCount];
                    for (int i = 0; i < lods.Length; i++) //Поиск LOD для установки в LOD Group
                    {
                        Renderer[] renderers = new Renderer[1];
                        if (objLodGroup.transform.Find($"{outputLodNameNoIndex}{i}") != null)
                        {
                            renderers[0] = objLodGroup.transform.Find($"{outputLodNameNoIndex}{i}").gameObject.GetComponent<Renderer>();
                            lods[i] = new LOD(1.0F / (i + 2), renderers);
                        }
                        else
                        {
                            break;
                        }
                    }

                    lods[0].screenRelativeTransitionHeight = lod1Transition;
                    if (lodCount == 3)
                    {
                        lods[1].screenRelativeTransitionHeight = lod2Transition;
                    }
                    lods[lods.Length-1].screenRelativeTransitionHeight = culledTransition;

                    objLodGroup.SetLODs(lods);
                    objLodGroup.RecalculateBounds();

                    var variantObj = PrefabUtility.SaveAsPrefabAsset(obj, $"{_path}/{obj.name}.prefab"); //Создать Prefab объекта
                    variantObj.transform.localScale = new Vector3(100, 100, 100); 
                    variantObj.transform.position = new Vector3(0, 0, 0);
                    variantObj.transform.rotation = Quaternion.identity;

                    Debug.Log(obj.name + " done!");
                }

                MeshCollider meshCollider = mesh.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh.sharedMesh;
            }
        }
        else
        {
            System.IO.Directory.CreateDirectory(_path);
            Start();
        }
    }
}
