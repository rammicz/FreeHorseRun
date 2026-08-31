using UnityEngine;

public sealed class PrivacyPolicyLink : MonoBehaviour
{
    private const string PrivacyPolicyUrl = "https://raw.githubusercontent.com/rammicz/freehorserun/master/PRIVACY_POLICY.md";
    private const float ButtonWidth = 190f;
    private const float ButtonHeight = 42f;
    private const float Margin = 12f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (FindFirstObjectByType<PrivacyPolicyLink>() != null)
            return;

        var link = new GameObject(nameof(PrivacyPolicyLink));
        DontDestroyOnLoad(link);
        link.AddComponent<PrivacyPolicyLink>();
    }

    private void OnGUI()
    {
        var buttonRect = new Rect(
            Screen.width - ButtonWidth - Margin,
            Screen.height - ButtonHeight - Margin,
            ButtonWidth,
            ButtonHeight);

        if (GUI.Button(buttonRect, "Privacy policy"))
            Application.OpenURL(PrivacyPolicyUrl);
    }
}
