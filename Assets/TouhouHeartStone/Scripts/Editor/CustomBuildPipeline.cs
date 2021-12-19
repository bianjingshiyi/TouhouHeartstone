using CustomBuilder.Editor;
using BJSYGameCore;
using System;
using UnityEngine;
using UnityEditor;
namespace TouhouHeartstone
{
    [InitializeOnLoad]
    public class CustomBuildPipeline
    {
        static CustomBuildPipeline()
        {
            BuilderSteps.onUpdateBuildInformationObject += onUpdateBuildInformationObject;
            Debug.Log("自定义构建管线初始化完成");
        }
        private static void onUpdateBuildInformationObject()
        {
            StreamingAssetsInfo streamingAssetsInfo;
            try
            {
                streamingAssetsInfo = StreamingAssetInfoHelper.updateStreamingAssetsInfo();
            }
            catch (Exception e)
            {
                Debug.LogError("更新流文件信息发生异常：" + e);
                return;
            }
            Debug.Log("更新流文件信息成功", streamingAssetsInfo);
        }
    }
}