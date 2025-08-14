//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        [StructLayout(LayoutKind.Auto)]
        private struct ResourceName : IComparable, IComparable<ResourceName>, IEquatable<ResourceName>
        {
            private static readonly Dictionary<ResourceName, string> s_ResourceFullNames = new Dictionary<ResourceName, string>();

            private readonly string mName;
            private readonly string mVariant;
            private readonly string mExtension;

            public ResourceName(string name, string variant, string extension)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Resource name is invalid.");
                }

                if (string.IsNullOrEmpty(extension))
                {
                    throw new GameFrameworkException("Resource extension is invalid.");
                }

                mName = name;
                mVariant = variant;
                mExtension = extension;
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public string Variant
            {
                get
                {
                    return mVariant;
                }
            }

            public string Extension
            {
                get
                {
                    return mExtension;
                }
            }

            public string FullName
            {
                get
                {
                    string fullName = null;
                    if (s_ResourceFullNames.TryGetValue(this, out fullName))
                    {
                        return fullName;
                    }

                    fullName = mVariant != null ? Utility.Text.Format("{0}.{1}.{2}", mName, mVariant, mExtension) : Utility.Text.Format("{0}.{1}", mName, mExtension);
                    s_ResourceFullNames.Add(this, fullName);
                    return fullName;
                }
            }

            public override string ToString()
            {
                return FullName;
            }

            public override int GetHashCode()
            {
                if (mVariant == null)
                {
                    return mName.GetHashCode() ^ mExtension.GetHashCode();
                }

                return mName.GetHashCode() ^ mVariant.GetHashCode() ^ mExtension.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return (obj is ResourceName) && Equals((ResourceName)obj);
            }

            public bool Equals(ResourceName value)
            {
                return string.Equals(mName, value.mName, StringComparison.Ordinal) && string.Equals(mVariant, value.mVariant, StringComparison.Ordinal) && string.Equals(mExtension, value.mExtension, StringComparison.Ordinal);
            }

            public static bool operator ==(ResourceName a, ResourceName b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(ResourceName a, ResourceName b)
            {
                return !(a == b);
            }

            public int CompareTo(object value)
            {
                if (value == null)
                {
                    return 1;
                }

                if (!(value is ResourceName))
                {
                    throw new GameFrameworkException("Type of value is invalid.");
                }

                return CompareTo((ResourceName)value);
            }

            public int CompareTo(ResourceName resourceName)
            {
                int result = string.CompareOrdinal(mName, resourceName.mName);
                if (result != 0)
                {
                    return result;
                }

                result = string.CompareOrdinal(mVariant, resourceName.mVariant);
                if (result != 0)
                {
                    return result;
                }

                return string.CompareOrdinal(mExtension, resourceName.mExtension);
            }
        }
    }
}
