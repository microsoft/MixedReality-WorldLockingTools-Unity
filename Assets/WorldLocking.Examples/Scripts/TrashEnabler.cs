using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

#if true
public class TrashEnabler : MonoBehaviour
{

    private List<Material> materials = new List<Material>();

    private Color focusColor = Color.white;
    private Color nofocColor = Color.grey;

    private void SetupColors()
    {
        if (!NeedAction)
        {
            focusColor = Color.green;
        }
        else
        {
            focusColor = Color.red;
        }
        nofocColor = focusColor * 0.5f;
    }

    private void DoThing()
    {
        if (WorldLockingManager.GetInstance().RefreezeIndicated)
        {
            WorldLockingManager.GetInstance().FragmentManager.Refreeze();
        }
        else if (WorldLockingManager.GetInstance().MergeIndicated)
        {
            WorldLockingManager.GetInstance().FragmentManager.Merge();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        CaptureMaterials();
        SetupColors();
        SetColor();
    }

    private void CaptureMaterials()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"Got {renderers.Length} renderers from {name}");
        foreach (var rend in renderers)
        {
            materials.Add(rend.material);
            Debug.Log($"Got {rend.material.name} from {rend.name}");
        }
    }

    private bool NeedAction { get { return WorldLockingManager.GetInstance().RefreezeIndicated || WorldLockingManager.GetInstance().MergeIndicated; } }
    private bool needAction = false;
    private bool focused = false;
    // Update is called once per frame
    void Update()
    {
        if (NeedAction != needAction)
        {
            needAction = NeedAction;
            SetupColors();
            SetColor();
        }
    }

    public void OnFocusOn()
    {
        focused = true;
        SetColor();
    }

    public void OnFocusOff()
    {
        focused = false;
        SetColor();
    }

    public void OnSelect()
    {
        DoThing();
        SetColor();
    }

    private void SetColor()
    {
        if (focused)
        {
            SetColor("_Color", focusColor);
        }
        else
        {
            SetColor("_Color", nofocColor);
        }
    }
    private void SetColor(string paramName, Color color)
    {
        foreach (var mat in materials)
        {
            mat.SetColor(paramName, color);
        }
    }
}
#else

public class TrashEnabler : MonoBehaviour
{

    private List<Material> materials = new List<Material>();

    private Color focusColor = Color.white;
    private Color nofocColor = Color.grey;

    private void SetupColors()
    {
        bool enabled = WorldLockingManager.GetInstance().Enabled;

        if (enabled)
        {
            focusColor = Color.green;
        }
        else
        {
            focusColor = Color.red;
        }
        nofocColor = focusColor * 0.5f;
    }

    private void ToggleSelected()
    {
        var settings = WorldLockingManager.GetInstance().Settings;
        settings.Enabled = !settings.Enabled;
        WorldLockingManager.GetInstance().Settings = settings;
        if (!settings.Enabled)
        {
            WorldLockingManager.GetInstance().LockedFromPlayspace = Pose.identity;
        }

        SetupColors();
    }

    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        CaptureMaterials();
        SetupColors();
        SetColor("_Color", nofocColor);
    }

    private void CaptureMaterials()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"Got {renderers.Length} renderers from {name}");
        foreach (var rend in renderers)
        {
            materials.Add(rend.material);
            Debug.Log($"Got {rend.material.name} from {rend.name}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFocusOn()
    {
        SetColor("_Color", focusColor);
    }

    public void OnFocusOff()
    {
        SetColor("_Color", nofocColor);
    }

    public void OnSelect()
    {
        ToggleSelected();
        SetColor("_Color", focusColor);
    }

    private void SetColor(string paramName, Color color)
    {
        foreach (var mat in materials)
        {
            mat.SetColor(paramName, color);
        }
    }
}
#endif