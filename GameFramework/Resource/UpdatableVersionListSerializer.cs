//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class UpdatableVersionListSerializer : GameFrameworkSerializer<UpdatableVersionList>
    {
        private static readonly byte[] Header = new byte[] { (byte)'G', (byte)'F', (byte)'U' };

        public UpdatableVersionListSerializer()
        {
        }

        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
