using CustomBuilder;
using BJSYGameCore;
using System;
using UnityEngine;
using UnityEditor;
namespace TouhouHeartstone
{
    [BuilderStep(-2)]
    public class CustomBuildPipeline: IBuilderStep
    {
        public void ActionAfter()
        {
            // do nothing yet.
        }

        public void ActionBefore(object option)
        {
            onUpdateBuildInformationObject();
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