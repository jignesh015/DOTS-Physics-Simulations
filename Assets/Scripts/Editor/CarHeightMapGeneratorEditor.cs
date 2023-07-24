using UnityEngine;
using UnityEditor;

namespace PhysicsSimulations
{
    [CustomEditor(typeof(CarHeightMapGenerator))]
    public class CarHeightMapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the default inspector first (optional).

            CarHeightMapGenerator generator = (CarHeightMapGenerator)target; // Cast the target to the appropriate type.

            // Add a button to the inspector GUI.
            if (GUILayout.Button("Generate JSON"))
            {
                generator.GenerateHeightmapJSON(); // Call the custom method in your script when the button is clicked.
            }
        }
    }
}
