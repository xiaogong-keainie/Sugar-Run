using UnityEngine;
using UnityEditor;
using System.IO;

public class FoodBaker
{
    [MenuItem("Tools/Bake Food Prefabs", false, 1)]
    static void Bake()
    {
        string folder = "Assets/Prefabs/Mg3D_Food";
        string[] fbxs = Directory.GetFiles(folder, "*.fbx");
        int count = 0;

        foreach (string fbx in fbxs)
        {
            string name = Path.GetFileNameWithoutExtension(fbx);
            string modelPath = Path.Combine(folder, name + ".fbx").Replace("\\", "/");
            string prefabPath = Path.Combine(folder, name + ".prefab").Replace("\\", "/");

            if (File.Exists(prefabPath))
            {
                // Assign stats to existing prefab
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (existing != null)
                {
                    FoodItem food = existing.GetComponent<FoodItem>();
                    if (food != null) AssignStats(name, food);
                }
                continue;
            }

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = name;
            instance.transform.localScale = Vector3.one * 0.5f;

            BoxCollider2D col = instance.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            FoodItem foodComp = instance.AddComponent<FoodItem>();
            AssignStats(name, foodComp);

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            count++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"Done! Created {count} food prefabs.");
    }

    static void AssignStats(string name, FoodItem food)
    {
        switch (name)
        {
            case "Hamburger":  food.sugarBonus = 15f; food.hungerBonus = 20f; break;
            case "Hotdog":     food.sugarBonus = 10f; food.hungerBonus = 15f; break;
            case "Cheese":     food.sugarBonus = 5f;  food.hungerBonus = 10f; break;
            case "Banana":     food.sugarBonus = 8f;  food.hungerBonus = 5f;  break;
            case "Cherry":     food.sugarBonus = 3f;  food.hungerBonus = 2f;  break;
            case "Olive":      food.sugarBonus = 2f;  food.hungerBonus = 1f;  break;
            case "Watermelon": food.sugarBonus = 5f;  food.hungerBonus = 8f;  break;
        }
    }
}
