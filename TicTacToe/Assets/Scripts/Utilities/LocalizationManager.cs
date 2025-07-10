using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq; 
public class LocalizationManager : MonoBehaviour
{
    public static string DefaultLanguage = "en";
    public string currentLanguage = DefaultLanguage;
    public static LocalizationManager Instance { get; private set; }

    private JObject localizationData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLocalization(currentLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void LoadLocalization(string language)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Localization/{language}");
        if (jsonFile != null)
        {
            localizationData = JObject.Parse(jsonFile.text);
        }
        else
        {
            Debug.LogError($"Localization file for '{language}' not found!");
        }
    }
    
    [RuntimeInitializeOnLoadMethod]
    static void CheckLocalizationManager()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("No LocalizationManager found in the scene! Please add one.");
        }
    }
    public string Get(string key)
    {
        if (localizationData == null)
            return key;

        JToken token = localizationData;
        foreach (var part in key.Split('.'))
        {
            if (token[part] != null)
            {
                token = token[part];
            }
            else
            {
                return key;
            }
        }
        return token.Type == JTokenType.String ? token.ToString() : key;
    }
    public void SetLanguage(string language)
    {
        if (localizationData != null && localizationData.ContainsKey(language))
        {
            currentLanguage = language;
            LoadLocalization(language);
        }
        else
        {
            Debug.LogError($"Language '{language}' not supported.");
        }
    }
}