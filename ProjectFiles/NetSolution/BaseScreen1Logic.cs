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
using FTOptix.Alarm;
using FTOptix.SerialPort;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.EventLogger;
#endregion

public class BaseScreen1Logic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void RemoveLinkAndSetDefaultValue(NodeId targetVariable)
    {
        if (InformationModel.GetVariable(targetVariable) is IUAVariable targetVariableNode && targetVariableNode.Owner is Item ownerUIItem)
        {
            targetVariableNode.ResetDynamicLink();
            if (ownerUIItem.Prototype.GetVariable(targetVariableNode.BrowseName) is IUAVariable prototypeTargetVariableNode)
            {
                targetVariableNode.Value = prototypeTargetVariableNode.Value;
            }
            else
            {
                Log.Warning("Unable to find property variable in the prototype to restore the default value");
            }
        }
    }

    [ExportMethod]
    public void RestoreDynamicLink(NodeId targetVariable, NodeId sourceVariable)
    {
        if (InformationModel.GetVariable(targetVariable) is IUAVariable targetVariableNode && targetVariableNode.Refs.GetNode(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink) == null && InformationModel.GetVariable(sourceVariable) is IUAVariable sourceVariableNode)
        {
            targetVariableNode.SetDynamicLink(sourceVariableNode);
        }
    }
}
