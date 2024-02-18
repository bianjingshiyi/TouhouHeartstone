using CustomBuilder;
using Game;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TouhouHeartstone
{
    [BuilderStep(4)]
    public class SteamBuildStep : IBuilderStep
    {
        string savedSymbol;
        bool shouldRestore = false;
        void IBuilderStep.ActionAfter()
        {
            if (shouldRestore)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, savedSymbol);
            }
        }

        void IBuilderStep.ActionBefore(object opt)
        {
            var options = (BuildOptions)opt;

            var isSteam = Environment.GetEnvironmentVariable("STEAM_BUILD");
            if (!string.IsNullOrWhiteSpace(isSteam))
            {
                shouldRestore = true;

                savedSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                var symbol = savedSymbol + ";STEAM_BUILD";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
            }
        }
    }
}