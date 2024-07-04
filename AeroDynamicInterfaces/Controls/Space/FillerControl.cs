using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Space;

public class FillerControl : Control
{
    private int _pixels;
    
    /// <summary>
    /// Construct a control which will fill an empty space
    /// </summary>
    /// <param name="id">The control's identifier</param>
    /// <param name="pixels">How many pixels it will take, 0 for it to dynamically use up space</param>
    public FillerControl(int id, int pixels = 0) : base(id)
    {
        _pixels = pixels;
    }

    public override void Draw()
    {
        if (_pixels == 0)
            GUILayout.FlexibleSpace();
        else
            GUILayout.Space(_pixels);
    }
}