using System.Linq;
using UnityEditor;
using UnityEngine;
#if STARTER_ASSETS_PACKAGES_CHECKED
using Unity.Cinemachine; // ✅ Corregido: Cinemachine 3.x usa Unity.Cinemachine
#endif

namespace StarterAssets
{
    public partial class StarterAssetsDeployMenu : ScriptableObject
    {
        public const string MenuRoot = "Tools/Starter Assets";

        // prefab names
        private const string MainCameraPrefabName = "MainCamera";
        private const string PlayerCapsulePrefabName = "PlayerCapsule";

        // names in hierarchy
        private const string CinemachineVirtualCameraName = "PlayerFollowCamera";

        // tags
        private const string PlayerTag = "Player";
        private const string MainCameraTag = "MainCamera";
        private const string CinemachineTargetTag = "CinemachineTarget";

        private static string StarterAssetsPath => PathToThisFile;

        private static GameObject _cinemachineVirtualCamera;

        public static string StarterAssetsInstallPath
        {
            get
            {
                string path = PathToThisFile;
                return path.Substring(0, path.LastIndexOf("StarterAssets"));
            }
        }

        private static string PathToThisFile
        {
            get
            {
                var dummy = CreateInstance<StarterAssetsDeployMenu>();
                string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(dummy));
                DestroyImmediate(dummy);
                return path.Substring(0, path.LastIndexOf("/Editor/StarterAssetsDeployMenu.cs"));
            }
        }

        [MenuItem(MenuRoot + "/Reinstall Dependencies", false)]
        static void ResetPackageChecker()
        {
            ScriptingDefineUtils.RemoveScriptingDefine(PackageChecker.PackageCheckerScriptingDefine);
        }

#if STARTER_ASSETS_PACKAGES_CHECKED
        private static void CheckCameras(string prefabPath, Transform targetParent)
        {
            CheckMainCamera(prefabPath);

            GameObject vcam = GameObject.Find(CinemachineVirtualCameraName);

            if (!vcam)
            {
                HandleInstantiatingPrefab(StarterAssetsPath + prefabPath,
                    CinemachineVirtualCameraName,
                    out GameObject vcamPrefab);
                _cinemachineVirtualCamera = vcamPrefab;
            }
            else
            {
                _cinemachineVirtualCamera = vcam;
            }

            GameObject[] targets = GameObject.FindGameObjectsWithTag(CinemachineTargetTag);
            GameObject target = targets.FirstOrDefault(t => t.transform.IsChildOf(targetParent));
            if (target == null)
            {
                target = new GameObject("PlayerCameraRoot");
                target.transform.SetParent(targetParent);
                target.transform.localPosition = new Vector3(0f, 1.375f, 0f);
                target.tag = CinemachineTargetTag;
                Undo.RegisterCreatedObjectUndo(target, "Created new cinemachine target");
            }
            CheckVirtualCameraFollowReference(target, _cinemachineVirtualCamera);
        }

        private static void CheckMainCamera(string prefabPath)
        {
            GameObject[] mainCameras = GameObject.FindGameObjectsWithTag(MainCameraTag);

            if (mainCameras.Length < 1)
            {
                HandleInstantiatingPrefab(StarterAssetsPath + prefabPath, MainCameraPrefabName,
                    out _);
            }
            else
            {
                // ✅ Corregido: CinemachineBrain sigue igual en Cinemachine 3.x
                if (!mainCameras[0].TryGetComponent(out CinemachineBrain cinemachineBrain))
                    mainCameras[0].AddComponent<CinemachineBrain>();
            }
        }

        private static void CheckVirtualCameraFollowReference(GameObject target,
            GameObject cinemachineVirtualCamera)
        {
            // ✅ Corregido: En Cinemachine 3.x se usa CinemachineCamera en lugar de CinemachineVirtualCamera
            // y la propiedad de seguimiento cambió de m_Follow a Target.TrackingTarget
            var cam = cinemachineVirtualCamera.GetComponent<CinemachineCamera>();

            if (cam != null)
            {
                // Cinemachine 3.x: asignar el target de seguimiento
                cam.Target.TrackingTarget = target.transform;
            }
            else
            {
                // Fallback por si el prefab aún usa la API antigua (por compatibilidad)
                Debug.LogWarning("[StarterAssets] No se encontró CinemachineCamera en el prefab. " +
                                 "Asegúrate de actualizar el prefab PlayerFollowCamera para Cinemachine 3.x");
            }
        }

        private static void HandleInstantiatingPrefab(string path, string prefabName, out GameObject prefab)
        {
            prefab = (GameObject) PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<Object>($"{path}{prefabName}.prefab"));
            Undo.RegisterCreatedObjectUndo(prefab, "Instantiate Starter Asset Prefab");

            prefab.transform.localPosition = Vector3.zero;
            prefab.transform.localEulerAngles = Vector3.zero;
            prefab.transform.localScale = Vector3.one;
        }
#endif
    }
}