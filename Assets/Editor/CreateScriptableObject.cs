using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace Sjabloon
{
    public class CreateScriptableObject
    {
        [MenuItem("Assets/Create/Schmup/Pattern")]
        public static void CreatePattern()
        {
            CreateAsset<Pattern>();
        }

        [MenuItem("Assets/Create/Schmup/BulletDefinition")]
        public static void CreateBulletDefinition()
        {
            CreateAsset<BulletDefinition>();
        }

        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}