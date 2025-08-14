//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace GameFramework.Config
{
    internal sealed partial class ConfigManager : GameFrameworkModule, IConfigManager
    {
        private readonly Dictionary<string, ConfigData> mConfigDatas;
        private readonly DataProvider<IConfigManager> mDataProvider;
        private IConfigHelper mConfigHelper;

        public ConfigManager()
        {
            mConfigDatas = new Dictionary<string, ConfigData>(StringComparer.Ordinal);
            mDataProvider = new DataProvider<IConfigManager>(this);
            mConfigHelper = null;
        }

        public int Count
        {
            get
            {
                return mConfigDatas.Count;
            }
        }

        public int CachedBytesSize
        {
            get
            {
                return DataProvider<IConfigManager>.CachedBytesSize;
            }
        }

        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                mDataProvider.ReadDataSuccess += value;
            }
            remove
            {
                mDataProvider.ReadDataSuccess -= value;
            }
        }

        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                mDataProvider.ReadDataFailure += value;
            }
            remove
            {
                mDataProvider.ReadDataFailure -= value;
            }
        }

        public event EventHandler<ReadDataUpdateEventArgs> ReadDataUpdate
        {
            add
            {
                mDataProvider.ReadDataUpdate += value;
            }
            remove
            {
                mDataProvider.ReadDataUpdate -= value;
            }
        }

        public event EventHandler<ReadDataDependencyAssetEventArgs> ReadDataDependencyAsset
        {
            add
            {
                mDataProvider.ReadDataDependencyAsset += value;
            }
            remove
            {
                mDataProvider.ReadDataDependencyAsset -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        public void SetResourceManager(IResourceManager resourceManager)
        {
            mDataProvider.SetResourceManager(resourceManager);
        }

        public void SetDataProviderHelper(IDataProviderHelper<IConfigManager> dataProviderHelper)
        {
            mDataProvider.SetDataProviderHelper(dataProviderHelper);
        }

        public void SetConfigHelper(IConfigHelper configHelper)
        {
            if (configHelper == null)
            {
                throw new GameFrameworkException("Config helper is invalid.");
            }

            mConfigHelper = configHelper;
        }

        public void EnsureCachedBytesSize(int ensureSize)
        {
            DataProvider<IConfigManager>.EnsureCachedBytesSize(ensureSize);
        }

        public void FreeCachedBytes()
        {
            DataProvider<IConfigManager>.FreeCachedBytes();
        }

        public void ReadData(string configAssetName)
        {
            mDataProvider.ReadData(configAssetName);
        }

        public void ReadData(string configAssetName, int priority)
        {
            mDataProvider.ReadData(configAssetName, priority);
        }

        public void ReadData(string configAssetName, object userData)
        {
            mDataProvider.ReadData(configAssetName, userData);
        }

        public void ReadData(string configAssetName, int priority, object userData)
        {
            mDataProvider.ReadData(configAssetName, priority, userData);
        }

        public bool ParseData(string configString)
        {
            return mDataProvider.ParseData(configString);
        }

        public bool ParseData(string configString, object userData)
        {
            return mDataProvider.ParseData(configString, userData);
        }

        public bool ParseData(byte[] configBytes)
        {
            return mDataProvider.ParseData(configBytes);
        }

        public bool ParseData(byte[] configBytes, object userData)
        {
            return mDataProvider.ParseData(configBytes, userData);
        }

        public bool ParseData(byte[] configBytes, int startIndex, int length)
        {
            return mDataProvider.ParseData(configBytes, startIndex, length);
        }

        public bool ParseData(byte[] configBytes, int startIndex, int length, object userData)
        {
            return mDataProvider.ParseData(configBytes, startIndex, length, userData);
        }

        public bool HasConfig(string configName)
        {
            return GetConfigData(configName).HasValue;
        }

        public bool GetBool(string configName)
        {
            ConfigData? configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.BoolValue;
        }

        public bool GetBool(string configName, bool defaultValue)
        {
            ConfigData? configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.BoolValue : defaultValue;
        }

        public int GetInt(string configName)
        {
            ConfigData? configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.IntValue;
        }

        public int GetInt(string configName, int defaultValue)
        {
            ConfigData? configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.IntValue : defaultValue;
        }

        public float GetFloat(string configName)
        {
            ConfigData? configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.FloatValue;
        }

        public float GetFloat(string configName, float defaultValue)
        {
            ConfigData? configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.FloatValue : defaultValue;
        }

        public string GetString(string configName)
        {
            ConfigData? configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.StringValue;
        }

        public string GetString(string configName, string defaultValue)
        {
            ConfigData? configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.StringValue : defaultValue;
        }

        public bool AddConfig(string configName, string configValue)
        {
            bool boolValue = false;
            bool.TryParse(configValue, out boolValue);

            int intValue = 0;
            int.TryParse(configValue, out intValue);

            float floatValue = 0f;
            float.TryParse(configValue, out floatValue);

            return AddConfig(configName, boolValue, intValue, floatValue, configValue);
        }

        public bool AddConfig(string configName, bool boolValue, int intValue, float floatValue, string stringValue)
        {
            if (HasConfig(configName))
            {
                return false;
            }

            mConfigDatas.Add(configName, new ConfigData(boolValue, intValue, floatValue, stringValue));
            return true;
        }

        public bool RemoveConfig(string configName)
        {
            if (!HasConfig(configName))
            {
                return false;
            }

            return mConfigDatas.Remove(configName);
        }

        public void RemoveAllConfigs()
        {
            mConfigDatas.Clear();
        }

        private ConfigData? GetConfigData(string configName)
        {
            if (string.IsNullOrEmpty(configName))
            {
                throw new GameFrameworkException("Config name is invalid.");
            }

            ConfigData configData = default(ConfigData);
            if (mConfigDatas.TryGetValue(configName, out configData))
            {
                return configData;
            }

            return null;
        }
    }
}
