using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KingdomBorder.TwinTowers.Editor
{
    public static class TwinTowerMenu
    {
        public const string Root = "Assets/_Project/TwinTowers";
        [MenuItem("Kingdom Border/双塔/打开演示场景")]
        public static void OpenDemo()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(Root + "/Demo/TwinTowerDemo.unity");
        }
        [MenuItem("Kingdom Border/双塔/查看塔参数")]
        public static void Settings()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TowerDefinition>(Root + "/Data/MageSettings.asset");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        [MenuItem("Kingdom Border/双塔/查看箭塔参数")]
        public static void ArrowSettings()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TowerDefinition>(Root + "/Data/ArrowSettings.asset");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        [MenuItem("Kingdom Border/双塔/查看投石塔参数")]
        public static void ArtillerySettings()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TowerDefinition>(Root + "/Data/ArtillerySettings.asset");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        [MenuItem("Kingdom Border/双塔/查看塔预制体")]
        public static void Prefabs()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/FrostTower.prefab");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        [MenuItem("Kingdom Border/双塔/查看中文说明")]
        public static void Readme()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "/Documentation/README.txt");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}
