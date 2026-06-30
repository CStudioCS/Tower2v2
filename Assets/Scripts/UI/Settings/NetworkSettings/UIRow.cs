using System;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// A wrapper class to allow Unity's Inspector to display a 2D list (list of lists) of Selectables.
/// Represents a single horizontal row in the UI navigation grid.
/// </summary>
[Serializable]
public class UIRow
{
    public List<Selectable> items = new List<Selectable>();

    public UIRow() { }

    public UIRow(IEnumerable<Selectable> selectables)
    {
        items.AddRange(selectables);
    }
}