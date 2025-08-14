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
    public interface ISettingHelper
    {
        int Count
        {
            get;
        }

        bool Load();

        bool Save();

        string[] GetAllSettingNames();

        void GetAllSettingNames(List<string> results);

        bool HasSetting(string settingName);

        bool RemoveSetting(string settingName);

        void RemoveAllSettings();

        bool GetBool(string settingName);

        bool GetBool(string settingName, bool defaultValue);

        void SetBool(string settingName, bool value);

        int GetInt(string settingName);

        int GetInt(string settingName, int defaultValue);

        void SetInt(string settingName, int value);

        float GetFloat(string settingName);

        float GetFloat(string settingName, float defaultValue);

        void SetFloat(string settingName, float value);

        string GetString(string settingName);

        string GetString(string settingName, string defaultValue);

        void SetString(string settingName, string value);

        T GetObject<T>(string settingName);

        object GetObject(Type objectType, string settingName);

        T GetObject<T>(string settingName, T defaultObj);

        object GetObject(Type objectType, string settingName, object defaultObj);

        void SetObject<T>(string settingName, T obj);

        void SetObject(string settingName, object obj);
    }
}
