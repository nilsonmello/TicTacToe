using UnityEngine;
using Newtonsoft.Json.Linq; 
public class LocalizationManager : MonoBehaviour
{
    public static Language DefaultLanguage = Language.English;
    public Language currentLanguage = DefaultLanguage;
    public static LocalizationManager Instance { get; private set; }

    private JObject localizationData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetLanguage(currentLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(Language language)
    {
        string langCode = LanguageToCode(language);
        LoadLocalization(langCode);
        currentLanguage = language;
    }
    private string LanguageToCode(Language language)
    {
        switch (language)
        {
            case Language.English: return "en";
            case Language.PortugueseBrazil:  return "pt-BR";
            default: return "en";
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
}