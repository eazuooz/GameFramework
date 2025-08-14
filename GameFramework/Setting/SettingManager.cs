//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.Setting
{
    internal sealed class SettingManager : GameFrameworkModule, ISettingManager
    {
        private ISettingHelper mSettingHelper;

        public SettingManager()
        {
            mSettingHelper = null;
        }

        public int Count
        {
            get
            {
                if (mSettingHelper == null)
                {
                    throw new GameFrameworkException("Setting helper is invalid.");
                }

                return mSettingHelper.Count;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            Save();
        }

        public void SetSettingHelper(ISettingHelper settingHelper)
        {
            if (settingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            mSettingHelper = settingHelper;
        }

        public bool Load()
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            return mSettingHelper.Load();
        }

        public bool Save()
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            return mSettingHelper.Save();
        }

        public string[] GetAllSettingNames()
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            return mSettingHelper.GetAllSettingNames();
        }

        public void GetAllSettingNames(List<string> results)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            mSettingHelper.GetAllSettingNames(results);
        }

        public bool HasSetting(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.HasSetting(settingName);
        }

        public bool RemoveSetting(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.RemoveSetting(settingName);
        }

        public void RemoveAllSettings()
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            mSettingHelper.RemoveAllSettings();
        }

        public bool GetBool(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetBool(settingName);
        }

        public bool GetBool(string settingName, bool defaultValue)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetBool(settingName, defaultValue);
        }

        public void SetBool(string settingName, bool value)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            mSettingHelper.SetBool(settingName, value);
        }

        public int GetInt(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetInt(settingName);
        }

        public int GetInt(string settingName, int defaultValue)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetInt(settingName, defaultValue);
        }

        public void SetInt(string settingName, int value)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            mSettingHelper.SetInt(settingName, value);
        }

        public float GetFloat(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetFloat(settingName);
        }

        public float GetFloat(string settingName, float defaultValue)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetFloat(settingName, defaultValue);
        }

        public void SetFloat(string settingName, float value)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            mSettingHelper.SetFloat(settingName, value);
        }

        public string GetString(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetString(settingName);
        }

        public string GetString(string settingName, string defaultValue)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetString(settingName, defaultValue);
        }

        public void SetString(string settingName, string value)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            mSettingHelper.SetString(settingName, value);
        }

        public T GetObject<T>(string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetObject<T>(settingName);
        }

        public object GetObject(Type objectType, string settingName)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (objectType == null)
            {
                throw new GameFrameworkException("Object type is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetObject(objectType, settingName);
        }

        public T GetObject<T>(string settingName, T defaultObj)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetObject(settingName, defaultObj);
        }

        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (objectType == null)
            {
                throw new GameFrameworkException("Object type is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            return mSettingHelper.GetObject(objectType, settingName, defaultObj);
        }

        public void SetObject<T>(string settingName, T obj)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            mSettingHelper.SetObject(settingName, obj);
        }

        public void SetObject(string settingName, object obj)
        {
            if (mSettingHelper == null)
            {
                throw new GameFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new GameFrameworkException("Setting name is invalid.");
            }

            mSettingHelper.SetObject(settingName, obj);
        }
    }
}
