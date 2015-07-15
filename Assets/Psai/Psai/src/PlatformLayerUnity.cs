//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace psai.net
{
    class PlatformLayerUnity : IPlatformLayer
    {
        public static readonly string NAME_OF_CHANNELS_CHILDNODE = "psaiChannels";

        private static GameObject s_psaiObject;
        public static GameObject PsaiGameObject
        {
            get
            {
                if (s_psaiObject == null)
                {
                    #if (!PSAI_STANDALONE && !PSAI_BUILT_BY_VS)
                        s_psaiObject = PsaiCoreManager.Instance.gameObject;
                    #endif

                }
                return s_psaiObject;
            }
        }

        Stream m_stream;

        GameObject _psaiChannelsNode;

        void IPlatformLayer.Initialize()
        {
            _psaiChannelsNode = new GameObject();
            _psaiChannelsNode.name = NAME_OF_CHANNELS_CHILDNODE;
            _psaiChannelsNode.transform.parent = PsaiGameObject.transform;
        }

        void IPlatformLayer.Release()
        {
            if (_psaiChannelsNode != null && _psaiChannelsNode.name.Equals(NAME_OF_CHANNELS_CHILDNODE))
            {
                GameObject.DestroyImmediate(_psaiChannelsNode);
            }
        }

        private Stream GetStreamOnPsaiCoreBinary(TextAsset textAsset)
        {
            if (textAsset != null)
            {
                m_stream = new System.IO.MemoryStream(textAsset.bytes);
                //m_stream = new System.IO.MemoryStream(textAsset.text);
                return m_stream;
            }
            else
            {
                return null;
            }
        }


        public string ConvertFilePathForPlatform(string originalPath)
        {
            string cleanedPath = originalPath.Replace('\\', '/');     // Path.Combine does not work for the Unity Resources Folder for some reason. The slash / seems to work for all platforms.                                   
            string filepathWithoutExtension = "";                       
            
            if (cleanedPath.Contains("/"))
            {
                filepathWithoutExtension = Path.GetDirectoryName(cleanedPath) + "/" + Path.GetFileNameWithoutExtension(cleanedPath);
            }
            else
            {
                filepathWithoutExtension = Path.GetFileNameWithoutExtension(cleanedPath);       // Resources.Load() does not work with file extensions
            }

            return filepathWithoutExtension;
        }

        public Stream GetStreamOnPsaiCoreBinary(string fullFilePathWithinResourcesDir)
        {
            #if !(PSAI_NOLOG)
            {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                string oss = "PlatformLayerUnity::GetStreamOnPsaiCoreBinary(): trying to load " + fullFilePathWithinResourcesDir;
	                Logger.Instance.Log(oss, LogLevel.info);
                }
            }
            #endif

            string cleanedPath = ConvertFilePathForPlatform(fullFilePathWithinResourcesDir);

            #if !(PSAI_NOLOG)
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
                	Logger.Instance.Log("Trying to load '" + cleanedPath + "' from Resources.", LogLevel.info);    
                }
            #endif
            
            TextAsset textAsset = new TextAsset();
            textAsset = (TextAsset)Resources.Load(cleanedPath, typeof(TextAsset));

            return GetStreamOnPsaiCoreBinary(textAsset);           
        }
    }
}
