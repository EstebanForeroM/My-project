using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CustomWindow : EditorWindow
{
    private enum Type
    {
        Interactables,
        Trees,
        Houses,
        Mountains,
        FrontPlayer
    }

    private struct TypeData
    {
        public int orderInLayer;
        public int z;

        public TypeData(int orderInLayer, int z)
        {
            this.orderInLayer = orderInLayer;
            this.z = z;
        }
    }

    private bool oneLayerInFront;
    private float size = 0.5f;
    private Type selectedType = Type.Interactables;
    private string[] categories = new string[] { "Extras", "Trees", "Houses", "Halloween", "Nature", "Mountains",
    "Terrain", "Rocks", "Spikes", "Animals" }; // Add more categories here
    private int selectedCategoryIndex = 0;
    private Dictionary<string, List<GameObject>> prefabsByCategory; // Dictionary to associate categories with prefab lists
    private TypeData[] typeData;
    private bool showPrefabAssignment = true; // Variable to control the foldout section
    private GameObject selectedPrefab; // Variable to store the selected prefab

    private const float PrefabButtonSize = 100f; // Tamaño de los botones de los prefabs
    private const float PrefabButtonPadding = 5f; // Espaciado entre los botones de los prefabs
    private GameObject slidePrefab;

    private GameObject slidesParent; // Referencia al objeto "Slides"
    private GameObject activeSlide; // Referencia a la slide activa
    private List<GameObject> allSlides = new List<GameObject>(); // Lista de todas las slides
    private int selectedSlideIndex = 0; // Índice de la slide seleccionada en el dropdown
    private const float CameraPOV = 45.5f;

    private Vector2 scrollPosition;


    [MenuItem("Window/Custom Window")]
    public static void ShowWindow()
    {
        GetWindow<CustomWindow>("Custom Window");
    }

    void OnEnable()
    {
        InitializeTypeData();
        InitializePrefabDictionary();
        LoadPrefabConfiguration();
        FindAllSlides();
    }

    void LoadPrefabConfiguration()
    {
        foreach (var category in categories)
        {
            string joinedPaths = EditorPrefs.GetString("CustomWindow_" + category, "");
            string[] prefabPaths = joinedPaths.Split(';');

            List<GameObject> prefabs = new List<GameObject>();
            foreach (string path in prefabPaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    prefabs.Add(prefab);
                }
            }

            prefabsByCategory[category] = prefabs;
        }
    }


    void FindAllSlides()
    {
        slidesParent = GameObject.Find("Slides") ?? new GameObject("Slides");
        allSlides.Clear();
        foreach (Transform child in slidesParent.transform)
        {
            allSlides.Add(child.gameObject);
        }
    }

    void InitializeTypeData()
    {
        // Initialize data for each type
        typeData = new TypeData[4];
        typeData[(int)Type.Interactables] = new TypeData(10, 0);
        typeData[(int)Type.Trees] = new TypeData(8, 1);
        typeData[(int)Type.Houses] = new TypeData(6, 2);
        typeData[(int)Type.Mountains] = new TypeData(4, 10);
        //typeData[(int)Type.FrontPlayer] = new TypeData(11, 0);
    }

    void InitializePrefabDictionary()
    {
        // Initialize the dictionary and assign empty lists to each category
        prefabsByCategory = new Dictionary<string, List<GameObject>>();
        foreach (var category in categories)
        {
            prefabsByCategory[category] = new List<GameObject>(); // Initialize an empty list for each category
        }
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DisplayActiveSlideDropdown();
        if (GUILayout.Button("InitNewSlide"))
        {
            InitNewSlide();
        }
        DisplayTypeDropdown();
        DisplayOneLayerInFrontCheckbox();
        DisplaySizeSlider();
        DisplayCategoryDropdown();
        DisplayPrefabGrid();
        if (selectedPrefab != null)
        {
            EditorGUILayout.LabelField("Selected Prefab: " + selectedPrefab.name);
        }

        if (GUILayout.Button("Create Prefab"))
        {
            CreatePrefab();
        }
        DisplayPrefabAssignmentSection();
        DisplaySlidePrefabAssignment();

        if (GUILayout.Button("Save"))
        {
            SavePrefabConfiguration();
        }
        EditorGUILayout.EndScrollView();
    }

    void SavePrefabConfiguration()
    {
        foreach (var category in categories)
        {
            List<GameObject> prefabs = prefabsByCategory[category];
            List<string> prefabPaths = new List<string>();

            foreach (GameObject prefab in prefabs)
            {
                if (prefab != null)
                {
                    string path = AssetDatabase.GetAssetPath(prefab);
                    prefabPaths.Add(path);
                }
            }

            string joinedPaths = string.Join(";", prefabPaths);
            EditorPrefs.SetString("CustomWindow_" + category, joinedPaths);
        }
    }


    void DisplayActiveSlideDropdown()
    {
        if (allSlides.Count > 0)
        {
            string[] slideNames = allSlides.ConvertAll(slide => slide.name).ToArray();
            selectedSlideIndex = EditorGUILayout.Popup("Active Slide", selectedSlideIndex, slideNames);
            activeSlide = allSlides[selectedSlideIndex];
        }
    }

    void CreatePrefab()
    {
        if (selectedPrefab != null && activeSlide != null)
        {
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            prefabInstance.transform.position = new Vector3(0, 0, typeData[(int)selectedType].z);
            SpriteRenderer spriteRenderer = prefabInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                int adder = oneLayerInFront ? 1 : 0;
                spriteRenderer.sortingOrder = typeData[(int)selectedType].orderInLayer + adder;
            }
            prefabInstance.transform.parent = activeSlide.transform; // Hacer que el prefab sea hijo de la slide activa

            // Ajustar el tamaño del prefab según el valor del slider "size" y la distancia en Z usando la regresión lineal
            float zDistance = Mathf.Abs(prefabInstance.transform.position.z);
            float scaleFactor = (0.05f * zDistance) + 1; // Aplicar la función de regresión lineal
            prefabInstance.transform.localScale = new Vector3(size * scaleFactor, size * scaleFactor, 1);

            // Seleccionar el prefab recién creado en el editor
            Selection.activeGameObject = prefabInstance;
        }
    }

    void DisplayTypeDropdown()
    {
        // Dropdown for types
        selectedType = (Type)EditorGUILayout.EnumPopup("Type", selectedType);
    }

    void DisplayOneLayerInFrontCheckbox()
    {
        // Checkbox for 1 layer in front
        oneLayerInFront = EditorGUILayout.Toggle("1 Layer In Front", oneLayerInFront);
    }

    void DisplaySizeSlider()
    {
        // Slider for size
        size = EditorGUILayout.Slider("Size", size, 0f, 2f);
    }

    void DisplayCategoryDropdown()
    {
        // Dropdown for categories
        selectedCategoryIndex = EditorGUILayout.Popup("Category", selectedCategoryIndex, categories);
    }

    void DisplayPrefabGrid()
    {
        string selectedCategory = categories[selectedCategoryIndex];
        List<GameObject> prefabs = prefabsByCategory[selectedCategory];

        if (prefabs.Count > 0)
        {
            EditorGUILayout.LabelField("Prefabs:");
            float windowWidth = EditorGUIUtility.currentViewWidth;
            int gridCount = Mathf.FloorToInt((windowWidth - PrefabButtonPadding) / (PrefabButtonSize + PrefabButtonPadding));
            int rows = Mathf.CeilToInt((float)prefabs.Count / gridCount);

            for (int i = 0; i < rows; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < gridCount; j++)
                {
                    int index = i * gridCount + j;
                    if (index < prefabs.Count)
                    {
                        Texture2D preview = AssetPreview.GetAssetPreview(prefabs[index]);
                        if (GUILayout.Button(preview, GUILayout.Width(PrefabButtonSize), GUILayout.Height(PrefabButtonSize)))
                        {
                            selectedPrefab = prefabs[index];
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    void DisplayPrefabAssignmentSection()
    {
        // Foldout section for prefab assignment
        showPrefabAssignment = EditorGUILayout.Foldout(showPrefabAssignment, "Object Assignment", true);
        if (showPrefabAssignment)
        {
            foreach (var category in categories)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(category);
                if (GUILayout.Button("Add"))
                {
                    AddPrefabToCategory(category);
                }
                EditorGUILayout.EndHorizontal();

                List<GameObject> prefabs = prefabsByCategory[category];
                for (int i = 0; i < prefabs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
                    if (GUILayout.Button("Remove"))
                    {
                        RemovePrefabFromCategory(category, i);
                        break; // Break to avoid modifying the list while iterating
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

    void AddPrefabToCategory(string category)
    {
        prefabsByCategory[category].Add(null); // Add a null element to the list
    }

    void RemovePrefabFromCategory(string category, int index)
    {
        prefabsByCategory[category].RemoveAt(index); // Remove the prefab at the specified index
    }

    void DisplaySlidePrefabAssignment()
    {
        // Sección para asignar el prefab del slide
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Slide Prefab");
        slidePrefab = (GameObject)EditorGUILayout.ObjectField(slidePrefab, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();
    }


    void InitNewSlide()
    {
        // Ocultar todas las slides excepto la slide activa
        foreach (GameObject slide in allSlides)
        {
            slide.SetActive(false);
        }

        // Crear un nuevo slide con un nombre único
        string slideName = "Slide" + (slidesParent.transform.childCount + 1);
        if (slidePrefab != null)
        {
            GameObject slideInstance = (GameObject)PrefabUtility.InstantiatePrefab(slidePrefab);
            slideInstance.name = slideName; // Asignar el nombre único al slide
            slideInstance.transform.parent = slidesParent.transform;
            slideInstance.transform.position = Vector3.zero;
            slideInstance.SetActive(true); // Activar la nueva slide
            activeSlide = slideInstance; // Establecer la nueva slide como activa
            allSlides.Add(slideInstance); // Añadir la nueva slide a la lista
            selectedSlideIndex = allSlides.Count - 1; // Actualizar el índice seleccionado
        }

        // Posicionar la vista de la cámara
        SceneView.lastActiveSceneView.pivot = Vector3.zero;
        SceneView.lastActiveSceneView.Repaint();
    }
}
