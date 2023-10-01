using CustomBuilder;
using System;
using UnityEditor;

namespace TouhouHeartstone
{
    [BuilderStep(4)]
    public class SnapshotBuildStep : IBuilderStep
    {
        string savedProductName, savedAndroidID, savedSymbol;
        bool shouldRestore = false;
        void IBuilderStep.ActionAfter()
        {
            if (shouldRestore)
            {
                PlayerSettings.productName = savedProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, savedAndroidID);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, savedSymbol);
            }
        }

        void IBuilderStep.ActionBefore(object opt)
        {
            var isSnapshot = Environment.GetEnvironmentVariable("SNAPSHOT_BUILD");
            if (!string.IsNullOrWhiteSpace(isSnapshot))
            {
                savedProductName = PlayerSettings.productName;
                savedAndroidID = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
                shouldRestore = true;

                PlayerSettings.productName = savedProductName + "快照";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, savedAndroidID + ".snapshot");
                
                savedSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                var symbol = savedSymbol + ";SNAPSHOT_BUILD";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
            }
        }
    }
}