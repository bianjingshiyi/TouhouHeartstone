using CustomBuilder;
using System;
using System.Linq;
using UnityEditor;

namespace TouhouHeartstone
{
    [BuilderStep(4)]
    public class AdditionalFlagsBuildStep : IBuilderStep
    {
        string savedProductName, savedAndroidID, savedSymbol;
        bool symbolChanged = false;
        bool productChanged = false;
        void IBuilderStep.ActionAfter()
        {
            if (productChanged)
            {
                PlayerSettings.productName = savedProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, savedAndroidID);
            }
            if (symbolChanged)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, savedSymbol);
            }
        }

        void IBuilderStep.ActionBefore(object opt)
        {
            var flags = Environment.GetEnvironmentVariable("ADDITIONAL_BUILD_FLAGS");
            var symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (!string.IsNullOrWhiteSpace(flags))
            {
                savedSymbol = symbol;
                if (!flags.StartsWith(';'))
                    flags = ';' + flags;
                symbol = savedSymbol + flags;
                symbolChanged = true;
            }

            var symbolList = symbol.Split(';');
            if (symbolList.Contains("SNAPSHOT_BUILD"))
            {
                savedProductName = PlayerSettings.productName;
                savedAndroidID = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
                productChanged = true;

                PlayerSettings.productName = $"{savedProductName}Snapshot";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, savedAndroidID + ".snapshot");
            }

            if (symbolChanged)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
            }
        }
    }
}