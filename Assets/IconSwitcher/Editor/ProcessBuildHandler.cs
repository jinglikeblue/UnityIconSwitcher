using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;

namespace IconSwitch
{
    /// <summary>
    /// 构建流程处理器
    /// </summary>
    public class ProcessBuildHandler : IPreprocessBuildWithReport
    {
        // [MenuItem("Test/AndroidProcess")]
        // static void TestAndroidProcess()
        // {
        //     string path = "D:/LMD/SV/_OtherProjects/IconSwitchDemo/NativeProjects/Android";
        //     ProcessProject(BuildTarget.Android, path);
        // }

        /// <summary>
        /// 是否激活该功能
        /// </summary>
        public static readonly bool IsActive = true;

        private static readonly string LibLauncherManifestXmlPath = "Assets/Plugins/Android/LauncherManifest.xml";

        private static readonly string LibAndroidManifestXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";

        private static readonly string IconSwitcherRootPath = Application.dataPath + "/IconSwitcher";

        private static readonly string IconSwitchTempLibFolder = $"{IconSwitcherRootPath}/Plugins/Android/IconSwitch.androidlib/res";

        private static string AndroidManifestXmlPath
        {
            get
            {
                string xmlPath = null;
                if (File.Exists(LibLauncherManifestXmlPath))
                {
                    xmlPath = LibLauncherManifestXmlPath;
                }
                else if (File.Exists(LibAndroidManifestXmlPath))
                {
                    xmlPath = LibAndroidManifestXmlPath;
                }

                return xmlPath;
            }
        }

        public int callbackOrder { get; }

        /// <summary>
        /// 是否是直接构建Android包体
        /// </summary>
        public static bool IsBuildAndroidAppPackage => BuildTarget.Android == EditorUserBuildSettings.activeBuildTarget && false == EditorUserBuildSettings.exportAsGoogleAndroidProject;

        /// <summary>
        /// 打包前处理
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            if (false == IsActive) return;

            if (IsBuildAndroidAppPackage)
            {
                BeforeAndroidAppPackageBuild();
            }
        }
        
        /// <summary>
        /// 构建Project工程后处理
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        [PostProcessBuild]
        private static void Process(BuildTarget target, string path)
        {
            if (false == IsActive) return;

            if (IsBuildAndroidAppPackage)
            {
                Debug.Log($"[Icon Switcher][打包结束] 构建Android包体之后的处理");
                AfterAndroidAppPackageBuild();
                AssetDatabase.Refresh();
                return;
            }

            Debug.Log($"[Icon Switcher][打包结束][工程目录处理] BuildTarget: {target} , Path: {path}");
            try
            {
                switch (target)
                {
                    case BuildTarget.iOS:
                        ProcessIOSProject(target, path);
                        break;
                    case BuildTarget.Android:
                        ProcessAndroidProject(target, path);
                        break;
                    default:
                        Debug.Log($"[Icon Switcher][打包结束][工程目录处理] 不支持的平台 BuildTarget: {target} , Path: {path}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Icon Switcher][打包结束][工程目录处理] 出错!");
                Debug.LogError(e);
            }
        }

        private static string[] GetIconNames()
        {
            return IconSwicher.IconNameConfigs;
            return File.ReadAllLines(IconSwitcherRootPath + "/icon_config.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string StandardizeSeparatorChar(string input)
        {
            var output = input.Replace('\\', '/');
            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ChangeFileName(string filePath, string newName)
        {
            var ext = Path.GetExtension(filePath);
            var newFileName = $"{newName}{ext}";
            var oldFileName = Path.GetFileName(filePath);
            var newFilePath = filePath.Replace(oldFileName, newFileName);
            return newFilePath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CopyTo(string sourcePath, string destPath)
        {
            Debug.Log($"[FileCopy] From: {sourcePath} To: {destPath}");
            try
            {
                var destFolder = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }

                File.Copy(sourcePath, destPath, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 处理Android构建的工程
        /// </summary>
        /// <param name="target"></param>
        /// <param name="projectPath"></param>
        static void ProcessAndroidProject(BuildTarget target, string projectPath)
        {
            if (BuildTarget.Android != target)
            {
                return;
            }

            #region 拷贝图标

            string targetIconFolder = $"{projectPath}/launcher/src/main/res";
            CopyAndroidIcons(targetIconFolder);

            #endregion

            #region 配置AndroidManifest.xml

            string targetAndroidManifestFile = $"{projectPath}/launcher/src/main/AndroidManifest.xml";
            ModifyAndroidManifest(targetAndroidManifestFile);

            #endregion
        }

        #region Android 操作

        /// <summary>
        /// 构建Android包体之前的处理
        /// </summary>
        static void BeforeAndroidAppPackageBuild()
        {
            if (AndroidManifestXmlPath != null)
            {
                ModifyAndroidManifest(AndroidManifestXmlPath);
                CopyAndroidIcons(IconSwitchTempLibFolder);
            }
            else
            {
                Debug.LogError($"[Icon Switcher] 无法找到AndroidManifest.xml文件！配置失败!");
            }
        }

        /// <summary>
        /// 构建Android包体之后的处理
        /// </summary>
        static void AfterAndroidAppPackageBuild()
        {
            #region 清理AndroidManifest中的配置

            if (AndroidManifestXmlPath != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AndroidManifestXmlPath);
                XmlNodeList activityAliasNodes = doc.SelectNodes("//activity-alias");
                foreach (XmlNode node in activityAliasNodes)
                {
                    node.ParentNode.RemoveChild(node);
                }

                // 保存修改后的 XML 文件
                doc.Save(AndroidManifestXmlPath);
            }

            #endregion

            #region 删除临时res目录

            if (Directory.Exists(IconSwitchTempLibFolder))
            {
                Directory.Delete(IconSwitchTempLibFolder, true);
            }

            #endregion
        }

        
        /// <summary>
        /// 拷贝Android使用的Icon
        /// </summary>
        /// <param name="targetIconFolder"></param>
        private static void CopyAndroidIcons(string targetIconFolder)
        {
            var iconNames = GetIconNames();
            foreach (var iconName in iconNames)
            {
                string iconSourceFolder = $"{IconSwitcherRootPath}/icons/{iconName}/android";
                if (!Directory.Exists(iconSourceFolder))
                {
                    Debug.Log($"不存在的目录: {iconSourceFolder}");
                    continue;
                }

                var iconSourceFiles = Directory.GetFiles(iconSourceFolder, "*.png", SearchOption.AllDirectories);

                foreach (var tempFile in iconSourceFiles)
                {
                    var sourceFile = StandardizeSeparatorChar(tempFile);
                    var targetFileName = sourceFile.Replace(iconSourceFolder, "");


                    string targetFile = $"{targetIconFolder}{ChangeFileName(targetFileName, iconName)}";
                    CopyTo(sourceFile, targetFile);
                }

                Debug.Log(iconName);
            }
        }

        /// <summary>
        /// 修改AndroidManifest.xml
        /// </summary>
        /// <param name="targetAndroidManifestFile"></param>
        private static void ModifyAndroidManifest(string targetAndroidManifestFile)
        {
            var iconNames = GetIconNames();

            XmlDocument doc = new XmlDocument();
            doc.Load(targetAndroidManifestFile);
            XmlNodeList activityAliasNodes = doc.SelectNodes("//activity-alias");
            foreach (XmlNode node in activityAliasNodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            XmlNode applicationNode = doc.SelectSingleNode("/manifest/application");

            XmlDocument aliasTemplateXML = new XmlDocument();
            aliasTemplateXML.Load($"{IconSwitcherRootPath}/Templates/alias_template.xml");
            var aliasTemplateNodeXML = aliasTemplateXML.SelectSingleNode("/manifest/application/activity-alias");

            foreach (var iconName in iconNames)
            {
                XmlAttribute iconAttribute = aliasTemplateNodeXML.Attributes["android:icon"];
                iconAttribute.Value = $"@mipmap/{iconName}";

                XmlAttribute nameAttribute = aliasTemplateNodeXML.Attributes["android:name"];
                if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                {
                    nameAttribute.Value = $".{iconName}_Activity";
                }
                else
                {
                    nameAttribute.Value = $"{PlayerSettings.applicationIdentifier}.{iconName}_Activity";
                }

                XmlNode importNode = doc.ImportNode(aliasTemplateNodeXML, true);
                applicationNode.AppendChild(importNode);
            }

            // 保存修改后的 XML 文件
            doc.Save(targetAndroidManifestFile);
        }

        #endregion
        
        /// <summary>
        /// 处理iOS构建的XCode工程
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        static void ProcessIOSProject(BuildTarget target, string path)
        {
            var iconNames = GetIconNames();
            if (0 == iconNames.Length)
            {
                return;
            }
#if UNITY_IOS
            var pbxProjectPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);
            var pbx = new UnityEditor.iOS.Xcode.PBXProject();
            pbx.ReadFromString(File.ReadAllText(pbxProjectPath));

            foreach (var iconName in iconNames)
            {
                string iconSourceFolder = $"{IconSwitcherRootPath}/icons/{iconName}/ios/AppIcon.appiconset";

                if (!Directory.Exists(iconSourceFolder))
                {
                    Debug.Log($"[Icon Switcher] 不存在的图标路径: {iconSourceFolder}");
                    continue;
                }

                var name = Path.GetFileName(iconSourceFolder);
                var targetFolder = $"{path}/Unity-iPhone/Images.xcassets/{iconName}.appiconset";

                Debug.Log($"[FileCopy] From: {iconSourceFolder} To: {targetFolder}");


                var iconSourceFiles = Directory.GetFiles(iconSourceFolder, "*", SearchOption.AllDirectories);

                foreach (var tempFile in iconSourceFiles)
                {
                    var sourceFile = StandardizeSeparatorChar(tempFile);
                    var targetFileName = Path.GetFileName(sourceFile);
                    string targetFile = $"{targetFolder}/{targetFileName}";
                    CopyTo(sourceFile, targetFile);
                }

                // CopyTo()
                // FileUtility.CopyDir(appIconSet, targetPath);
            }

            //将工程Build Settings中 Include all app icon assets改为 YES
            pbx.SetBuildProperty(pbx.GetUnityMainTargetGuid(), "ASSETCATALOG_COMPILER_INCLUDE_ALL_APPICON_ASSETS", "YES");
            File.WriteAllText(pbxProjectPath, pbx.WriteToString());
#endif
        }
    }
}