using System.Collections.Generic;
using TMPro;

public static class UIExtensions
{
    public static void SetupDropdown(this TMP_Dropdown dropdown, List<string> options, int currentOption)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = currentOption;
        dropdown.RefreshShownValue();
    }
}