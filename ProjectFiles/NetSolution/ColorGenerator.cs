#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.WebUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.Core;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics.Tracing;
using FTOptix.Alarm;
using FTOptix.SerialPort;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.EventLogger;
#endregion

public class ColorGenerator : BaseNetLogic
{
    public override void Start()
    {
        var nodePointer = (NodePointer)Project.Current.Get("UI/Screens/Screen1/NodePointer1");
        var targetObject = Project.Current.GetObject("Model/Object1");
        if (nodePointer != null && targetObject != null)
        {
            nodePointer.Value = targetObject.NodeId;
        }
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void GenerateFromHexCode(string hexBaseColor)
    {
        string _baseColor = hexBaseColor;
        if (Regex.IsMatch(_baseColor, @"^([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$"))
        {
            if (Regex.IsMatch(_baseColor, @"^[0-9A-Fa-f]{8}$"))
            {
                _baseColor = _baseColor.Substring(0,_baseColor.Length-2);
            }
            var red = Convert.ToByte(_baseColor.Substring(0,2), 16);
            var green = Convert.ToByte(_baseColor.Substring(2,2), 16);
            var blue = Convert.ToByte(_baseColor.Substring(4,2), 16);
            LogicObject.GetVariable("Color10").Value = new Color((byte)(10/100.0 * 255),red,green,blue);
            LogicObject.GetVariable("Color25").Value = new Color((byte)(25/100.0 * 255),red,green,blue);
            LogicObject.GetVariable("Color50").Value = new Color((byte)(50/100.0 * 255),red,green,blue);
            LogicObject.GetVariable("Color75").Value = new Color((byte)(75/100.0 * 255),red,green,blue);
            LogicObject.GetVariable("Color100").Value = new Color(255,red,green,blue);     
        }
    }
}
